using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Interfaces;
using SharpDX;

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
        private readonly Stopwatch _mouseThrottle = new Stopwatch();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private Entity _currentTarget;

        private readonly Dictionary<string, Stopwatch> _areaTimers = new Dictionary<string, Stopwatch>();
        private string _currentAreaName;

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
            _mouseThrottle.Start();
            return true;
        }

        public override Job Tick()
        {
            if (Settings.ToggleAutoFlask.PressedOnce())
            {
                Settings.IsAutoFlaskEnabled.Value = !Settings.IsAutoFlaskEnabled.Value;
                DebugWindow.LogMsg($"[AutoFlask] Active: {Settings.IsAutoFlaskEnabled.Value}", 3, Color.Yellow);
            }

            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value)
                return base.Tick();

            if (!GameController.InGame || GameController.IsLoading)
            {
                if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var currentStopwatch))
                {
                    currentStopwatch.Stop();
                }
                return base.Tick();
            }

            // Track area time
            var currentArea = GameController.Area.CurrentArea;
            if (currentArea != null)
            {
                var areaName = currentArea.Name;
                if (areaName != _currentAreaName)
                {
                    if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var prevStopwatch))
                    {
                        prevStopwatch.Stop();
                    }
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

            if (!GameController.Player.IsAlive)
                return base.Tick();

            if (GameController.Area.CurrentArea == null || GameController.Area.CurrentArea.IsHideout || GameController.Area.CurrentArea.IsTown)
                return base.Tick();

            HandleFlasks();
            HandleSkills();

            return base.Tick();
        }

        private bool TraceMonster(float maxDistance)
        {
            if (!GameController.Window.IsForeground())
            {
                _currentTarget = null;
                return false;
            }

            _currentTarget = GameController.EntityListWrapper.Entities
                .Where(e => e.Type == EntityType.Monster && e.IsHostile && e.IsAlive && e.IsTargetable && e.DistancePlayer <= maxDistance)
                .OrderByDescending(e => (int)e.Rarity)
                .ThenBy(e => e.DistancePlayer)
                .FirstOrDefault();

            if (_currentTarget != null)
            {
                var screenPos = GameController.IngameState.Camera.WorldToScreen(_currentTarget.Pos);
                if (screenPos != Vector2.Zero)
                {
                    var windowRect = GameController.Window.GetWindowRectangle();
                    Input.SetCursorPos(screenPos + windowRect.Location);
                    return true;
                }
            }
            return false;
        }

        private void HandleFlasks()
        {
            if (!Settings.IsAutoFlasksGroupEnabled.Value) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100;
            float manaPercent = (float)life.CurMana / life.MaxMana * 100;

            ProcessFlask(0, Settings.IsFlask1Enabled.Value, Settings.Flask1Type.Value, Settings.Flask1HpPercentage.Value, Settings.Flask1ManaPercentage.Value, (byte)Settings.Flask1Key.Value, Settings.Flask1CooldownEnable.Value, Settings.Flask1Cooldown.Value, Settings.Flask1UseWhenAvailable.Value, hpPercent, manaPercent, _flask1Timer);
            ProcessFlask(1, Settings.IsFlask2Enabled.Value, Settings.Flask2Type.Value, Settings.Flask2HpPercentage.Value, Settings.Flask2ManaPercentage.Value, (byte)Settings.Flask2Key.Value, Settings.Flask2CooldownEnable.Value, Settings.Flask2Cooldown.Value, Settings.Flask2UseWhenAvailable.Value, hpPercent, manaPercent, _flask2Timer);
            ProcessFlask(2, Settings.IsFlask3Enabled.Value, Settings.Flask3Type.Value, Settings.Flask3HpPercentage.Value, Settings.Flask3ManaPercentage.Value, (byte)Settings.Flask3Key.Value, Settings.Flask3CooldownEnable.Value, Settings.Flask3Cooldown.Value, Settings.Flask3UseWhenAvailable.Value, hpPercent, manaPercent, _flask3Timer);
            ProcessFlask(3, Settings.IsFlask4Enabled.Value, Settings.Flask4Type.Value, Settings.Flask4HpPercentage.Value, Settings.Flask4ManaPercentage.Value, (byte)Settings.Flask4Key.Value, Settings.Flask4CooldownEnable.Value, Settings.Flask4Cooldown.Value, Settings.Flask4UseWhenAvailable.Value, hpPercent, manaPercent, _flask4Timer);
            ProcessFlask(4, Settings.IsFlask5Enabled.Value, Settings.Flask5Type.Value, Settings.Flask5HpPercentage.Value, Settings.Flask5ManaPercentage.Value, (byte)Settings.Flask5Key.Value, Settings.Flask5CooldownEnable.Value, Settings.Flask5Cooldown.Value, Settings.Flask5UseWhenAvailable.Value, hpPercent, manaPercent, _flask5Timer);
        }

        private void ProcessFlask(int slotIndex, bool isEnabled, string flaskType, int hpThreshold, int manaThreshold, byte key, bool cooldownEnable, int cooldownMs, bool useWhenAvailable, float currentHpPercent, float currentManaPercent, Stopwatch timer)
        {
            if (!isEnabled) return;

            bool cooldownReady = !cooldownEnable || timer.ElapsedMilliseconds >= cooldownMs;
            if (!cooldownReady) return;

            if (flaskType == "HP")
            {
                if (currentHpPercent <= hpThreshold)
                {
                    timer.Restart();
                    SendKeyPress(key);
                }
            }
            else if (flaskType == "Mana")
            {
                if (currentManaPercent <= manaThreshold)
                {
                    timer.Restart();
                    SendKeyPress(key);
                }
            }
            else if (flaskType == "Utility")
            {
                bool availabilityReady = !useWhenAvailable || IsUtilityFlaskAvailable(slotIndex);
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

        private bool IsUtilityFlaskAvailable(int index)
        {
            try
            {
                var flaskInventory = GameController.IngameState.ServerData.PlayerInventories.LastOrDefault(x => x.TypeId == InventoryNameE.Flask1);
                if (flaskInventory == null || flaskInventory.Inventory == null) return false;

                var flaskItem = flaskInventory.Inventory[index, 0];
                if (flaskItem?.Address == 0 || flaskItem?.Item?.Address == 0) return false;

                var item = flaskItem.Item;
                if (!item.TryGetComponent<Charges>(out var chargeComponent)) return false;
                if (!item.TryGetComponent<Flask>(out var flask)) return false;

                // Check charges
                bool hasCharges = chargeComponent.NumCharges >= chargeComponent.ChargesPerUse;
                if (!hasCharges) return false;

                // Check active buffs
                if (GameController.Player.TryGetComponent<Buffs>(out var playerBuffs))
                {
                    var buffNames = GetFlaskBuffNames(flask);
                    bool isActive = playerBuffs.BuffsList.Any(b => buffNames.Contains(b.Name) && b.FlaskSlot == index);
                    // "Use When Available" means use when NOT active
                    return !isActive;
                }
            }
            catch (Exception ex)
            {
                DebugWindow.LogError($"[AutoFlask] Error checking flask {index + 1} availability: {ex.Message}");
            }
            return true;
        }

        private void HandleSkills()
        {
            // Optional: Get Mana component to prevent dry firing
            var life = GameController.Player.GetComponent<Life>();
            if (life == null || life.CurMana < 10) return; // Basic mana safety

            // Skill 1 Logic
            if (Settings.Skill1Enabled.Value && _skill1Timer.ElapsedMilliseconds >= Settings.Skill1Cooldown.Value)
            {
                bool canCast = true;
                if (Settings.Skill1TracingEnabled.Value)
                {
                    canCast = TraceMonster(Settings.Skill1TracingRange.Value);
                }

                if (canCast)
                {
                    _skill1Timer.Restart();
                    SendKeyPress((byte)Settings.Skill1Key.Value);
                }
            }

            // Skill 2 Logic
            if (Settings.Skill2Enabled.Value && _skill2Timer.ElapsedMilliseconds >= Settings.Skill2Cooldown.Value)
            {
                bool canCast = true;
                if (Settings.Skill2TracingEnabled.Value)
                {
                    canCast = TraceMonster(Settings.Skill2TracingRange.Value);
                }

                if (canCast)
                {
                    _skill2Timer.Restart();
                    SendKeyPress((byte)Settings.Skill2Key.Value);
                }
            }

            // Skill 3 Logic
            if (Settings.Skill3Enabled.Value && _skill3Timer.ElapsedMilliseconds >= Settings.Skill3Cooldown.Value)
            {
                bool canCast = true;
                if (Settings.Skill3TracingEnabled.Value)
                {
                    canCast = TraceMonster(Settings.Skill3TracingRange.Value);
                }

                if (canCast)
                {
                    _skill3Timer.Restart();
                    SendKeyPress((byte)Settings.Skill3Key.Value);
                }
            }
        }

        private void SendKeyPress(byte key)
        {
            keybd_event(key, 0, 0, 0);
            Thread.Sleep(25);
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
        }

        public override void Render()
        {
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value) return;

            var life = GameController.Player.GetComponent<Life>();
            float hpPercent = life != null ? (float)life.CurHP / life.MaxHP * 100 : 0;
            float manaPercent = life != null ? (float)life.CurMana / life.MaxMana * 100 : 0;
            
            string areaTimeStr = "00:00:00";
            if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var currentStopwatch))
            {
                var ts = currentStopwatch.Elapsed;
                areaTimeStr = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
            }

            var skillCooldowns = new List<string>();
            if (Settings.Skill1Enabled.Value)
            {
                skillCooldowns.Add($"S1: {Math.Max(0, Settings.Skill1Cooldown.Value - _skill1Timer.ElapsedMilliseconds)}ms");
            }
            if (Settings.Skill2Enabled.Value)
            {
                skillCooldowns.Add($"S2: {Math.Max(0, Settings.Skill2Cooldown.Value - _skill2Timer.ElapsedMilliseconds)}ms");
            }
            if (Settings.Skill3Enabled.Value)
            {
                skillCooldowns.Add($"S3: {Math.Max(0, Settings.Skill3Cooldown.Value - _skill3Timer.ElapsedMilliseconds)}ms");
            }
            string skillCdStr = skillCooldowns.Count > 0 ? string.Join(" | ", skillCooldowns) : "None";

            var drawPos = new Vector2(Settings.HudPositionX.Value, Settings.HudPositionY.Value);

            // Line 1: Auto
            Graphics.DrawText("Auto [ON]", drawPos, Color.Cyan);
            drawPos.Y += 20;

            // Line 2: HP|Mana
            Graphics.DrawText($"HP: {hpPercent:F0}% | Mana: {manaPercent:F0}%", drawPos, Color.Cyan);
            drawPos.Y += 20;

            // Line 3: Area Time
            Graphics.DrawText($"Area Time ({_currentAreaName}): {areaTimeStr}", drawPos, Color.Cyan);
            drawPos.Y += 20;

            // Line 4: Skill Cool Down
            Graphics.DrawText($"Skill Cooldown: {skillCdStr}", drawPos, Color.Cyan);
        }
    }
}