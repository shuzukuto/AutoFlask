using System;
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

            if (!GameController.InGame || GameController.IsLoading || !GameController.Player.IsAlive)
                return base.Tick();

            HandleMonsterTracing();
            HandleLifeFlask();
            HandleManaFlask();
            HandleUtilityFlasks();
            HandleSkills();

            return base.Tick();
        }

        private void HandleMonsterTracing()
        {
            if (!Settings.IsTracingMonstersEnabled.Value || !GameController.Window.IsForeground())
            {
                _currentTarget = null;
                return;
            }

            if (_mouseThrottle.ElapsedMilliseconds < 50) return;
            _mouseThrottle.Restart();

            _currentTarget = GameController.EntityListWrapper.Entities
                .Where(e => e.Type == EntityType.Monster && e.IsHostile && e.IsAlive && e.IsTargetable && e.DistancePlayer <= 80)
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
                }
            }
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

        private void HandleUtilityFlasks()
        {
            if (!Settings.IsUltilityFlaskEnabled.Value) return;

            if (Settings.IsUtilityFlask1Enabled.Value && _utilityFlask1Timer.ElapsedMilliseconds >= Settings.UtilityFlask1Cooldown.Value)
            {
                _utilityFlask1Timer.Restart();
                SendKeyPress((byte)Settings.UtilityFlask1Key.Value);
            }

            if (Settings.IsUtilityFlask2Enabled.Value && _utilityFlask2Timer.ElapsedMilliseconds >= Settings.UtilityFlask2Cooldown.Value)
            {
                _utilityFlask2Timer.Restart();
                SendKeyPress((byte)Settings.UtilityFlask2Key.Value);
            }

            if (Settings.IsUtilityFlask3Enabled.Value && _utilityFlask3Timer.ElapsedMilliseconds >= Settings.UtilityFlask3Cooldown.Value)
            {
                _utilityFlask3Timer.Restart();
                SendKeyPress((byte)Settings.UtilityFlask3Key.Value);
            }

            if (Settings.IsUtilityFlask4Enabled.Value && _utilityFlask4Timer.ElapsedMilliseconds >= Settings.UtilityFlask4Cooldown.Value)
            {
                _utilityFlask4Timer.Restart();
                SendKeyPress((byte)Settings.UtilityFlask4Key.Value);
            }

            if (Settings.IsUtilityFlask5Enabled.Value && _utilityFlask5Timer.ElapsedMilliseconds >= Settings.UtilityFlask5Cooldown.Value)
            {
                _utilityFlask5Timer.Restart();
                SendKeyPress((byte)Settings.UtilityFlask5Key.Value);
            }
        }

        private void HandleSkills()
        {
            if (!Settings.IsSkillEnabled.Value) return;

            // Optional: Get Mana component to prevent dry firing
            var life = GameController.Player.GetComponent<Life>();
            if (life == null || life.CurMana < 10) return; // Basic mana safety

            // Skill 1 Logic
            if (_skill1Timer.ElapsedMilliseconds >= Settings.Skill1Cooldown.Value)
            {
                _skill1Timer.Restart();
                SendKeyPress((byte)Settings.Skill1Key.Value);
            }

            // Skill 2 Logic
            if (_skill2Timer.ElapsedMilliseconds >= Settings.Skill2Cooldown.Value)
            {
                _skill2Timer.Restart();
                SendKeyPress((byte)Settings.Skill2Key.Value);
            }

            // Skill 3 Logic
            if (_skill3Timer.ElapsedMilliseconds >= Settings.Skill3Cooldown.Value)
            {
                _skill3Timer.Restart();
                SendKeyPress((byte)Settings.Skill3Key.Value);
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
            Graphics.DrawText($"Auto [ON] | HP: {hpPercent:F0}% | Mana: {manaPercent:F0}% | S3 CD: {Math.Max(0, Settings.Skill3Cooldown.Value - _skill3Timer.ElapsedMilliseconds)}ms", drawPos, Color.Cyan);
        }
    }
}