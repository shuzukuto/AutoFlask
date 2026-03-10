using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using SharpDX;

namespace AutoFlask
{
    public class AutoFlask : BaseSettingsPlugin<AutoFlaskSettings>
    {
        // Timers for different categories
        private readonly Stopwatch _utilityFlaskTimer = new Stopwatch();
        private readonly Stopwatch _lifeFlaskThrottle = new Stopwatch();
        private readonly Stopwatch _skill1Timer = new Stopwatch();
        private readonly Stopwatch _skill2Timer = new Stopwatch();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private readonly byte[] _utilityFlaskKeys = { 0x31, 0x32, 0x33, 0x34 }; // Keys 1-4
        private const byte VK_5 = 0x35; // Key 5

        public override bool Initialise()
        {
            Name = "Auto Flask & Skill Pro";
            
            // Start all timers
            _utilityFlaskTimer.Start();
            _lifeFlaskThrottle.Start();
            _skill1Timer.Start();
            _skill2Timer.Start();

            return true;
        }

        public override Job Tick()
        {
            // Toggle Logic via Hotkey
            if (Settings.ToggleAutoFlask.PressedOnce())
            {
                Settings.IsAutoFlaskEnabled.Value = !Settings.IsAutoFlaskEnabled.Value;
                DebugWindow.LogMsg($"[AutoFlask] Status: {Settings.IsAutoFlaskEnabled.Value}", 3, Color.Yellow);
            }

            // Global and Game State Checks
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value)
                return base.Tick();

            if (!GameController.InGame || GameController.IsLoading || !GameController.Player.IsAlive)
                return base.Tick();

            // 1. Emergency Life Flask (Highest Priority)
            HandleLifeFlask();

            // 2. Utility Flasks (Interval based)
            HandleUtilityFlasks();

            // 3. Skill Management (Individual Cooldowns)
            HandleSkills();

            return base.Tick();
        }

        private void HandleLifeFlask()
        {
            if (!Settings.IsLifeFlaskEnabled.Value) return;
            if (_lifeFlaskThrottle.ElapsedMilliseconds < 500) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100;
            if (hpPercent <= Settings.LifeFlaskPercentage.Value)
            {
                _lifeFlaskThrottle.Restart();
                SendKeyPress(VK_5);
                DebugWindow.LogMsg($"[AutoFlask] Emergency HP: {hpPercent:F0}% - Key 5 Sent", 2, Color.Red);
            }
        }

        private void HandleUtilityFlasks()
        {
            if (!Settings.IsUltilityFlaskEnabled.Value) return;
            if (_utilityFlaskTimer.ElapsedMilliseconds < Settings.TimeBetweenActions.Value) return;

            _utilityFlaskTimer.Restart();
            foreach (var key in _utilityFlaskKeys)
            {
                SendKeyPress(key);
                Thread.Sleep(15); // Tiny gap between flask presses
            }
        }

        private void HandleSkills()
        {
            if (!Settings.IsSkillEnabled.Value) return;

            // Handle Skill 1
            if (_skill1Timer.ElapsedMilliseconds >= Settings.Skill1Cooldown.Value)
            {
                _skill1Timer.Restart();
                SendKeyPress((byte)Settings.Skill1.Hotkey.Value);
                Thread.Sleep(10);
            }

            // Handle Skill 2
            if (_skill2Timer.ElapsedMilliseconds >= Settings.Skill2Cooldown.Value)
            {
                _skill2Timer.Restart();
                SendKeyPress((byte)Settings.Skill2.Hotkey.Value);
            }
        }

        private void SendKeyPress(byte key)
        {
            // Authentic Press/Release with standard 25ms delay
            keybd_event(key, 0, 0, 0); // Down
            Thread.Sleep(25);
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0); // Up
        }

        public override void Render()
        {
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value) return;

            var life = GameController.Player.GetComponent<Life>();
            if (life == null) return;

            float hpPercent = (float)life.CurHP / life.MaxHP * 100;
            var flaskCountdown = Math.Max(0, Settings.TimeBetweenActions.Value - _utilityFlaskTimer.ElapsedMilliseconds);
            
            var drawPos = new Vector2(30, 120);
            
            // UI Overlay
            Graphics.DrawText($"Auto [ON] | Flask: {flaskCountdown}ms | HP: {hpPercent:F0}%", drawPos, Color.Cyan);

            // Cooldown Indicators for Skills
            var s1Cd = Math.Max(0, Settings.Skill1Cooldown.Value - _skill1Timer.ElapsedMilliseconds);
            var s2Cd = Math.Max(0, Settings.Skill2Cooldown.Value - _skill2Timer.ElapsedMilliseconds);
            
            Graphics.DrawText($"S1 CD: {s1Cd}ms | S2 CD: {s2Cd}ms", drawPos + new Vector2(0, 20), Color.White);

            if (hpPercent <= Settings.LifeFlaskPercentage.Value && Settings.IsLifeFlaskEnabled.Value)
            {
                Graphics.DrawText("!! LOW HEALTH !!", drawPos + new Vector2(0, 40), Color.Red);
            }
        }
    }
}