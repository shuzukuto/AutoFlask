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
        private readonly Stopwatch _flaskTimer = new Stopwatch();
        private readonly Stopwatch _lifeFlaskThrottle = new Stopwatch();

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private readonly byte[] _flaskKeys = { 0x31, 0x32, 0x33, 0x34};
        private const byte VK_5 = 0x35;

        public override bool Initialise()
        {
            Name = "Auto Flask Trigger";
            _flaskTimer.Start();
            _lifeFlaskThrottle.Start();
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

            // Global and State Checks
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value)
                return base.Tick();

            if (!GameController.InGame || GameController.IsLoading || !GameController.Player.IsAlive)
                return base.Tick();

            // 1. Check Life Flask Logic (Health-based)
            HandleLifeFlaskLogic();

            // 2. Check Standard Interval Logic (Keys 1-5)
            HandleFlaskIntervalLogic();

            return base.Tick();
        }

        private void HandleLifeFlaskLogic()
        {
            if (!Settings.IsLifeFlaskEnabled.Value) return;

            // Throttle to prevent spamming the key every frame (300ms delay)
            if (_lifeFlaskThrottle.ElapsedMilliseconds < 300) return;

            var lifeComponent = GameController.Player.GetComponent<Life>();
            if (lifeComponent == null) return;

            // Calculate HP percentage: (Current / Max) * 100
            float hpPercentage = (float)lifeComponent.CurHP / lifeComponent.MaxHP * 100;

            if (hpPercentage <= Settings.LifeFlaskPercentage.Value)
            {
                _lifeFlaskThrottle.Restart();
                DebugWindow.LogMsg($"[AutoFlask] Emergency Life Flask! HP: {hpPercentage:F1}%", 2, Color.Red);
                
                // Press Key 5
                keybd_event(VK_5, 0, 0, 0);
                Thread.Sleep(25);
                keybd_event(VK_5, 0, KEYEVENTF_KEYUP, 0);
            }
        }

        private void HandleFlaskIntervalLogic()
        {
            if (_flaskTimer.ElapsedMilliseconds >= Settings.TimeBetweenActions.Value)
            {
                _flaskTimer.Restart();
                
                foreach (var key in _flaskKeys)
                {
                    keybd_event(key, 0, 0, 0);
                    Thread.Sleep(25);
                    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
                    Thread.Sleep(20);
                }
                
                DebugWindow.LogMsg("[AutoFlask] Interval sequence 1-5 executed.", 1, Color.SpringGreen);
            }
        }

        public override void Render()
        {
            if (!Settings.Enable || !Settings.IsAutoFlaskEnabled.Value) return;

            var lifeComponent = GameController.Player.GetComponent<Life>();
            if (lifeComponent == null) return;

            float hpPercentage = (float)lifeComponent.CurHP / lifeComponent.MaxHP * 100;
            var drawPos = new Vector2(30, 150);

            // UI Display in English
            var countdown = Math.Max(0, Settings.TimeBetweenActions.Value - _flaskTimer.ElapsedMilliseconds);
            Graphics.DrawText($"Auto Flask [ON] | Next: {countdown}ms | HP: {hpPercentage:F0}%", drawPos, Color.Cyan);
            
            if (hpPercentage <= Settings.LifeFlaskPercentage.Value && Settings.IsLifeFlaskEnabled.Value)
            {
                Graphics.DrawText("!!! LOW HEALTH !!!", drawPos + new Vector2(0, 20), Color.Red);
            }
        }
    }
}