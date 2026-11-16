using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace AutoFlask
{
    public class AutoFlaskSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode IsAutoFlaskEnabled { get; set; } = new ToggleNode(false);

        [Menu("Master Hotkey: Toggle Auto Flask")]
        public HotkeyNodeV2 ToggleAutoFlask { get; set; } = new HotkeyNodeV2(Keys.PageDown);

        #region Safety & General Settings
        [Menu("Safety & General Settings", 50)]
        public EmptyNode SafetyHeader { get; set; }

        [Menu("Only Run When Game Is Focused", 51, 50)]
        public ToggleNode OnlyRunWhenForeground { get; set; } = new ToggleNode(true);

        [Menu("Pause In Town And Hideout", 52, 50)]
        public ToggleNode PauseInTownAndHideout { get; set; } = new ToggleNode(true);

        [Menu("Key Press Duration (ms)", 53, 50)]
        public RangeNode<int> KeyPressDurationMs { get; set; } = new RangeNode<int>(20, 5, 100);
        #endregion

        #region Flask Settings
        [Menu("Flask Settings", 100)]
        public EmptyNode FlaskHeader { get; set; }

        [Menu("Enable Auto Flasks Group", 101, 100)]
        public ToggleNode IsAutoFlasksGroupEnabled { get; set; } = new ToggleNode(true);

        // Flask 1
        [Menu("Flask 1 Settings", 110, 100)] public EmptyNode Flask1Header { get; set; }
        [Menu("Enable Flask 1", 111, 110)] public ToggleNode IsFlask1Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 1 Type", 112, 110)] public ListNode Flask1Type { get; set; } = new ListNode { Value = "HP", Values = new List<string> { "HP", "Mana", "Utility" } };
        [Menu("Use % Threshold (HP/Mana)", 113, 110)] public RangeNode<int> Flask1Percentage { get; set; } = new RangeNode<int>(40, 10, 100);
        [Menu("Key 1", 114, 110)] public HotkeyNodeV2 Flask1Key { get; set; } = new HotkeyNodeV2(Keys.D1);
        [Menu("Enable Cooldown 1", 115, 110)] public ToggleNode Flask1CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 1 (ms)", 116, 110)] public RangeNode<int> Flask1Cooldown { get; set; } = new RangeNode<int>(600, 0, 60000);
        [Menu("Use When Available 1 (Utility)", 117, 110)] public ToggleNode Flask1UseWhenAvailable { get; set; } = new ToggleNode(false);

        // Flask 2
        [Menu("Flask 2 Settings", 120, 100)] public EmptyNode Flask2Header { get; set; }
        [Menu("Enable Flask 2", 121, 120)] public ToggleNode IsFlask2Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 2 Type", 122, 120)] public ListNode Flask2Type { get; set; } = new ListNode { Value = "Utility", Values = new List<string> { "HP", "Mana", "Utility" } };
        [Menu("Use % Threshold (HP/Mana)", 123, 120)] public RangeNode<int> Flask2Percentage { get; set; } = new RangeNode<int>(30, 10, 100);
        [Menu("Key 2", 124, 120)] public HotkeyNodeV2 Flask2Key { get; set; } = new HotkeyNodeV2(Keys.D2);
        [Menu("Enable Cooldown 2", 125, 120)] public ToggleNode Flask2CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 2 (ms)", 126, 120)] public RangeNode<int> Flask2Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 2 (Utility)", 127, 120)] public ToggleNode Flask2UseWhenAvailable { get; set; } = new ToggleNode(false);

        // Flask 3
        [Menu("Flask 3 Settings", 130, 100)] public EmptyNode Flask3Header { get; set; }
        [Menu("Enable Flask 3", 131, 130)] public ToggleNode IsFlask3Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 3 Type", 132, 130)] public ListNode Flask3Type { get; set; } = new ListNode { Value = "Utility", Values = new List<string> { "HP", "Mana", "Utility" } };
        [Menu("Use % Threshold (HP/Mana)", 133, 130)] public RangeNode<int> Flask3Percentage { get; set; } = new RangeNode<int>(20, 10, 100);
        [Menu("Key 3", 134, 130)] public HotkeyNodeV2 Flask3Key { get; set; } = new HotkeyNodeV2(Keys.D3);
        [Menu("Enable Cooldown 3", 135, 130)] public ToggleNode Flask3CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 3 (ms)", 136, 130)] public RangeNode<int> Flask3Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 3 (Utility)", 137, 130)] public ToggleNode Flask3UseWhenAvailable { get; set; } = new ToggleNode(false);

        // Flask 4
        [Menu("Flask 4 Settings", 140, 100)] public EmptyNode Flask4Header { get; set; }
        [Menu("Enable Flask 4", 141, 140)] public ToggleNode IsFlask4Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 4 Type", 142, 140)] public ListNode Flask4Type { get; set; } = new ListNode { Value = "Utility", Values = new List<string> { "HP", "Mana", "Utility" } };
        [Menu("Use % Threshold (HP/Mana)", 143, 140)] public RangeNode<int> Flask4Percentage { get; set; } = new RangeNode<int>(15, 10, 100);
        [Menu("Key 4", 144, 140)] public HotkeyNodeV2 Flask4Key { get; set; } = new HotkeyNodeV2(Keys.D4);
        [Menu("Enable Cooldown 4", 145, 140)] public ToggleNode Flask4CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 4 (ms)", 146, 140)] public RangeNode<int> Flask4Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 4 (Utility)", 147, 140)] public ToggleNode Flask4UseWhenAvailable { get; set; } = new ToggleNode(false);

        // Flask 5
        [Menu("Flask 5 Settings", 150, 100)] public EmptyNode Flask5Header { get; set; }
        [Menu("Enable Flask 5", 151, 150)] public ToggleNode IsFlask5Enabled { get; set; } = new ToggleNode(true);
        [Menu("Flask 5 Type", 152, 150)] public ListNode Flask5Type { get; set; } = new ListNode { Value = "Utility", Values = new List<string> { "HP", "Mana", "Utility" } };
        [Menu("Use % Threshold (HP/Mana)", 153, 150)] public RangeNode<int> Flask5Percentage { get; set; } = new RangeNode<int>(10, 10, 100);
        [Menu("Key 5", 154, 150)] public HotkeyNodeV2 Flask5Key { get; set; } = new HotkeyNodeV2(Keys.D5);
        [Menu("Enable Cooldown 5", 155, 150)] public ToggleNode Flask5CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 5 (ms)", 156, 150)] public RangeNode<int> Flask5Cooldown { get; set; } = new RangeNode<int>(5000, 0, 60000);
        [Menu("Use When Available 5 (Utility)", 157, 150)] public ToggleNode Flask5UseWhenAvailable { get; set; } = new ToggleNode(false);
        #endregion

        #region Skill & Targeting Settings
        [Menu("Skill & Targeting Settings", 300)]
        public EmptyNode SkillHeader { get; set; }

        [Menu("Min Mana Safety for Skills", 301, 300)]
        public RangeNode<int> MinManaForSkills { get; set; } = new RangeNode<int>(10, 0, 100);

        // Skill 1
        [Menu("Skill 1 Settings", 310, 300)] public EmptyNode Skill1Header { get; set; }
        [Menu("Enable Skill 1", 311, 310)] public ToggleNode Skill1Enabled { get; set; } = new ToggleNode(false);
        [Menu("Is Vaal Skill (Phụ thuộc Soul)", 312, 310)] public ToggleNode Skill1IsVaalSkill { get; set; } = new ToggleNode(false);
        [Menu("Skill 1 Key", 313, 310)] public HotkeyNodeV2 Skill1Key { get; set; } = new HotkeyNodeV2(Keys.W);
        [Menu("Skill 1 Cooldown", 314, 310)] public RangeNode<int> Skill1Cooldown { get; set; } = new RangeNode<int>(5000, 100, 30000);
        [Menu("Use When Available (Skill 1)", 315, 310)] public ToggleNode Skill1UseWhenAvailable { get; set; } = new ToggleNode(false);
        [Menu("Skill 1 Tracing", 316, 310)] public ToggleNode Skill1TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 1 Tracing Range", 317, 310)] public RangeNode<int> Skill1TracingRange { get; set; } = new RangeNode<int>(60, 50, 150);

        // Skill 2
        [Menu("Skill 2 Settings", 320, 300)] public EmptyNode Skill2Header { get; set; }
        [Menu("Enable Skill 2", 321, 320)] public ToggleNode Skill2Enabled { get; set; } = new ToggleNode(false);
        [Menu("Is Vaal Skill (Phụ thuộc Soul)", 322, 320)] public ToggleNode Skill2IsVaalSkill { get; set; } = new ToggleNode(false);
        [Menu("Skill 2 Key", 323, 320)] public HotkeyNodeV2 Skill2Key { get; set; } = new HotkeyNodeV2(Keys.E);
        [Menu("Skill 2 Cooldown", 324, 320)] public RangeNode<int> Skill2Cooldown { get; set; } = new RangeNode<int>(10000, 100, 30000);
        [Menu("Use When Available (Skill 2)", 325, 320)] public ToggleNode Skill2UseWhenAvailable { get; set; } = new ToggleNode(false);
        [Menu("Skill 2 Tracing", 326, 320)] public ToggleNode Skill2TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 2 Tracing Range", 327, 320)] public RangeNode<int> Skill2TracingRange { get; set; } = new RangeNode<int>(60, 50, 150);

        // Skill 3
        [Menu("Skill 3 Settings", 330, 300)] public EmptyNode Skill3Header { get; set; }
        [Menu("Enable Skill 3", 331, 330)] public ToggleNode Skill3Enabled { get; set; } = new ToggleNode(false);
        [Menu("Is Vaal Skill (Phụ thuộc Soul)", 332, 330)] public ToggleNode Skill3IsVaalSkill { get; set; } = new ToggleNode(false);
        [Menu("Skill 3 Key", 333, 330)] public HotkeyNodeV2 Skill3Key { get; set; } = new HotkeyNodeV2(Keys.R);
        [Menu("Skill 3 Cooldown", 334, 330)] public RangeNode<int> Skill3Cooldown { get; set; } = new RangeNode<int>(4000, 100, 30000);
        [Menu("Use When Available (Skill 3)", 335, 330)] public ToggleNode Skill3UseWhenAvailable { get; set; } = new ToggleNode(false);
        [Menu("Skill 3 Tracing", 336, 330)] public ToggleNode Skill3TracingEnabled { get; set; } = new ToggleNode(false);
        [Menu("Skill 3 Tracing Range", 337, 330)] public RangeNode<int> Skill3TracingRange { get; set; } = new RangeNode<int>(60, 50, 150);
        #endregion

        #region HUD & Display Settings
        [Menu("HUD & Display Settings", 500)]
        public EmptyNode HudHeader { get; set; }

        [Menu("Show On-Screen HUD", 501, 500)]
        public ToggleNode ShowHud { get; set; } = new ToggleNode(true);

        [Menu("HUD X Position", 502, 500)]
        public RangeNode<int> HudPositionX { get; set; } = new RangeNode<int>(30, 0, 3840);

        [Menu("HUD Y Position", 503, 500)]
        public RangeNode<int> HudPositionY { get; set; } = new RangeNode<int>(170, 0, 2160);

        [Menu("Show HP & Mana on HUD", 504, 500)]
        public ToggleNode ShowVitalsOnHud { get; set; } = new ToggleNode(true);

        [Menu("Show Energy Shield on HUD", 505, 500)]
        public ToggleNode ShowEnergyShieldOnHud { get; set; } = new ToggleNode(true);

        [Menu("Show Area & Time on HUD", 506, 500)]
        public ToggleNode ShowAreaTimeOnHud { get; set; } = new ToggleNode(true);

        [Menu("Show Skill Cooldowns on HUD", 507, 500)]
        public ToggleNode ShowSkillsOnHud { get; set; } = new ToggleNode(true);

        [Menu("Show Skill Hotkeys on HUD", 508, 500)]
        public ToggleNode ShowSkillKeysOnHud { get; set; } = new ToggleNode(true);

        [Menu("Show Flask Status on HUD", 509, 500)]
        public ToggleNode ShowFlaskStatusOnHud { get; set; } = new ToggleNode(true);

        [Menu("Draw HUD Background Box", 510, 500)]
        public ToggleNode DrawHudBackground { get; set; } = new ToggleNode(false);
        #endregion
    }
}
