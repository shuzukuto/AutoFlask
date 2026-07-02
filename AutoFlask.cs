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
        private readonly Stopwatch _utilityFlask1Timer = new Stopwatch();
        private readonly Stopwatch _utilityFlask2Timer = new Stopwatch();
        private readonly Stopwatch _utilityFlask3Timer = new Stopwatch();
        private readonly Stopwatch _utilityFlask4Timer = new Stopwatch();
        private readonly Stopwatch _utilityFlask5Timer = new Stopwatch();

        private readonly Stopwatch _lifeFlask1Throttle = new Stopwatch();
        private readonly Stopwatch _lifeFlask2Throttle = new Stopwatch();
        private readonly Stopwatch _lifeFlask3Throttle = new Stopwatch();
        private readonly Stopwatch _lifeFlask4Throttle = new Stopwatch();
        private readonly Stopwatch _lifeFlask5Throttle = new Stopwatch();

        private readonly Stopwatch _manaFlask1Throttle = new Stopwatch();
        private readonly Stopwatch _manaFlask2Throttle = new Stopwatch();
        private readonly Stopwatch _manaFlask3Throttle = new Stopwatch();
        private readonly Stopwatch _manaFlask4Throttle = new Stopwatch();
        private readonly Stopwatch _manaFlask5Throttle = new Stopwatch();

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
            _utilityFlask1Timer.Start();
            _utilityFlask2Timer.Start();
            _utilityFlask3Timer.Start();
            _utilityFlask4Timer.Start();
            _utilityFlask5Timer.Start();

            _lifeFlask1Throttle.Start();
            _lifeFlask2Throttle.Start();
            _lifeFlask3Throttle.Start();
            _lifeFlask4Throttle.Start();
            _lifeFlask5Throttle.Start();

            _manaFlask1Throttle.Start();
            _manaFlask2Throttle.Start();
            _manaFlask3Throttle.Start();
            _manaFlask4Throttle.Start();
            _manaFlask5Throttle.Start();

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

            HandleLifeFlask();
            HandleManaFlask();
            HandleUtilityFlasks();
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

        private void HandleLifeFlask()
        {
            if (!Settings.IsLifeFlaskEnabled.Value) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100;

            if (Settings.IsLifeFlask1Enabled.Value && hpPercent <= Settings.LifeFlask1Percentage.Value)
            {
                if (!Settings.LifeFlask1CooldownEnable.Value || _lifeFlask1Throttle.ElapsedMilliseconds >= Settings.LifeFlask1Cooldown.Value)
                {
                    _lifeFlask1Throttle.Restart();
                    SendKeyPress((byte)Settings.LifeFlask1Key.Value);
                }
            }

            if (Settings.IsLifeFlask2Enabled.Value && hpPercent <= Settings.LifeFlask2Percentage.Value)
            {
                if (!Settings.LifeFlask2CooldownEnable.Value || _lifeFlask2Throttle.ElapsedMilliseconds >= Settings.LifeFlask2Cooldown.Value)
                {
                    _lifeFlask2Throttle.Restart();
                    SendKeyPress((byte)Settings.LifeFlask2Key.Value);
                }
            }

            if (Settings.IsLifeFlask3Enabled.Value && hpPercent <= Settings.LifeFlask3Percentage.Value)
            {
                if (!Settings.LifeFlask3CooldownEnable.Value || _lifeFlask3Throttle.ElapsedMilliseconds >= Settings.LifeFlask3Cooldown.Value)
                {
                    _lifeFlask3Throttle.Restart();
                    SendKeyPress((byte)Settings.LifeFlask3Key.Value);
                }
            }

            if (Settings.IsLifeFlask4Enabled.Value && hpPercent <= Settings.LifeFlask4Percentage.Value)
            {
                if (!Settings.LifeFlask4CooldownEnable.Value || _lifeFlask4Throttle.ElapsedMilliseconds >= Settings.LifeFlask4Cooldown.Value)
                {
                    _lifeFlask4Throttle.Restart();
                    SendKeyPress((byte)Settings.LifeFlask4Key.Value);
                }
            }

            if (Settings.IsLifeFlask5Enabled.Value && hpPercent <= Settings.LifeFlask5Percentage.Value)
            {
                if (!Settings.LifeFlask5CooldownEnable.Value || _lifeFlask5Throttle.ElapsedMilliseconds >= Settings.LifeFlask5Cooldown.Value)
                {
                    _lifeFlask5Throttle.Restart();
                    SendKeyPress((byte)Settings.LifeFlask5Key.Value);
                }
            }
        }

        private void HandleManaFlask()
        {
            if (!Settings.IsManaFlaskEnabled.Value) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float manaPercent = (float)life.CurMana / life.MaxMana * 100;

            if (Settings.IsManaFlask1Enabled.Value && manaPercent <= Settings.ManaFlask1Percentage.Value)
            {
                if (!Settings.ManaFlask1CooldownEnable.Value || _manaFlask1Throttle.ElapsedMilliseconds >= Settings.ManaFlask1Cooldown.Value)
                {
                    _manaFlask1Throttle.Restart();
                    SendKeyPress((byte)Settings.ManaFlask1Key.Value);
                }
            }

            if (Settings.IsManaFlask2Enabled.Value && manaPercent <= Settings.ManaFlask2Percentage.Value)
            {
                if (!Settings.ManaFlask2CooldownEnable.Value || _manaFlask2Throttle.ElapsedMilliseconds >= Settings.ManaFlask2Cooldown.Value)
                {
                    _manaFlask2Throttle.Restart();
                    SendKeyPress((byte)Settings.ManaFlask2Key.Value);
                }
            }

            if (Settings.IsManaFlask3Enabled.Value && manaPercent <= Settings.ManaFlask3Percentage.Value)
            {
                if (!Settings.ManaFlask3CooldownEnable.Value || _manaFlask3Throttle.ElapsedMilliseconds >= Settings.ManaFlask3Cooldown.Value)
                {
                    _manaFlask3Throttle.Restart();
                    SendKeyPress((byte)Settings.ManaFlask3Key.Value);
                }
            }

            if (Settings.IsManaFlask4Enabled.Value && manaPercent <= Settings.ManaFlask4Percentage.Value)
            {
                if (!Settings.ManaFlask4CooldownEnable.Value || _manaFlask4Throttle.ElapsedMilliseconds >= Settings.ManaFlask4Cooldown.Value)
                {
                    _manaFlask4Throttle.Restart();
                    SendKeyPress((byte)Settings.ManaFlask4Key.Value);
                }
            }

            if (Settings.IsManaFlask5Enabled.Value && manaPercent <= Settings.ManaFlask5Percentage.Value)
            {
                if (!Settings.ManaFlask5CooldownEnable.Value || _manaFlask5Throttle.ElapsedMilliseconds >= Settings.ManaFlask5Cooldown.Value)
                {
                    _manaFlask5Throttle.Restart();
                    SendKeyPress((byte)Settings.ManaFlask5Key.Value);
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

        private void HandleUtilityFlasks()
        {
            if (!Settings.IsUltilityFlaskEnabled.Value) return;

            // Utility Flask 1
            if (Settings.IsUtilityFlask1Enabled.Value)
            {
                bool cooldownReady = !Settings.UtilityFlask1CooldownEnable.Value || 
                                     _utilityFlask1Timer.ElapsedMilliseconds >= Settings.UtilityFlask1Cooldown.Value;
                bool availabilityReady = !Settings.UtilityFlask1UseWhenAvailable.Value || 
                                         IsUtilityFlaskAvailable(0);

                if (cooldownReady && availabilityReady)
                {
                    _utilityFlask1Timer.Restart();
                    SendKeyPress((byte)Settings.UtilityFlask1Key.Value);
                }
            }

            // Utility Flask 2
            if (Settings.IsUtilityFlask2Enabled.Value)
            {
                bool cooldownReady = !Settings.UtilityFlask2CooldownEnable.Value || 
                                     _utilityFlask2Timer.ElapsedMilliseconds >= Settings.UtilityFlask2Cooldown.Value;
                bool availabilityReady = !Settings.UtilityFlask2UseWhenAvailable.Value || 
                                         IsUtilityFlaskAvailable(1);

                if (cooldownReady && availabilityReady)
                {
                    _utilityFlask2Timer.Restart();
                    SendKeyPress((byte)Settings.UtilityFlask2Key.Value);
                }
            }

            // Utility Flask 3
            if (Settings.IsUtilityFlask3Enabled.Value)
            {
                bool cooldownReady = !Settings.UtilityFlask3CooldownEnable.Value || 
                                     _utilityFlask3Timer.ElapsedMilliseconds >= Settings.UtilityFlask3Cooldown.Value;
                bool availabilityReady = !Settings.UtilityFlask3UseWhenAvailable.Value || 
                                         IsUtilityFlaskAvailable(2);

                if (cooldownReady && availabilityReady)
                {
                    _utilityFlask3Timer.Restart();
                    SendKeyPress((byte)Settings.UtilityFlask3Key.Value);
                }
            }

            // Utility Flask 4
            if (Settings.IsUtilityFlask4Enabled.Value)
            {
                bool cooldownReady = !Settings.UtilityFlask4CooldownEnable.Value || 
                                     _utilityFlask4Timer.ElapsedMilliseconds >= Settings.UtilityFlask4Cooldown.Value;
                bool availabilityReady = !Settings.UtilityFlask4UseWhenAvailable.Value || 
                                         IsUtilityFlaskAvailable(3);

                if (cooldownReady && availabilityReady)
                {
                    _utilityFlask4Timer.Restart();
                    SendKeyPress((byte)Settings.UtilityFlask4Key.Value);
                }
            }

            // Utility Flask 5
            if (Settings.IsUtilityFlask5Enabled.Value)
            {
                bool cooldownReady = !Settings.UtilityFlask5CooldownEnable.Value || 
                                     _utilityFlask5Timer.ElapsedMilliseconds >= Settings.UtilityFlask5Cooldown.Value;
                bool availabilityReady = !Settings.UtilityFlask5UseWhenAvailable.Value || 
                                         IsUtilityFlaskAvailable(4);

                if (cooldownReady && availabilityReady)
                {
                    _utilityFlask5Timer.Restart();
                    SendKeyPress((byte)Settings.UtilityFlask5Key.Value);
                }
            }
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
            
            var drawPos = new Vector2(30, 120);

            string areaTimeStr = "00:00:00";
            if (!string.IsNullOrEmpty(_currentAreaName) && _areaTimers.TryGetValue(_currentAreaName, out var currentStopwatch))
            {
                var ts = currentStopwatch.Elapsed;
                areaTimeStr = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
            }

            Graphics.DrawText($"Auto [ON] | HP: {hpPercent:F0}% | Mana: {manaPercent:F0}% | Area Time ({_currentAreaName}): {areaTimeStr} | S3 CD: {Math.Max(0, Settings.Skill3Cooldown.Value - _skill3Timer.ElapsedMilliseconds)}ms", drawPos, Color.Cyan);
        }
    }
}