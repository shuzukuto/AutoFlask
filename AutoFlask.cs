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

        private static readonly string[] FlaskTypes = { "HP", "Mana", "Utility" };

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

        #region Monster Tracing & Skills
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

        private void HandleSkills()
        {
            var life = GameController.Player?.GetComponent<Life>();
            if (life == null) return;

            // Check Minimum Mana Safety
            int minMana = Settings.MinManaForSkills?.Value ?? 10;
            if (life.CurMana < minMana) return;

            // Skill 1
            if (Settings.Skill1Enabled.Value && _skill1Timer.ElapsedMilliseconds >= Settings.Skill1Cooldown.Value)
            {
                bool canCast = !Settings.Skill1TracingEnabled.Value || TraceMonster(Settings.Skill1TracingRange.Value);
                if (canCast)
                {
                    _skill1Timer.Restart();
                    SendKeyPress((byte)Settings.Skill1Key.Value.Key);
                }
            }

            // Skill 2
            if (Settings.Skill2Enabled.Value && _skill2Timer.ElapsedMilliseconds >= Settings.Skill2Cooldown.Value)
            {
                bool canCast = !Settings.Skill2TracingEnabled.Value || TraceMonster(Settings.Skill2TracingRange.Value);
                if (canCast)
                {
                    _skill2Timer.Restart();
                    SendKeyPress((byte)Settings.Skill2Key.Value.Key);
                }
            }

            // Skill 3
            if (Settings.Skill3Enabled.Value && _skill3Timer.ElapsedMilliseconds >= Settings.Skill3Cooldown.Value)
            {
                bool canCast = !Settings.Skill3TracingEnabled.Value || TraceMonster(Settings.Skill3TracingRange.Value);
                if (canCast)
                {
                    _skill3Timer.Restart();
                    SendKeyPress((byte)Settings.Skill3Key.Value.Key);
                }
            }
        }
        #endregion

        #region Flask Logic
        private void HandleFlasks()
        {
            if (!Settings.IsAutoFlasksGroupEnabled.Value) return;

            var life = GameController.Player?.GetComponent<Life>();
            if (life == null || life.MaxHP <= 0) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100f;
            float manaPercent = life.MaxMana > 0 ? (float)life.CurMana / life.MaxMana * 100f : 100f;

            ProcessFlask(0, Settings.IsFlask1Enabled.Value, Settings.Flask1Type.Value, Settings.Flask1Percentage.Value, (byte)Settings.Flask1Key.Value.Key, Settings.Flask1CooldownEnable.Value, Settings.Flask1Cooldown.Value, Settings.Flask1UseWhenAvailable.Value, hpPercent, manaPercent, _flask1Timer);
            ProcessFlask(1, Settings.IsFlask2Enabled.Value, Settings.Flask2Type.Value, Settings.Flask2Percentage.Value, (byte)Settings.Flask2Key.Value.Key, Settings.Flask2CooldownEnable.Value, Settings.Flask2Cooldown.Value, Settings.Flask2UseWhenAvailable.Value, hpPercent, manaPercent, _flask2Timer);
            ProcessFlask(2, Settings.IsFlask3Enabled.Value, Settings.Flask3Type.Value, Settings.Flask3Percentage.Value, (byte)Settings.Flask3Key.Value.Key, Settings.Flask3CooldownEnable.Value, Settings.Flask3Cooldown.Value, Settings.Flask3UseWhenAvailable.Value, hpPercent, manaPercent, _flask3Timer);
            ProcessFlask(3, Settings.IsFlask4Enabled.Value, Settings.Flask4Type.Value, Settings.Flask4Percentage.Value, (byte)Settings.Flask4Key.Value.Key, Settings.Flask4CooldownEnable.Value, Settings.Flask4Cooldown.Value, Settings.Flask4UseWhenAvailable.Value, hpPercent, manaPercent, _flask4Timer);
            ProcessFlask(4, Settings.IsFlask5Enabled.Value, Settings.Flask5Type.Value, Settings.Flask5Percentage.Value, (byte)Settings.Flask5Key.Value.Key, Settings.Flask5CooldownEnable.Value, Settings.Flask5Cooldown.Value, Settings.Flask5UseWhenAvailable.Value, hpPercent, manaPercent, _flask5Timer);
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
                if (Settings.Skill1Enabled.Value)
                {
                    long rem = Math.Max(0, Settings.Skill1Cooldown.Value - _skill1Timer.ElapsedMilliseconds);
                    skillParts.Add($"S1: {(rem > 0 ? $"{rem / 1000f:F1}s" : "RDY")}");
                }
                if (Settings.Skill2Enabled.Value)
                {
                    long rem = Math.Max(0, Settings.Skill2Cooldown.Value - _skill2Timer.ElapsedMilliseconds);
                    skillParts.Add($"S2: {(rem > 0 ? $"{rem / 1000f:F1}s" : "RDY")}");
                }
                if (Settings.Skill3Enabled.Value)
                {
                    long rem = Math.Max(0, Settings.Skill3Cooldown.Value - _skill3Timer.ElapsedMilliseconds);
                    skillParts.Add($"S3: {(rem > 0 ? $"{rem / 1000f:F1}s" : "RDY")}");
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

                    string type = i switch
                    {
                        0 => Settings.Flask1Type.Value,
                        1 => Settings.Flask2Type.Value,
                        2 => Settings.Flask3Type.Value,
                        3 => Settings.Flask4Type.Value,
                        _ => Settings.Flask5Type.Value
                    };

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

            // Measure background box dimensions
            float maxWidth = 0f;
            float lineHeight = 19f;
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
            ImGui.TextDisabled("v1.1.0");

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
            ImGui.Separator();

            for (int i = 0; i < 5; i++)
            {
                DrawSingleFlaskCard(i);
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

            // Diagnostic status
            IsUtilityFlaskAvailable(slot, out string liveStatus);

            ImGui.PushID($"FlaskSlot_{slot}");

            bool headerOpen = ImGui.CollapsingHeader($"Flask [{slot + 1}] - ({typeNode.Value}) | Key: {keyNode.Value.Key}", ImGuiTreeNodeFlags.DefaultOpen);
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
                if (typeNode.Value == "HP" || typeNode.Value == "Mana")
                {
                    int pct = pctNode.Value;
                    if (ImGui.SliderInt($"Trigger Threshold ({typeNode.Value} %)", ref pct, 5, 100))
                    {
                        pctNode.Value = pct;
                    }
                }
                else if (typeNode.Value == "Utility")
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

            DrawSingleSkillCard(1, Settings.Skill1Enabled, Settings.Skill1Key, Settings.Skill1Cooldown, Settings.Skill1TracingEnabled, Settings.Skill1TracingRange, _skill1Timer);
            DrawSingleSkillCard(2, Settings.Skill2Enabled, Settings.Skill2Key, Settings.Skill2Cooldown, Settings.Skill2TracingEnabled, Settings.Skill2TracingRange, _skill2Timer);
            DrawSingleSkillCard(3, Settings.Skill3Enabled, Settings.Skill3Key, Settings.Skill3Cooldown, Settings.Skill3TracingEnabled, Settings.Skill3TracingRange, _skill3Timer);
        }

        private void DrawSingleSkillCard(
            int skillIndex,
            ToggleNode enabledNode,
            HotkeyNodeV2 keyNode,
            RangeNode<int> cdNode,
            ToggleNode tracingNode,
            RangeNode<int> rangeNode,
            Stopwatch timer)
        {
            ImGui.PushID($"SkillCard_{skillIndex}");

            long remainingCd = Math.Max(0, cdNode.Value - timer.ElapsedMilliseconds);
            string cdStatus = remainingCd > 0 ? $"{remainingCd / 1000f:F1}s" : "READY";

            bool open = ImGui.CollapsingHeader($"Skill [{skillIndex}] - Key: {keyNode.Value.Key} | Status: {cdStatus}", ImGuiTreeNodeFlags.DefaultOpen);
            if (open)
            {
                bool enabled = enabledNode.Value;
                if (ImGui.Checkbox("Enable Skill", ref enabled)) enabledNode.Value = enabled;

                ImGui.SameLine();
                if (ImGui.Button("Test Cast"))
                {
                    SendKeyPress((byte)keyNode.Value.Key);
                }

                DrawKeySelector("Cast Key", keyNode);

                int cd = cdNode.Value;
                if (ImGui.SliderInt("Cooldown (ms)", ref cd, 100, 30000)) cdNode.Value = cd;

                ImGui.SameLine();
                if (ImGui.Button("2s")) cdNode.Value = 2000;
                ImGui.SameLine();
                if (ImGui.Button("5s")) cdNode.Value = 5000;
                ImGui.SameLine();
                if (ImGui.Button("10s")) cdNode.Value = 10000;

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

            bool showFlasks = Settings.ShowFlaskStatusOnHud.Value;
            if (ImGui.Checkbox("Show Flask Status Badges", ref showFlasks)) Settings.ShowFlaskStatusOnHud.Value = showFlasks;
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
        #endregion
    }
}
