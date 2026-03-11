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
        private readonly Stopwatch _utilityFlaskTimer = new Stopwatch();
        private readonly Stopwatch _lifeFlaskThrottle = new Stopwatch();
        private readonly Stopwatch _skill1Timer = new Stopwatch();
        private readonly Stopwatch _skill2Timer = new Stopwatch();
        private readonly Stopwatch _mouseThrottle = new Stopwatch();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private readonly byte[] _utilityFlaskKeys = { 0x31, 0x32, 0x33, 0x34 };
        private const byte VK_5 = 0x35;

        private Entity _currentTarget;

        public override bool Initialise()
        {
            Name = "Auto Flask & Monster Tracer";
            _utilityFlaskTimer.Start();
            _lifeFlaskThrottle.Start();
            _skill1Timer.Start();
            _skill2Timer.Start();
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

            // 1. Monster Tracing Logic
            HandleMonsterTracing();

            // 2. Flask & Skill Logic
            HandleLifeFlask();
            HandleUtilityFlasks();
            HandleSkills();

            return base.Tick();
        }

        private void HandleMonsterTracing()
        {
            if (!Settings.IsTracingMonstersEnabled.Value)
            {
                _currentTarget = null;
                return;
            }

            // Safety: Don't move mouse if game is not the active window
            if (!GameController.Window.IsForeground()) return;

            if (_mouseThrottle.ElapsedMilliseconds < 50) return;
            _mouseThrottle.Restart();

            _currentTarget = GameController.EntityListWrapper.Entities
                .Where(e => e.Type == EntityType.Monster && 
                            e.IsHostile && 
                            e.IsAlive && 
                            e.IsTargetable &&
                            e.DistancePlayer <= 80)
                .OrderByDescending(e => (int)e.Rarity)
                .ThenBy(e => e.DistancePlayer)
                .FirstOrDefault();

            if (_currentTarget != null)
            {
                var screenPos = GameController.IngameState.Camera.WorldToScreen(_currentTarget.Pos);
                if (screenPos != Vector2.Zero)
                {
                    // Use the built-in Input.SetCursorPos from ExileCore
                    // We multiply by GameController.Window.GetWindowRectangle().Location 
                    // only if the API requires absolute screen coordinates. 
                    // Standard ExileCore Input.SetCursorPos usually handles this.
                    var windowRect = GameController.Window.GetWindowRectangle();
                    Input.SetCursorPos(screenPos + windowRect.Location);
                }
            }
        }

        private void HandleLifeFlask()
        {
            if (!Settings.IsLifeFlaskEnabled.Value || _lifeFlaskThrottle.ElapsedMilliseconds < 600) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100;
            if (hpPercent <= Settings.LifeFlaskPercentage.Value)
            {
                _lifeFlaskThrottle.Restart();
                SendKeyPress(VK_5);
            }
        }

        private void HandleUtilityFlasks()
        {
            if (!Settings.IsUltilityFlaskEnabled.Value || _utilityFlaskTimer.ElapsedMilliseconds < Settings.TimeBetweenActions.Value) return;

            _utilityFlaskTimer.Restart();
            foreach (var key in _utilityFlaskKeys)
            {
                SendKeyPress(key);
                Thread.Sleep(15);
            }
        }

        private void HandleSkills()
        {
            if (!Settings.IsSkillEnabled.Value) return;

            if (_skill1Timer.ElapsedMilliseconds >= Settings.Skill1Cooldown.Value)
            {
                _skill1Timer.Restart();
                SendKeyPress((byte)Settings.Skill1.Hotkey.Value);
            }

            if (_skill2Timer.ElapsedMilliseconds >= Settings.Skill2Cooldown.Value)
            {
                _skill2Timer.Restart();
                SendKeyPress((byte)Settings.Skill2.Hotkey.Value);
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

            // Recalculate variables for drawing
            var life = GameController.Player.GetComponent<Life>();
            float hpPercent = life != null ? (float)life.CurHP / life.MaxHP * 100 : 0;
            var flaskCountdown = Math.Max(0, Settings.TimeBetweenActions.Value - _utilityFlaskTimer.ElapsedMilliseconds);
            
            var drawPos = new Vector2(30, 120);
            
            // Single line status display in English
            Graphics.DrawText($"Auto [ON] | Next Flask: {flaskCountdown}ms | HP: {hpPercent:F0}% | Tracer: {(_currentTarget != null ? "Targeting" : "Scanning")}", drawPos, Color.Cyan);
        }
    }
}