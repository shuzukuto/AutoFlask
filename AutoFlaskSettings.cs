using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace AutoFlask
{
    public class AutoFlaskSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode IsAutoFlaskEnabled { get; set; } = new ToggleNode(false);
        
        [Menu("Toggle Auto Flask")] public HotkeyNode ToggleAutoFlask { get; set; } = Keys.PageDown;
        
        [Menu("Flask Settings", 100)]
        public EmptyNode FlaskHeader { get; set; }
        [Menu("Enable Auto Flasks", 101, 100)] public ToggleNode IsAutoFlasksGroupEnabled { get; set; } = new ToggleNode(true);

        [Menu("Flask 1 Settings", 110, 100)] public EmptyNode Flask1Header { get; set; }
        [Menu("Enable Flask 1", 111, 110)] public ToggleNode IsFlask1Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 1 Type", 112, 110)] public ListNode Flask1Type { get; set; } = new ListNode { Value = "HP", Values = new System.Collections.Generic.List<string> { "HP", "Mana", "Utility" } };
        [Menu("HP % Threshold (HP Type)", 113, 110)] public RangeNode<int> Flask1HpPercentage { get; set; } = new RangeNode<int>(40, 10, 100);
        [Menu("Mana % Threshold (Mana Type)", 114, 110)] public RangeNode<int> Flask1ManaPercentage { get; set; } = new RangeNode<int>(40, 10, 100);
        [Menu("Key 1", 115, 110)] public HotkeyNode Flask1Key { get; set; } = Keys.D1;
        [Menu("Enable Cooldown 1", 116, 110)] public ToggleNode Flask1CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 1 (ms)", 117, 110)] public RangeNode<int> Flask1Cooldown { get; set; } = new RangeNode<int>(600, 0, 60000);
        [Menu("Use When Available 1 (Utility)", 118, 110)] public ToggleNode Flask1UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Flask 2 Settings", 120, 100)] public EmptyNode Flask2Header { get; set; }
        [Menu("Enable Flask 2", 121, 120)] public ToggleNode IsFlask2Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 2 Type", 122, 120)] public ListNode Flask2Type { get; set; } = new ListNode { Value = "Utility", Values = new System.Collections.Generic.List<string> { "HP", "Mana", "Utility" } };
        [Menu("HP % Threshold (HP Type)", 123, 120)] public RangeNode<int> Flask2HpPercentage { get; set; } = new RangeNode<int>(30, 10, 100);
        [Menu("Mana % Threshold (Mana Type)", 124, 120)] public RangeNode<int> Flask2ManaPercentage { get; set; } = new RangeNode<int>(30, 10, 100);
        [Menu("Key 2", 125, 120)] public HotkeyNode Flask2Key { get; set; } = Keys.D2;
        [Menu("Enable Cooldown 2", 126, 120)] public ToggleNode Flask2CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 2 (ms)", 127, 120)] public RangeNode<int> Flask2Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 2 (Utility)", 128, 120)] public ToggleNode Flask2UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Flask 3 Settings", 130, 100)] public EmptyNode Flask3Header { get; set; }
        [Menu("Enable Flask 3", 131, 130)] public ToggleNode IsFlask3Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 3 Type", 132, 130)] public ListNode Flask3Type { get; set; } = new ListNode { Value = "Utility", Values = new System.Collections.Generic.List<string> { "HP", "Mana", "Utility" } };
        [Menu("HP % Threshold (HP Type)", 133, 130)] public RangeNode<int> Flask3HpPercentage { get; set; } = new RangeNode<int>(20, 10, 100);
        [Menu("Mana % Threshold (Mana Type)", 134, 130)] public RangeNode<int> Flask3ManaPercentage { get; set; } = new RangeNode<int>(20, 10, 100);
        [Menu("Key 3", 135, 130)] public HotkeyNode Flask3Key { get; set; } = Keys.D3;
        [Menu("Enable Cooldown 3", 136, 130)] public ToggleNode Flask3CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 3 (ms)", 137, 130)] public RangeNode<int> Flask3Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 3 (Utility)", 138, 130)] public ToggleNode Flask3UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Flask 4 Settings", 140, 100)] public EmptyNode Flask4Header { get; set; }
        [Menu("Enable Flask 4", 141, 140)] public ToggleNode IsFlask4Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 4 Type", 142, 140)] public ListNode Flask4Type { get; set; } = new ListNode { Value = "Utility", Values = new System.Collections.Generic.List<string> { "HP", "Mana", "Utility" } };
        [Menu("HP % Threshold (HP Type)", 143, 140)] public RangeNode<int> Flask4HpPercentage { get; set; } = new RangeNode<int>(15, 10, 100);
        [Menu("Mana % Threshold (Mana Type)", 144, 140)] public RangeNode<int> Flask4ManaPercentage { get; set; } = new RangeNode<int>(15, 10, 100);
        [Menu("Key 4", 145, 140)] public HotkeyNode Flask4Key { get; set; } = Keys.D4;
        [Menu("Enable Cooldown 4", 146, 140)] public ToggleNode Flask4CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 4 (ms)", 147, 140)] public RangeNode<int> Flask4Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 4 (Utility)", 148, 140)] public ToggleNode Flask4UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Flask 5 Settings", 150, 100)] public EmptyNode Flask5Header { get; set; }
        [Menu("Enable Flask 5", 151, 150)] public ToggleNode IsFlask5Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 5 Type", 152, 150)] public ListNode Flask5Type { get; set; } = new ListNode { Value = "Utility", Values = new System.Collections.Generic.List<string> { "HP", "Mana", "Utility" } };
        [Menu("HP % Threshold (HP Type)", 153, 150)] public RangeNode<int> Flask5HpPercentage { get; set; } = new RangeNode<int>(10, 10, 100);
        [Menu("Mana % Threshold (Mana Type)", 154, 150)] public RangeNode<int> Flask5ManaPercentage { get; set; } = new RangeNode<int>(10, 10, 100);
        [Menu("Key 5", 155, 150)] public HotkeyNode Flask5Key { get; set; } = Keys.D5;
        [Menu("Enable Cooldown 5", 156, 150)] public ToggleNode Flask5CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 5 (ms)", 157, 150)] public RangeNode<int> Flask5Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 5 (Utility)", 158, 150)] public ToggleNode Flask5UseWhenAvailable { get; set; } = new ToggleNode(false);
        
        [Menu("Skill & Targeting Settings", 300)]
        public EmptyNode SkillHeader { get; set; }

        [Menu("Skill 1 Settings", 310, 300)] public EmptyNode Skill1Header { get; set; }
        [Menu("Enable Skill 1", 311, 310)] public ToggleNode Skill1Enabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 1 Key", 312, 310)] public HotkeyNode Skill1Key { get; set; } = Keys.W;
        [Menu("Skill 1 Cooldown", 313, 310)] public RangeNode<int> Skill1Cooldown { get; set; } = new RangeNode<int>(5000, 100, 30000);
        [Menu("Skill 1 Tracing", 314, 310)] public ToggleNode Skill1TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 1 Tracing Range", 315, 310)] public RangeNode<int> Skill1TracingRange { get; set; } = new RangeNode<int>(60, 50, 100);
        
        [Menu("Skill 2 Settings", 320, 300)] public EmptyNode Skill2Header { get; set; }
        [Menu("Enable Skill 2", 321, 320)] public ToggleNode Skill2Enabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 2 Key", 322, 320)] public HotkeyNode Skill2Key { get; set; } = Keys.E;
        [Menu("Skill 2 Cooldown", 323, 320)] public RangeNode<int> Skill2Cooldown { get; set; } = new RangeNode<int>(10000, 100, 30000);
        [Menu("Skill 2 Tracing", 324, 320)] public ToggleNode Skill2TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 2 Tracing Range", 325, 320)] public RangeNode<int> Skill2TracingRange { get; set; } = new RangeNode<int>(60, 50, 100);

        [Menu("Skill 3 Settings", 330, 300)] public EmptyNode Skill3Header { get; set; }
        [Menu("Enable Skill 3", 331, 330)] public ToggleNode Skill3Enabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 3 Key", 332, 330)] public HotkeyNode Skill3Key { get; set; } = Keys.R;
        [Menu("Skill 3 Cooldown", 333, 330)] public RangeNode<int> Skill3Cooldown { get; set; } = new RangeNode<int>(4000, 100, 30000);
        [Menu("Skill 3 Tracing", 334, 330)] public ToggleNode Skill3TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 3 Tracing Range", 335, 330)] public RangeNode<int> Skill3TracingRange { get; set; } = new RangeNode<int>(60, 50, 100);

        [Menu("HUD & Display Settings", 500)]
        public EmptyNode HudHeader { get; set; }
        [Menu("HUD X Position", 501, 500)] public RangeNode<int> HudPositionX { get; set; } = new RangeNode<int>(30, 0, 3840);
        [Menu("HUD Y Position", 502, 500)] public RangeNode<int> HudPositionY { get; set; } = new RangeNode<int>(170, 0, 2160);
    }
}