using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Nodes;
using Hexa.NET.ImGui;
using Color = SharpDX.Color;
using RectangleF = SharpDX.RectangleF;

namespace AutoFlask
{
    public class AutoFlask : BaseSettingsPlugin<AutoFlaskSettings>
    {
        private readonly Stopwatch _flask1Timer = new Stopwatch();
        private readonly Stopwatch _flask2Timer = new Stopwatch();
        private readonly Stopwatch _flask3Timer = new Stopwatch();
        private readonly Stopwatch _flask4Timer = new Stopwatch();
        private readonly Stopwatch _flask5Timer = new Stopwatch();

        private readonly Stopwatch _skill1Timer = new Stopwatch();
        private readonly Stopwatch _skill2Timer = new Stopwatch();
        private readonly Stopwatch _skill3Timer = new Stopwatch();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private Entity _currentTarget;
        private readonly Dictionary<string, Stopwatch> _areaTimers = new Dictionary<string, Stopwatch>();
        private string _currentAreaName;

        private static readonly Keys[] CommonKeys =
        {
            Keys.None,
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
            Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T,
            Keys.A, Keys.S, Keys.D, Keys.F,
            Keys.Space, Keys.PageDown, Keys.PageUp,
            Keys.XButton1, Keys.XButton2
        };

        private static readonly string[] CommonKeyLabels =
        {
            "None",
            "1", "2", "3", "4", "5",
            "Q", "W", "E", "R", "T",
            "A", "S", "D", "F",
            "Space", "PageDown", "PageUp",
            "Mouse4", "Mouse5"
        };

        private static readonly string[] FlaskTypes = { "Auto", "HP", "Mana", "Utility" };

        public override bool Initialise()
        {
            Name = "Auto Flask & Triple Skill Bot";

            _flask1Timer.Start();
            _flask2Timer.Start();
            _flask3Timer.Start();
            _flask4Timer.Start();
            _flask5Timer.Start();

            _skill1Timer.Start();
            _skill2Timer.Start();
            _skill3Timer.Start();

            // Auto-migrate legacy inverted layout where slot 1 was Mana and slot 5 was HP
            if (Settings.Flask1Type.Value == "Mana" && Settings.Flask5Type.Value == "HP")
            {
                Settings.Flask1Type.Value = "HP";
                Settings.Flask1Percentage.Value = 45;
                Settings.Flask1UseWhenAvailable.Value = false;
                Settings.Flask5Type.Value = "Utility";
                Settings.Flask5UseWhenAvailable.Value = true;
            }

            // Register Master Toggle Hotkey
            Input.RegisterKey(Settings.ToggleAutoFlask.Value.Key);
            Settings.ToggleAutoFlask.OnValueChanged += () => Input.RegisterKey(Settings.ToggleAutoFlask.Value.Key);

            return true;
        }

        public override Job Tick()
        {
            // Handle Toggle Hotkey
            if (Settings.ToggleAutoFlask.PressedOnce())
            {
                Settings.IsAutoFlaskEnabled.Value = !Settings.IsAutoFlaskEnabled.Value;
                DebugWindow.LogMsg(
                    $"[AutoFlask] Active: {Settings.IsAutoFlaskEnabled.Value}",
                    3,
                    Settings.IsAutoFlaskEnabled.Value ? Color.LimeGreen : Color.Orange);
            }

            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value)
                return base.Tick();

            if (!GameController.InGame || GameController.IsLoading)
            {
                PauseCurrentAreaTimer();
                return base.Tick();
            }

            // Window Focus Safety Check
            if (Settings.OnlyRunWhenForeground.Value && !GameController.Window.IsForeground())
                return base.Tick();

            // Track area time
            TrackAreaTime();

            if (!GameController.Player.IsAlive)
                return base.Tick();

            // Town and Hideout Safety Check
            if (Settings.PauseInTownAndHideout.Value)
            {
                var area = GameController.Area.CurrentArea;
                if (area == null || area.IsHideout || area.IsTown)
                    return base.Tick();
            }

            HandleFlasks();
            HandleSkills();

            return base.Tick();
        }

        private void PauseCurrentAreaTimer()
        {
            if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var currentStopwatch))
            {
                currentStopwatch.Stop();
            }
        }

        private void TrackAreaTime()
        {
            var currentArea = GameController.Area.CurrentArea;
            if (currentArea == null) return;

            var areaName = currentArea.Name;
            if (areaName != _currentAreaName)
            {
                PauseCurrentAreaTimer();
                _currentAreaName = areaName;
            }

            if (!_areaTimers.TryGetValue(areaName, out var currentStopwatch))
            {
                currentStopwatch = new Stopwatch();
                _areaTimers[areaName] = currentStopwatch;
            }

            if (!currentStopwatch.IsRunning)
            {
                currentStopwatch.Start();
            }
        }

        #region Skills & Monster Tracing
        private bool TraceMonster(float maxDistance)
        {
            if (!GameController.Window.IsForeground())
            {
                _currentTarget = null;
                return false;
            }

            Entity bestTarget = null;
            int bestRarity = -1;
            float bestDistance = float.MaxValue;

            var entities = GameController.EntityListWrapper?.Entities;
            if (entities == null) return false;

            foreach (var e in entities)
            {
                if (e == null || e.Type != EntityType.Monster || !e.IsHostile || !e.IsAlive || !e.IsTargetable)
                    continue;

                float dist = e.DistancePlayer;
                if (dist > maxDistance)
                    continue;

                int rarity = (int)e.Rarity;
                if (rarity > bestRarity || (rarity == bestRarity && dist < bestDistance))
                {
                    bestTarget = e;
                    bestRarity = rarity;
                    bestDistance = dist;
                }
            }

            _currentTarget = bestTarget;
            if (_currentTarget != null)
            {
                var screenPos = GameController.IngameState.Camera.WorldToScreen(_currentTarget.PosNum);
                if (screenPos != Vector2.Zero)
                {
                    var windowRect = GameController.Window.GetWindowRectangle();
                    Input.SetCursorPos(screenPos + new Vector2(windowRect.Location.X, windowRect.Location.Y));
                    return true;
                }
            }
            return false;
        }

        private ActorSkill FindActorSkill(Keys key, bool preferVaal = false)
        {
            var actor = GameController.Player?.GetComponent<Actor>();
            if (actor == null) return null;

            int targetSlot = key switch
            {
                Keys.Q => 3,
                Keys.W => 4,
                Keys.E => 5,
                Keys.R => 6,
                Keys.T => 7,
                _ => -1
            };

            var allSkills = actor.ActorSkills;
            if (allSkills == null) return null;

            if (targetSlot >= 0)
            {
                var slotSkills = allSkills.Where(s => s.SkillSlotIndex == targetSlot).ToList();
                if (preferVaal)
                {
                    var vaalInSlot = slotSkills.FirstOrDefault(s => s.IsVaalSkill || (s.Name != null && s.Name.StartsWith("Vaal ")));
                    if (vaalInSlot != null) return vaalInSlot;

                    var baseInSlot = slotSkills.FirstOrDefault();
                    if (baseInSlot != null && !string.IsNullOrEmpty(baseInSlot.Name))
                    {
                        var vaalMatch = allSkills.FirstOrDefault(s =>
                            (s.IsVaalSkill || (s.Name != null && s.Name.StartsWith("Vaal "))) &&
                            (s.Name.Contains(baseInSlot.Name, StringComparison.OrdinalIgnoreCase) ||
                             (baseInSlot.InternalName != null && s.InternalName != null && s.InternalName.Contains(baseInSlot.InternalName, StringComparison.OrdinalIgnoreCase))));
                        if (vaalMatch != null) return vaalMatch;
                    }

                    var vaalOnBar = allSkills.FirstOrDefault(s => (s.IsVaalSkill || (s.Name != null && s.Name.StartsWith("Vaal "))) && s.IsOnSkillBar);
                    if (vaalOnBar != null) return vaalOnBar;

                    var anyVaal = allSkills.FirstOrDefault(s => s.IsVaalSkill || (s.Name != null && s.Name.StartsWith("Vaal ")));
                    if (anyVaal != null) return anyVaal;
                }
                else
                {
                    if (slotSkills.Count > 0)
                    {
                        var regularInSlot = slotSkills.FirstOrDefault(s => !s.IsVaalSkill && !(s.Name != null && s.Name.StartsWith("Vaal ")));
                        return regularInSlot ?? slotSkills[0];
                    }
                }
            }

            if (preferVaal)
            {
                return allSkills.FirstOrDefault(s => s.IsVaalSkill || (s.Name != null && s.Name.StartsWith("Vaal ")));
            }

            return null;
        }

        private ActorVaalSkill FindVaalSkill(Actor actor, ActorSkill skill)
        {
            if (actor == null || skill == null) return null;
            var vaalSkills = actor.ActorVaalSkills;
            if (vaalSkills == null || vaalSkills.Count == 0) return null;

            return vaalSkills.FirstOrDefault(v =>
                (!string.IsNullOrEmpty(v.VaalSkillInternalName) && !string.IsNullOrEmpty(skill.InternalName) &&
                 (v.VaalSkillInternalName.Equals(skill.InternalName, StringComparison.OrdinalIgnoreCase) ||
                  skill.InternalName.Contains(v.VaalSkillInternalName, StringComparison.OrdinalIgnoreCase))) ||
                (!string.IsNullOrEmpty(v.VaalSkillDisplayName) && !string.IsNullOrEmpty(skill.Name) &&
                 v.VaalSkillDisplayName.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(v.VaalSkillSkillName) && !string.IsNullOrEmpty(skill.Name) &&
                 (skill.Name.Contains(v.VaalSkillSkillName, StringComparison.OrdinalIgnoreCase) ||
                  v.VaalSkillSkillName.Contains(skill.Name, StringComparison.OrdinalIgnoreCase))));
        }

        private bool IsSkillReady(Keys key, Stopwatch timer, int cooldownMs, bool isVaalMode, bool useWhenAvailable, out string status)
        {
            var actor = GameController.Player?.GetComponent<Actor>();
            var actorSkill = FindActorSkill(key, isVaalMode);

            if (actorSkill != null)
            {
                bool isVaalSkill = isVaalMode || actorSkill.IsVaalSkill || (actorSkill.Name != null && actorSkill.Name.StartsWith("Vaal "));

                if (isVaalSkill)
                {
                    var vaalSkill = FindVaalSkill(actor, actorSkill);
                    int currSouls = vaalSkill?.CurrVaalSouls ?? 0;
                    int soulsPerUse = actorSkill.SoulsPerUse > 0 ? actorSkill.SoulsPerUse : (vaalSkill?.VaalMaxSouls ?? 0);

                    bool hasSouls = actorSkill.HasEnoughSouls || (soulsPerUse > 0 && currSouls >= soulsPerUse);

                    if (!hasSouls)
                    {
                        status = soulsPerUse > 0 ? $"Souls ({currSouls}/{soulsPerUse})" : "Need Souls";
                        return false;
                    }

                    if (!actorSkill.CanBeUsed)
                    {
                        status = soulsPerUse > 0 ? $"Soul Prevention ({currSouls}/{soulsPerUse})" : "Soul Prevention";
                        return false;
                    }

                    if (timer.ElapsedMilliseconds < 500)
                    {
                        status = "Casting...";
                        return false;
                    }

                    if (useWhenAvailable)
                    {
                        status = soulsPerUse > 0 ? $"Ready ({currSouls}/{soulsPerUse})" : $"Ready ({actorSkill.Name})";
                        return true;
                    }

                    if (timer.ElapsedMilliseconds < cooldownMs)
                    {
                        long rem = cooldownMs - timer.ElapsedMilliseconds;
                        status = $"Cooldown ({(rem / 1000f):F1}s)";
                        return false;
                    }

                    status = soulsPerUse > 0 ? $"Ready ({currSouls}/{soulsPerUse})" : $"Ready ({actorSkill.Name})";
                    return true;
                }

                if (actorSkill.IsOnCooldown)
                {
                    status = "On Cooldown";
                    return false;
                }

                if (actorSkill.TotalUses > 0 && actorSkill.RemainingUses <= 0)
                {
                    status = "No Charges";
                    return false;
                }

                if (!actorSkill.CanBeUsed)
                {
                    status = "Cannot Use";
                    return false;
                }

                if (useWhenAvailable)
                {
                    if (timer.ElapsedMilliseconds < 250)
                    {
                        status = "Casting...";
                        return false;
                    }

                    status = $"Ready ({actorSkill.Name})";
                    return true;
                }
            }

            if (timer.ElapsedMilliseconds < cooldownMs)
            {
                long rem = cooldownMs - timer.ElapsedMilliseconds;
                status = $"Cooldown ({(rem / 1000f):F1}s)";
                return false;
            }

            status = actorSkill != null ? $"Ready ({actorSkill.Name})" : "Ready";
            return true;
        }

        private void ProcessSkill(
            bool isEnabled,
            Keys key,
            int cooldownMs,
            bool isVaalSkill,
            bool useWhenAvailable,
            bool tracingEnabled,
            int tracingRange,
            Stopwatch timer)
        {
            if (!isEnabled || key == Keys.None) return;

            bool isReady = IsSkillReady(key, timer, cooldownMs, isVaalSkill, useWhenAvailable, out _);
            if (!isReady) return;

            bool canCast = !tracingEnabled || TraceMonster(tracingRange);
            if (canCast)
            {
                timer.Restart();
                SendKeyPress((byte)key);
            }
        }

        private void HandleSkills()
        {
            var life = GameController.Player?.GetComponent<Life>();
            if (life == null) return;

            // Check Minimum Mana Safety
            int minMana = Settings.MinManaForSkills?.Value ?? 10;
            if (life.CurMana < minMana) return;

            // Skill 1
            ProcessSkill(
                Settings.Skill1Enabled.Value,
                Settings.Skill1Key.Value.Key,
                Settings.Skill1Cooldown.Value,
                Settings.Skill1IsVaalSkill.Value,
                Settings.Skill1UseWhenAvailable.Value,
                Settings.Skill1TracingEnabled.Value,
                Settings.Skill1TracingRange.Value,
                _skill1Timer);

            // Skill 2
            ProcessSkill(
                Settings.Skill2Enabled.Value,
                Settings.Skill2Key.Value.Key,
                Settings.Skill2Cooldown.Value,
                Settings.Skill2IsVaalSkill.Value,
                Settings.Skill2UseWhenAvailable.Value,
                Settings.Skill2TracingEnabled.Value,
                Settings.Skill2TracingRange.Value,
                _skill2Timer);

            // Skill 3
            ProcessSkill(
                Settings.Skill3Enabled.Value,
                Settings.Skill3Key.Value.Key,
                Settings.Skill3Cooldown.Value,
                Settings.Skill3IsVaalSkill.Value,
                Settings.Skill3UseWhenAvailable.Value,
                Settings.Skill3TracingEnabled.Value,
                Settings.Skill3TracingRange.Value,
                _skill3Timer);
        }
        #endregion

        #region Flask Logic
        private string ResolveFlaskType(int slotIndex, string configuredType)
        {
            if (configuredType == "Auto")
            {
                var detected = GetDetectedFlaskType(slotIndex, out _);
                if (!string.IsNullOrEmpty(detected)) return detected;
                return slotIndex == 0 ? "HP" : "Utility";
            }
            return configuredType;
        }

        private void HandleFlasks()
        {
            if (!Settings.IsAutoFlasksGroupEnabled.Value) return;

            var life = GameController.Player?.GetComponent<Life>();
            if (life == null || life.MaxHP <= 0) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100f;
            float manaPercent = life.MaxMana > 0 ? (float)life.CurMana / life.MaxMana * 100f : 100f;

            ProcessFlask(0, Settings.IsFlask1Enabled.Value, ResolveFlaskType(0, Settings.Flask1Type.Value), Settings.Flask1Percentage.Value, (byte)Settings.Flask1Key.Value.Key, Settings.Flask1CooldownEnable.Value, Settings.Flask1Cooldown.Value, Settings.Flask1UseWhenAvailable.Value, hpPercent, manaPercent, _flask1Timer);
            ProcessFlask(1, Settings.IsFlask2Enabled.Value, ResolveFlaskType(1, Settings.Flask2Type.Value), Settings.Flask2Percentage.Value, (byte)Settings.Flask2Key.Value.Key, Settings.Flask2CooldownEnable.Value, Settings.Flask2Cooldown.Value, Settings.Flask2UseWhenAvailable.Value, hpPercent, manaPercent, _flask2Timer);
            ProcessFlask(2, Settings.IsFlask3Enabled.Value, ResolveFlaskType(2, Settings.Flask3Type.Value), Settings.Flask3Percentage.Value, (byte)Settings.Flask3Key.Value.Key, Settings.Flask3CooldownEnable.Value, Settings.Flask3Cooldown.Value, Settings.Flask3UseWhenAvailable.Value, hpPercent, manaPercent, _flask3Timer);
            ProcessFlask(3, Settings.IsFlask4Enabled.Value, ResolveFlaskType(3, Settings.Flask4Type.Value), Settings.Flask4Percentage.Value, (byte)Settings.Flask4Key.Value.Key, Settings.Flask4CooldownEnable.Value, Settings.Flask4Cooldown.Value, Settings.Flask4UseWhenAvailable.Value, hpPercent, manaPercent, _flask4Timer);
            ProcessFlask(4, Settings.IsFlask5Enabled.Value, ResolveFlaskType(4, Settings.Flask5Type.Value), Settings.Flask5Percentage.Value, (byte)Settings.Flask5Key.Value.Key, Settings.Flask5CooldownEnable.Value, Settings.Flask5Cooldown.Value, Settings.Flask5UseWhenAvailable.Value, hpPercent, manaPercent, _flask5Timer);
        }

        private void ProcessFlask(
            int slotIndex,
            bool isEnabled,
            string flaskType,
            int thresholdPercent,
            byte key,
            bool cooldownEnable,
            int cooldownMs,
            bool useWhenAvailable,
            float currentHpPercent,
            float currentManaPercent,
            Stopwatch timer)
        {
            if (!isEnabled || key == 0) return;

            bool cooldownReady = !cooldownEnable || timer.ElapsedMilliseconds >= cooldownMs;
            if (!cooldownReady) return;

            if (flaskType == "HP")
            {
                if (currentHpPercent <= thresholdPercent)
                {
                    timer.Restart();
                    SendKeyPress(key);
                }
            }
            else if (flaskType == "Mana")
            {
                if (currentManaPercent <= thresholdPercent)
                {
                    timer.Restart();
                    SendKeyPress(key);
                }
            }
            else if (flaskType == "Utility")
            {
                bool availabilityReady = !useWhenAvailable || IsUtilityFlaskAvailable(slotIndex, out _);
                if (availabilityReady)
                {
                    timer.Restart();
                    SendKeyPress(key);
                }
            }
        }

        private static readonly string[] LifeFlaskBuffs = { "flask_effect_life" };
        private static readonly string[] ManaFlaskBuffs =
        {
            "flask_effect_mana",
            "flask_effect_mana_not_removed_when_full",
            "flask_instant_mana_recovery_at_end_of_effect"
        };

        private IEnumerable<string> GetFlaskBuffNames(Flask flask)
        {
            if (flask == null || flask.Address == 0) return Enumerable.Empty<string>();
            try
            {
                var type = flask.M.Read<int>(flask.Address + 0x28, 0x10);
                return type switch
                {
                    1 => LifeFlaskBuffs,
                    2 => ManaFlaskBuffs,
                    3 => LifeFlaskBuffs.Concat(ManaFlaskBuffs),
                    4 when flask.M.ReadStringU(flask.M.Read<long>(flask.Address + 0x28, 0x18, 0x0)) is { } s and not "" => new[] { s },
                    _ => Enumerable.Empty<string>()
                };
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private string GetDetectedFlaskType(int index, out string flaskName)
        {
            flaskName = null;
            try
            {
                var flaskInventory = GameController.IngameState?.ServerData?.PlayerInventories?.LastOrDefault(x => x.TypeId == InventoryNameE.Flask1);
                if (flaskInventory?.Inventory == null) return null;

                var flaskItem = flaskInventory.Inventory[index, 0];
                if (flaskItem?.Address == 0 || flaskItem?.Item?.Address == 0) return null;

                var item = flaskItem.Item;
                if (!string.IsNullOrEmpty(item.Path))
                {
                    var baseType = GameController.Files?.BaseItemTypes?.Translate(item.Path);
                    flaskName = baseType?.BaseName ?? item.Metadata;
                }

                if (item.TryGetComponent<Flask>(out var flask))
                {
                    int type = flask.M.Read<int>(flask.Address + 0x28, 0x10);
                    return type switch
                    {
                        1 => "HP",
                        2 => "Mana",
                        3 => "Hybrid",
                        4 => "Utility",
                        _ => null
                    };
                }
            }
            catch
            {
            }
            return null;
        }

        private bool IsUtilityFlaskAvailable(int index, out string status)
        {
            status = "Ready";
            try
            {
                var flaskInventory = GameController.IngameState?.ServerData?.PlayerInventories?.LastOrDefault(x => x.TypeId == InventoryNameE.Flask1);
                if (flaskInventory?.Inventory == null)
                {
                    status = "No Inventory";
                    return false;
                }

                var flaskItem = flaskInventory.Inventory[index, 0];
                if (flaskItem?.Address == 0 || flaskItem?.Item?.Address == 0)
                {
                    status = "Empty Slot";
                    return false;
                }

                var item = flaskItem.Item;
                if (!item.TryGetComponent<Charges>(out var chargeComponent))
                {
                    status = "No Charges Comp";
                    return false;
                }
                if (!item.TryGetComponent<Flask>(out var flask))
                {
                    status = "Not Flask";
                    return false;
                }

                // Check charges
                if (chargeComponent.NumCharges < chargeComponent.ChargesPerUse)
                {
                    status = $"No Charges ({chargeComponent.NumCharges}/{chargeComponent.ChargesPerUse})";
                    return false;
                }

                // Check active buffs
                if (GameController.Player.TryGetComponent<Buffs>(out var playerBuffs))
                {
                    var buffNames = GetFlaskBuffNames(flask).ToList();
                    var activeBuff = playerBuffs.BuffsList.FirstOrDefault(b =>
                        b.FlaskSlot == index ||
                        (buffNames.Count > 0 && buffNames.Contains(b.Name)));

                    if (activeBuff != null)
                    {
                        status = $"Buff Active ({activeBuff.Name})";
                        return false;
                    }
                }

                status = $"Ready ({chargeComponent.NumCharges}/{chargeComponent.ChargesPerUse})";
                return true;
            }
            catch (Exception ex)
            {
                status = "Err: " + ex.Message;
                return false;
            }
        }

        private void SendKeyPress(byte key)
        {
            if (key == 0) return;
            int duration = Math.Max(5, Settings.KeyPressDurationMs?.Value ?? 20);

            keybd_event(key, 0, 0, 0);
            Task.Delay(duration).ContinueWith(_ =>
            {
                keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
            });
        }
        #endregion

        #region HUD Rendering
        public override void Render()
        {
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value || !Settings.ShowHud.Value) return;
            if (!GameController.InGame || GameController.IsLoading) return;

            var life = GameController.Player?.GetComponent<Life>();
            float hpPercent = life != null && life.MaxHP > 0 ? (float)life.CurHP / life.MaxHP * 100f : 0f;
            float manaPercent = life != null && life.MaxMana > 0 ? (float)life.CurMana / life.MaxMana * 100f : 0f;
            float esPercent = life != null && life.MaxES > 0 ? (float)life.CurES / life.MaxES * 100f : 0f;

            var drawPos = new Vector2(Settings.HudPositionX.Value, Settings.HudPositionY.Value);
            var startPos = drawPos;

            var lines = new List<(string text, Color color)>();

            // Line 1: Auto Status
            string statusText = Settings.IsAutoFlaskEnabled.Value ? "AutoFlask: [ACTIVE]" : "AutoFlask: [PAUSED]";
            var statusColor = Settings.IsAutoFlaskEnabled.Value ? Color.LimeGreen : Color.Gold;
            lines.Add((statusText, statusColor));

            // Line 2: Vitals
            if (Settings.ShowVitalsOnHud.Value)
            {
                var hpColor = hpPercent > 50 ? Color.LimeGreen : (hpPercent > 25 ? Color.Orange : Color.Red);
                string vitalsText = $"HP: {hpPercent:F0}% | MP: {manaPercent:F0}%";
                if (Settings.ShowEnergyShieldOnHud.Value && life != null && life.MaxES > 0)
                {
                    vitalsText += $" | ES: {esPercent:F0}%";
                }
                lines.Add((vitalsText, hpColor));
            }

            // Line 3: Area Time
            if (Settings.ShowAreaTimeOnHud.Value)
            {
                string areaTimeStr = "00:00:00";
                if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var currentStopwatch))
                {
                    var ts = currentStopwatch.Elapsed;
                    areaTimeStr = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
                }
                lines.Add(($"Area: {_currentAreaName ?? "Unknown"} ({areaTimeStr})", Color.DeepSkyBlue));
            }

            // Line 4: Skills Cooldown
            if (Settings.ShowSkillsOnHud.Value)
            {
                var skillParts = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    bool enabled = i switch
                    {
                        0 => Settings.Skill1Enabled.Value,
                        1 => Settings.Skill2Enabled.Value,
                        _ => Settings.Skill3Enabled.Value
                    };
                    if (!enabled) continue;

                    Keys key = i switch
                    {
                        0 => Settings.Skill1Key.Value.Key,
                        1 => Settings.Skill2Key.Value.Key,
                        _ => Settings.Skill3Key.Value.Key
                    };

                    int cd = i switch
                    {
                        0 => Settings.Skill1Cooldown.Value,
                        1 => Settings.Skill2Cooldown.Value,
                        _ => Settings.Skill3Cooldown.Value
                    };

                    bool avail = i switch
                    {
                        0 => Settings.Skill1UseWhenAvailable.Value,
                        1 => Settings.Skill2UseWhenAvailable.Value,
                        _ => Settings.Skill3UseWhenAvailable.Value
                    };

                    Stopwatch timer = i switch
                    {
                        0 => _skill1Timer,
                        1 => _skill2Timer,
                        _ => _skill3Timer
                    };

                    bool isVaal = i switch
                    {
                        0 => Settings.Skill1IsVaalSkill.Value,
                        1 => Settings.Skill2IsVaalSkill.Value,
                        _ => Settings.Skill3IsVaalSkill.Value
                    };

                    bool isReady = IsSkillReady(key, timer, cd, isVaal, avail, out string status);
                    string tag;
                    if (isReady)
                    {
                        tag = "RDY";
                    }
                    else if (status.StartsWith("Souls ("))
                    {
                        tag = status.Replace("Souls (", "").Replace(")", "");
                    }
                    else if (status.StartsWith("Soul Prevention"))
                    {
                        tag = "Prev";
                    }
                    else if (status.StartsWith("Cooldown ("))
                    {
                        tag = status.Replace("Cooldown (", "").Replace(")", "");
                    }
                    else if (status.StartsWith("On Cooldown"))
                    {
                        tag = "CD";
                    }
                    else
                    {
                        tag = status;
                    }

                    string skillLabel = (Settings.ShowSkillKeysOnHud.Value && key != Keys.None)
                        ? $"S{i + 1}[{FormatKeyName(key)}]"
                        : $"S{i + 1}";

                    skillParts.Add($"{skillLabel}: {tag}");
                }

                if (skillParts.Count > 0)
                {
                    lines.Add(($"Skills: {string.Join(" | ", skillParts)}", Color.LightSkyBlue));
                }
            }

            // Line 5: Flask Slot Status
            if (Settings.ShowFlaskStatusOnHud.Value && Settings.IsAutoFlasksGroupEnabled.Value)
            {
                var flaskBadges = new List<string>();
                for (int i = 0; i < 5; i++)
                {
                    bool enabled = i switch
                    {
                        0 => Settings.IsFlask1Enabled.Value,
                        1 => Settings.IsFlask2Enabled.Value,
                        2 => Settings.IsFlask3Enabled.Value,
                        3 => Settings.IsFlask4Enabled.Value,
                        _ => Settings.IsFlask5Enabled.Value
                    };
                    if (!enabled) continue;

                    string rawType = i switch
                    {
                        0 => Settings.Flask1Type.Value,
                        1 => Settings.Flask2Type.Value,
                        2 => Settings.Flask3Type.Value,
                        3 => Settings.Flask4Type.Value,
                        _ => Settings.Flask5Type.Value
                    };
                    string type = ResolveFlaskType(i, rawType);

                    Stopwatch timer = i switch
                    {
                        0 => _flask1Timer,
                        1 => _flask2Timer,
                        2 => _flask3Timer,
                        3 => _flask4Timer,
                        _ => _flask5Timer
                    };

                    int cd = i switch
                    {
                        0 => Settings.Flask1Cooldown.Value,
                        1 => Settings.Flask2Cooldown.Value,
                        2 => Settings.Flask3Cooldown.Value,
                        3 => Settings.Flask4Cooldown.Value,
                        _ => Settings.Flask5Cooldown.Value
                    };

                    bool cdEnabled = i switch
                    {
                        0 => Settings.Flask1CooldownEnable.Value,
                        1 => Settings.Flask2CooldownEnable.Value,
                        2 => Settings.Flask3CooldownEnable.Value,
                        3 => Settings.Flask4CooldownEnable.Value,
                        _ => Settings.Flask5CooldownEnable.Value
                    };

                    long elapsed = timer.ElapsedMilliseconds;
                    if (cdEnabled && elapsed < cd)
                    {
                        flaskBadges.Add($"[{i + 1}:CD]");
                    }
                    else
                    {
                        flaskBadges.Add($"[{i + 1}:{type[0]}]");
                    }
                }
                if (flaskBadges.Count > 0)
                {
                    lines.Add(($"Flasks: {string.Join(" ", flaskBadges)}", Color.PaleGreen));
                }
            }

            const float lineHeight = 19f;

            // Draw optional background box
            if (Settings.DrawHudBackground.Value)
            {
                float maxWidth = 0f;
                foreach (var line in lines)
                {
                    var size = Graphics.MeasureText(line.text);
                    if (size.X > maxWidth) maxWidth = size.X;
                }

                if (maxWidth > 0)
                {
                    var boxRect = new RectangleF(startPos.X - 8, startPos.Y - 6, maxWidth + 16, lines.Count * lineHeight + 12);
                    Graphics.DrawBox(boxRect, new Color(10, 15, 20, 190));
                    Graphics.DrawFrame(boxRect, new Color(40, 60, 80, 220), 1);
                }
            }

            foreach (var (text, color) in lines)
            {
                Graphics.DrawText(text, drawPos, color);
                drawPos.Y += lineHeight;
            }
        }
        #endregion

        #region Custom ImGui Settings UI
        public override void DrawSettings()
        {
            // Banner & Quick Control
            ImGui.TextColored(new Vector4(0.2f, 0.8f, 1f, 1f), "=== AUTO FLASK & SKILL BOT ===");
            ImGui.SameLine();
            ImGui.TextDisabled("v1.2.3");

            bool isGlobalActive = Settings.IsAutoFlaskEnabled.Value;
            if (isGlobalActive)
            {
                if (ImGui.Button("● BOT ACTIVE (Click to Pause)", new Vector2(250, 26)))
                {
                    Settings.IsAutoFlaskEnabled.Value = false;
                }
            }
            else
            {
                if (ImGui.Button("○ BOT PAUSED (Click to Activate)", new Vector2(250, 26)))
                {
                    Settings.IsAutoFlaskEnabled.Value = true;
                }
            }

            ImGui.SameLine();
            ImGui.Text("Hotkey:");
            ImGui.SameLine();
            DrawKeySelector("##MasterHotkey", Settings.ToggleAutoFlask);

            ImGui.Separator();

            if (ImGui.BeginTabBar("AutoFlaskMainTabs"))
            {
                if (ImGui.BeginTabItem("🧪 Flasks (1-5)"))
                {
                    DrawFlasksTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("⚔️ Skills (1-3)"))
                {
                    DrawSkillsTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("📊 HUD Overlay"))
                {
                    DrawHudTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("🛡️ Safety & Config"))
                {
                    DrawSafetyTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("⚙️ Classic Tree View"))
                {
                    base.DrawSettings();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private void DrawFlasksTab()
        {
            bool groupEnabled = Settings.IsAutoFlasksGroupEnabled.Value;
            if (ImGui.Checkbox("Enable Auto Flasks Group", ref groupEnabled))
            {
                Settings.IsAutoFlasksGroupEnabled.Value = groupEnabled;
            }
            ImGui.SameLine();
            if (ImGui.Button("🔄 Tự động đồng bộ loại bình theo trang bị game"))
            {
                SyncAllFlasksFromInventory();
            }
            ImGui.TextDisabled("Mẹo: Bạn có thể chọn Type là 'Auto' để bot tự động nhận diện loại bình thực tế trong game (HP/Mana/Utility).");
            ImGui.Separator();

            for (int i = 0; i < 5; i++)
            {
                DrawSingleFlaskCard(i);
            }
        }

        private void SyncAllFlasksFromInventory()
        {
            for (int slot = 0; slot < 5; slot++)
            {
                string detected = GetDetectedFlaskType(slot, out _);
                if (string.IsNullOrEmpty(detected)) continue;

                ListNode typeNode = slot switch
                {
                    0 => Settings.Flask1Type,
                    1 => Settings.Flask2Type,
                    2 => Settings.Flask3Type,
                    3 => Settings.Flask4Type,
                    _ => Settings.Flask5Type
                };

                RangeNode<int> pctNode = slot switch
                {
                    0 => Settings.Flask1Percentage,
                    1 => Settings.Flask2Percentage,
                    2 => Settings.Flask3Percentage,
                    3 => Settings.Flask4Percentage,
                    _ => Settings.Flask5Percentage
                };

                ToggleNode availNode = slot switch
                {
                    0 => Settings.Flask1UseWhenAvailable,
                    1 => Settings.Flask2UseWhenAvailable,
                    2 => Settings.Flask3UseWhenAvailable,
                    3 => Settings.Flask4UseWhenAvailable,
                    _ => Settings.Flask5UseWhenAvailable
                };

                typeNode.Value = detected;
                if (detected == "HP")
                {
                    pctNode.Value = 45;
                    availNode.Value = false;
                }
                else if (detected == "Mana")
                {
                    pctNode.Value = 25;
                    availNode.Value = false;
                }
                else if (detected == "Utility")
                {
                    availNode.Value = true;
                }
            }
        }

        private void DrawSingleFlaskCard(int slot)
        {
            ToggleNode isEnabledNode = slot switch
            {
                0 => Settings.IsFlask1Enabled,
                1 => Settings.IsFlask2Enabled,
                2 => Settings.IsFlask3Enabled,
                3 => Settings.IsFlask4Enabled,
                _ => Settings.IsFlask5Enabled
            };

            ListNode typeNode = slot switch
            {
                0 => Settings.Flask1Type,
                1 => Settings.Flask2Type,
                2 => Settings.Flask3Type,
                3 => Settings.Flask4Type,
                _ => Settings.Flask5Type
            };

            RangeNode<int> pctNode = slot switch
            {
                0 => Settings.Flask1Percentage,
                1 => Settings.Flask2Percentage,
                2 => Settings.Flask3Percentage,
                3 => Settings.Flask4Percentage,
                _ => Settings.Flask5Percentage
            };

            HotkeyNodeV2 keyNode = slot switch
            {
                0 => Settings.Flask1Key,
                1 => Settings.Flask2Key,
                2 => Settings.Flask3Key,
                3 => Settings.Flask4Key,
                _ => Settings.Flask5Key
            };

            ToggleNode cdEnabledNode = slot switch
            {
                0 => Settings.Flask1CooldownEnable,
                1 => Settings.Flask2CooldownEnable,
                2 => Settings.Flask3CooldownEnable,
                3 => Settings.Flask4CooldownEnable,
                _ => Settings.Flask5CooldownEnable
            };

            RangeNode<int> cdMsNode = slot switch
            {
                0 => Settings.Flask1Cooldown,
                1 => Settings.Flask2Cooldown,
                2 => Settings.Flask3Cooldown,
                3 => Settings.Flask4Cooldown,
                _ => Settings.Flask5Cooldown
            };

            ToggleNode availNode = slot switch
            {
                0 => Settings.Flask1UseWhenAvailable,
                1 => Settings.Flask2UseWhenAvailable,
                2 => Settings.Flask3UseWhenAvailable,
                3 => Settings.Flask4UseWhenAvailable,
                _ => Settings.Flask5UseWhenAvailable
            };

            string detectedType = GetDetectedFlaskType(slot, out string detectedName);
            string effectiveType = ResolveFlaskType(slot, typeNode.Value);

            // Diagnostic status
            IsUtilityFlaskAvailable(slot, out string liveStatus);

            ImGui.PushID($"FlaskSlot_{slot}");

            string itemDesc = !string.IsNullOrEmpty(detectedName) ? $" | {detectedName}" : "";
            string typeTag = typeNode.Value == "Auto" ? $"Auto ({effectiveType})" : typeNode.Value;
            bool headerOpen = ImGui.CollapsingHeader($"Flask [{slot + 1}] - ({typeTag}){itemDesc} | Key: {keyNode.Value.Key}", ImGuiTreeNodeFlags.DefaultOpen);
            if (headerOpen)
            {
                bool enabled = isEnabledNode.Value;
                if (ImGui.Checkbox("Enable", ref enabled)) isEnabledNode.Value = enabled;

                ImGui.SameLine();
                ImGui.Text("Status:");
                ImGui.SameLine();
                var statusColor = liveStatus.StartsWith("Ready") ? new Vector4(0.2f, 1f, 0.4f, 1f) :
                                  liveStatus.StartsWith("Buff") ? new Vector4(1f, 0.9f, 0.2f, 1f) :
                                  liveStatus.StartsWith("No Charges") ? new Vector4(1f, 0.3f, 0.3f, 1f) :
                                  new Vector4(0.7f, 0.7f, 0.7f, 1f);
                ImGui.TextColored(statusColor, liveStatus);

                ImGui.SameLine();
                if (ImGui.Button("Test Trigger"))
                {
                    SendKeyPress((byte)keyNode.Value.Key);
                }

                if (!string.IsNullOrEmpty(detectedType))
                {
                    ImGui.TextDisabled($"Trang bị trong game: {detectedName ?? "Flask"} [{detectedType}]");
                    if (typeNode.Value != "Auto" && typeNode.Value != detectedType)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1f, 0.4f, 0.2f, 1f), $"[⚠️ Lệch loại: Cấu hình là {typeNode.Value}]");
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"Đồng bộ sang {detectedType}##sync_{slot}"))
                        {
                            typeNode.Value = detectedType;
                        }
                    }
                }

                // Key selector
                DrawKeySelector("Key", keyNode);

                // Type dropdown
                int currentTypeIndex = Array.IndexOf(FlaskTypes, typeNode.Value);
                if (currentTypeIndex < 0) currentTypeIndex = 0;
                if (ImGui.Combo("Type", ref currentTypeIndex, FlaskTypes, FlaskTypes.Length))
                {
                    typeNode.Value = FlaskTypes[currentTypeIndex];
                }

                // HP/Mana Threshold
                if (effectiveType == "HP" || effectiveType == "Mana" || effectiveType == "Hybrid")
                {
                    int pct = pctNode.Value;
                    if (ImGui.SliderInt($"Trigger Threshold ({effectiveType} %)", ref pct, 5, 100))
                    {
                        pctNode.Value = pct;
                    }
                }
                else if (effectiveType == "Utility")
                {
                    bool avail = availNode.Value;
                    if (ImGui.Checkbox("Use When Available (Auto re-drink when buff expires & charges ready)", ref avail))
                    {
                        availNode.Value = avail;
                    }
                }

                // Cooldown
                bool cdEnabled = cdEnabledNode.Value;
                if (ImGui.Checkbox("Enable Cooldown", ref cdEnabled)) cdEnabledNode.Value = cdEnabled;

                if (cdEnabledNode.Value)
                {
                    int cd = cdMsNode.Value;
                    if (ImGui.SliderInt("Cooldown (ms)", ref cd, 0, 30000)) cdMsNode.Value = cd;

                    ImGui.SameLine();
                    if (ImGui.Button("1s")) cdMsNode.Value = 1000;
                    ImGui.SameLine();
                    if (ImGui.Button("3s")) cdMsNode.Value = 3000;
                    ImGui.SameLine();
                    if (ImGui.Button("5s")) cdMsNode.Value = 5000;
                }

                ImGui.Separator();
            }

            ImGui.PopID();
        }

        private void DrawSkillsTab()
        {
            int minMana = Settings.MinManaForSkills?.Value ?? 10;
            if (ImGui.SliderInt("Minimum Mana Safety for Skills", ref minMana, 0, 100))
            {
                if (Settings.MinManaForSkills != null) Settings.MinManaForSkills.Value = minMana;
            }
            ImGui.TextDisabled("Skills will not cast if current Mana is below this value.");
            ImGui.Separator();

            DrawSingleSkillCard(1, Settings.Skill1Enabled, Settings.Skill1IsVaalSkill, Settings.Skill1Key, Settings.Skill1Cooldown, Settings.Skill1UseWhenAvailable, Settings.Skill1TracingEnabled, Settings.Skill1TracingRange, _skill1Timer);
            DrawSingleSkillCard(2, Settings.Skill2Enabled, Settings.Skill2IsVaalSkill, Settings.Skill2Key, Settings.Skill2Cooldown, Settings.Skill2UseWhenAvailable, Settings.Skill2TracingEnabled, Settings.Skill2TracingRange, _skill2Timer);
            DrawSingleSkillCard(3, Settings.Skill3Enabled, Settings.Skill3IsVaalSkill, Settings.Skill3Key, Settings.Skill3Cooldown, Settings.Skill3UseWhenAvailable, Settings.Skill3TracingEnabled, Settings.Skill3TracingRange, _skill3Timer);
        }

        private void DrawSingleSkillCard(
            int skillIndex,
            ToggleNode enabledNode,
            ToggleNode isVaalNode,
            HotkeyNodeV2 keyNode,
            RangeNode<int> cdNode,
            ToggleNode useWhenAvailableNode,
            ToggleNode tracingNode,
            RangeNode<int> rangeNode,
            Stopwatch timer)
        {
            ImGui.PushID($"SkillCard_{skillIndex}");

            bool isVaal = isVaalNode.Value;
            var detectedSkill = FindActorSkill(keyNode.Value.Key, isVaal);
            bool autoDetectedVaal = detectedSkill != null && (detectedSkill.IsVaalSkill || (detectedSkill.Name != null && detectedSkill.Name.StartsWith("Vaal ")));
            bool effectiveVaal = isVaal || autoDetectedVaal;

            bool isReady = IsSkillReady(keyNode.Value.Key, timer, cdNode.Value, effectiveVaal, useWhenAvailableNode.Value, out string liveStatus);

            var actor = GameController.Player?.GetComponent<Actor>();
            var vaalSkillObj = effectiveVaal && detectedSkill != null ? FindVaalSkill(actor, detectedSkill) : null;

            string titleStatus = isReady ? "READY" : liveStatus;
            string vaalPrefix = effectiveVaal ? "[VAAL] " : "";
            bool open = ImGui.CollapsingHeader($"{vaalPrefix}Skill [{skillIndex}] - Key: {keyNode.Value.Key} | Status: {titleStatus}", ImGuiTreeNodeFlags.DefaultOpen);
            if (open)
            {
                bool enabled = enabledNode.Value;
                if (ImGui.Checkbox("Enable Skill", ref enabled)) enabledNode.Value = enabled;

                ImGui.SameLine();
                ImGui.Text("Status:");
                ImGui.SameLine();
                var statusColor = isReady ? new Vector4(0.2f, 1f, 0.4f, 1f) :
                                  liveStatus.StartsWith("Souls") ? new Vector4(0.9f, 0.4f, 1f, 1f) :
                                  liveStatus.StartsWith("Soul Prevention") ? new Vector4(1f, 0.8f, 0.2f, 1f) :
                                  liveStatus.StartsWith("On Cooldown") ? new Vector4(1f, 0.7f, 0.2f, 1f) :
                                  new Vector4(0.7f, 0.7f, 0.7f, 1f);
                ImGui.TextColored(statusColor, liveStatus);

                if (detectedSkill != null)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"({detectedSkill.Name})");
                }

                ImGui.SameLine();
                if (ImGui.Button("Test Cast"))
                {
                    SendKeyPress((byte)keyNode.Value.Key);
                }

                DrawKeySelector("Cast Key", keyNode);

                bool vaalChecked = isVaalNode.Value;
                if (ImGui.Checkbox("Is Vaal Skill (Phụ thuộc Soul, không hồi chiêu thường)", ref vaalChecked))
                {
                    isVaalNode.Value = vaalChecked;
                }
                if (autoDetectedVaal && !vaalChecked)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(0.8f, 0.5f, 1f, 1f), "[Tự động nhận diện Vaal Skill]");
                }

                if (effectiveVaal)
                {
                    int currSouls = vaalSkillObj?.CurrVaalSouls ?? 0;
                    int maxSouls = vaalSkillObj?.VaalMaxSouls ?? 0;
                    int soulsPerUse = (detectedSkill != null && detectedSkill.SoulsPerUse > 0) ? detectedSkill.SoulsPerUse : maxSouls;

                    float progress = soulsPerUse > 0 ? Math.Min(1f, (float)currSouls / soulsPerUse) : 0f;
                    string soulsOverlay = soulsPerUse > 0 ? $"{currSouls} / {soulsPerUse} Souls (Max: {maxSouls})" : $"{currSouls} Souls";
                    ImGui.ProgressBar(progress, new Vector2(-1, 0), soulsOverlay);

                    ImGui.TextDisabled("Vaal Skill phụ thuộc Souls thay vì thời gian hồi chiêu. Khi đủ Souls và không bị Soul Prevention sẽ sẵn sàng kích hoạt.");
                }

                bool useWhenAvail = useWhenAvailableNode.Value;
                string useAvailLabel = effectiveVaal ? "Use When Available (Tự động dùng ngay khi đủ Souls)" : "Use When Available (Tự động dùng khi skill hồi chiêu)";
                if (ImGui.Checkbox(useAvailLabel, ref useWhenAvail))
                {
                    useWhenAvailableNode.Value = useWhenAvail;
                }
                if (effectiveVaal)
                {
                    ImGui.TextDisabled("Tự động tung chiêu ngay khi tích lũy đủ Souls và không trong trạng thái Soul Gain Prevention.");
                }
                else
                {
                    ImGui.TextDisabled("Tự động tung chiêu ngay khi kỹ năng vừa hết thời gian hồi (Off Cooldown) trong game.");
                }

                if (!effectiveVaal || !useWhenAvailableNode.Value)
                {
                    int cd = cdNode.Value;
                    string cdLabel = effectiveVaal ? "Min Interval / Cooldown Throttling (ms)" : "Cooldown (ms)";
                    if (ImGui.SliderInt(cdLabel, ref cd, 100, 30000)) cdNode.Value = cd;

                    ImGui.SameLine();
                    if (ImGui.Button("2s")) cdNode.Value = 2000;
                    ImGui.SameLine();
                    if (ImGui.Button("5s")) cdNode.Value = 5000;
                    ImGui.SameLine();
                    if (ImGui.Button("10s")) cdNode.Value = 10000;
                }

                bool tracing = tracingNode.Value;
                if (ImGui.Checkbox("Auto-Aim (Monster Tracing)", ref tracing)) tracingNode.Value = tracing;

                if (tracingNode.Value)
                {
                    int range = rangeNode.Value;
                    if (ImGui.SliderInt("Tracing Max Range", ref range, 30, 150)) rangeNode.Value = range;
                    ImGui.TextDisabled("Cursor will snap to nearest high-rarity monster in range before casting.");
                }

                ImGui.Separator();
            }

            ImGui.PopID();
        }

        private void DrawHudTab()
        {
            bool showHud = Settings.ShowHud.Value;
            if (ImGui.Checkbox("Show On-Screen HUD Overlay", ref showHud)) Settings.ShowHud.Value = showHud;

            ImGui.Separator();
            ImGui.Text("Quick Position Presets:");
            if (ImGui.Button("Top-Left"))
            {
                Settings.HudPositionX.Value = 30;
                Settings.HudPositionY.Value = 170;
            }
            ImGui.SameLine();
            if (ImGui.Button("Top-Right"))
            {
                Settings.HudPositionX.Value = 1600;
                Settings.HudPositionY.Value = 170;
            }
            ImGui.SameLine();
            if (ImGui.Button("Bottom-Left"))
            {
                Settings.HudPositionX.Value = 30;
                Settings.HudPositionY.Value = 850;
            }
            ImGui.SameLine();
            if (ImGui.Button("Bottom-Right"))
            {
                Settings.HudPositionX.Value = 1600;
                Settings.HudPositionY.Value = 850;
            }

            int posX = Settings.HudPositionX.Value;
            if (ImGui.SliderInt("HUD X Position", ref posX, 0, 3840)) Settings.HudPositionX.Value = posX;

            int posY = Settings.HudPositionY.Value;
            if (ImGui.SliderInt("HUD Y Position", ref posY, 0, 2160)) Settings.HudPositionY.Value = posY;

            ImGui.Separator();
            ImGui.Text("HUD Display Items:");

            bool showVitals = Settings.ShowVitalsOnHud.Value;
            if (ImGui.Checkbox("Show HP & Mana", ref showVitals)) Settings.ShowVitalsOnHud.Value = showVitals;

            bool showEs = Settings.ShowEnergyShieldOnHud.Value;
            if (ImGui.Checkbox("Show Energy Shield", ref showEs)) Settings.ShowEnergyShieldOnHud.Value = showEs;

            bool showArea = Settings.ShowAreaTimeOnHud.Value;
            if (ImGui.Checkbox("Show Area & Stopwatch Time", ref showArea)) Settings.ShowAreaTimeOnHud.Value = showArea;

            bool showSkills = Settings.ShowSkillsOnHud.Value;
            if (ImGui.Checkbox("Show Skill Cooldowns", ref showSkills)) Settings.ShowSkillsOnHud.Value = showSkills;

            if (Settings.ShowSkillsOnHud.Value)
            {
                ImGui.SameLine();
                bool showSkillKeys = Settings.ShowSkillKeysOnHud.Value;
                if (ImGui.Checkbox("Show Hotkeys (Hiện phím tắt)", ref showSkillKeys)) Settings.ShowSkillKeysOnHud.Value = showSkillKeys;
            }

            bool showFlasks = Settings.ShowFlaskStatusOnHud.Value;
            if (ImGui.Checkbox("Show Flask Status Badges", ref showFlasks)) Settings.ShowFlaskStatusOnHud.Value = showFlasks;

            bool drawBg = Settings.DrawHudBackground.Value;
            if (ImGui.Checkbox("Draw Background Box (Tô nền khung HUD)", ref drawBg)) Settings.DrawHudBackground.Value = drawBg;
        }

        private void DrawSafetyTab()
        {
            bool fg = Settings.OnlyRunWhenForeground.Value;
            if (ImGui.Checkbox("Only Run When Game Is Focused (Foreground Window)", ref fg))
            {
                Settings.OnlyRunWhenForeground.Value = fg;
            }
            ImGui.TextDisabled("Prevents sending flask/skill keys to Discord or web browser when alt-tabbed.");

            bool pauseTown = Settings.PauseInTownAndHideout.Value;
            if (ImGui.Checkbox("Pause In Town And Hideout", ref pauseTown))
            {
                Settings.PauseInTownAndHideout.Value = pauseTown;
            }
            ImGui.TextDisabled("Saves flask charges and prevents accidental casts in peaceful areas.");

            int pressDuration = Settings.KeyPressDurationMs.Value;
            if (ImGui.SliderInt("Key Press Duration (ms)", ref pressDuration, 5, 100))
            {
                Settings.KeyPressDurationMs.Value = pressDuration;
            }
            ImGui.TextDisabled("Time between key down and key up (sent asynchronously on thread pool).");
        }

        private void DrawKeySelector(string label, HotkeyNodeV2 node)
        {
            var currentKey = node.Value.Key;
            int currentIndex = Array.IndexOf(CommonKeys, currentKey);
            if (currentIndex < 0) currentIndex = 0;

            if (ImGui.Combo(label, ref currentIndex, CommonKeyLabels, CommonKeyLabels.Length))
            {
                node.Value = node.Value with { Key = CommonKeys[currentIndex] };
                Input.RegisterKey(node.Value.Key);
            }
        }

        private static string FormatKeyName(Keys key)
        {
            return key switch
            {
                Keys.D0 => "0",
                Keys.D1 => "1",
                Keys.D2 => "2",
                Keys.D3 => "3",
                Keys.D4 => "4",
                Keys.D5 => "5",
                Keys.D6 => "6",
                Keys.D7 => "7",
                Keys.D8 => "8",
                Keys.D9 => "9",
                _ => key.ToString()
            };
        }
        #endregion
    }
}
