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
        
        [Menu("Utility Flasks Settings", 100)]
        public EmptyNode UtilityHeader { get; set; }
        [Menu("Enable Utility Flasks", 101, 100)] public ToggleNode IsUltilityFlaskEnabled { get; set; } = new ToggleNode(true);
        
        [Menu("Utility Flask 1 Settings", 110, 100)] public EmptyNode UtilityFlask1Header { get; set; }
        [Menu("Enable Utility Flask 1", 111, 110)] public ToggleNode IsUtilityFlask1Enabled { get; set; } = new ToggleNode(true);
        [Menu("Key 1", 112, 110)] public HotkeyNode UtilityFlask1Key { get; set; } = Keys.D1;
        [Menu("Enable Cooldown 1", 113, 110)] public ToggleNode UtilityFlask1CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 1 (ms)", 114, 110)] public RangeNode<int> UtilityFlask1Cooldown { get; set; } = new RangeNode<int>(5000, 10, 60000);
        [Menu("Use When Available 1", 115, 110)] public ToggleNode UtilityFlask1UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Utility Flask 2 Settings", 120, 100)] public EmptyNode UtilityFlask2Header { get; set; }
        [Menu("Enable Utility Flask 2", 121, 120)] public ToggleNode IsUtilityFlask2Enabled { get; set; } = new ToggleNode(true);
        [Menu("Key 2", 122, 120)] public HotkeyNode UtilityFlask2Key { get; set; } = Keys.D2;
        [Menu("Enable Cooldown 2", 123, 120)] public ToggleNode UtilityFlask2CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 2 (ms)", 124, 120)] public RangeNode<int> UtilityFlask2Cooldown { get; set; } = new RangeNode<int>(5000, 10, 60000);
        [Menu("Use When Available 2", 125, 120)] public ToggleNode UtilityFlask2UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Utility Flask 3 Settings", 130, 100)] public EmptyNode UtilityFlask3Header { get; set; }
        [Menu("Enable Utility Flask 3", 131, 130)] public ToggleNode IsUtilityFlask3Enabled { get; set; } = new ToggleNode(true);
        [Menu("Key 3", 132, 130)] public HotkeyNode UtilityFlask3Key { get; set; } = Keys.D3;
        [Menu("Enable Cooldown 3", 133, 130)] public ToggleNode UtilityFlask3CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 3 (ms)", 134, 130)] public RangeNode<int> UtilityFlask3Cooldown { get; set; } = new RangeNode<int>(5000, 10, 60000);
        [Menu("Use When Available 3", 135, 130)] public ToggleNode UtilityFlask3UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Utility Flask 4 Settings", 140, 100)] public EmptyNode UtilityFlask4Header { get; set; }
        [Menu("Enable Utility Flask 4", 141, 140)] public ToggleNode IsUtilityFlask4Enabled { get; set; } = new ToggleNode(true);
        [Menu("Key 4", 142, 140)] public HotkeyNode UtilityFlask4Key { get; set; } = Keys.D4;
        [Menu("Enable Cooldown 4", 143, 140)] public ToggleNode UtilityFlask4CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 4 (ms)", 144, 140)] public RangeNode<int> UtilityFlask4Cooldown { get; set; } = new RangeNode<int>(5000, 10, 60000);
        [Menu("Use When Available 4", 145, 140)] public ToggleNode UtilityFlask4UseWhenAvailable { get; set; } = new ToggleNode(false);

        [Menu("Utility Flask 5 Settings", 150, 100)] public EmptyNode UtilityFlask5Header { get; set; }
        [Menu("Enable Utility Flask 5", 151, 150)] public ToggleNode IsUtilityFlask5Enabled { get; set; } = new ToggleNode(true);
        [Menu("Key 5", 152, 150)] public HotkeyNode UtilityFlask5Key { get; set; } = Keys.D5;
        [Menu("Enable Cooldown 5", 153, 150)] public ToggleNode UtilityFlask5CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 5 (ms)", 154, 150)] public RangeNode<int> UtilityFlask5Cooldown { get; set; } = new RangeNode<int>(5000, 10, 60000);
        [Menu("Use When Available 5", 155, 150)] public ToggleNode UtilityFlask5UseWhenAvailable { get; set; } = new ToggleNode(false);
        
        [Menu("Life Flask Settings", 200)]
        public EmptyNode LifeHeader { get; set; }
        [Menu("Enable Life Flasks", 201, 200)] public ToggleNode IsLifeFlaskEnabled { get; set; } = new ToggleNode(true);
        
        [Menu("Life Flask 1 Settings", 210, 200)] public EmptyNode LifeFlask1Header { get; set; }
        [Menu("Enable Life Flask 1", 211, 210)] public ToggleNode IsLifeFlask1Enabled { get; set; } = new ToggleNode(true);
        [Menu("HP % to use Life Flask 1", 212, 210)] public RangeNode<int> LifeFlask1Percentage { get; set; } = new RangeNode<int>(40, 10, 100);
        [Menu("Key 1", 213, 210)] public HotkeyNode LifeFlask1Key { get; set; } = Keys.D1;
        [Menu("Enable Cooldown 1", 214, 210)] public ToggleNode LifeFlask1CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 1 (ms)", 215, 210)] public RangeNode<int> LifeFlask1Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Life Flask 2 Settings", 220, 200)] public EmptyNode LifeFlask2Header { get; set; }
        [Menu("Enable Life Flask 2", 221, 220)] public ToggleNode IsLifeFlask2Enabled { get; set; } = new ToggleNode(false);
        [Menu("HP % to use Life Flask 2", 222, 220)] public RangeNode<int> LifeFlask2Percentage { get; set; } = new RangeNode<int>(30, 10, 100);
        [Menu("Key 2", 223, 220)] public HotkeyNode LifeFlask2Key { get; set; } = Keys.D2;
        [Menu("Enable Cooldown 2", 224, 220)] public ToggleNode LifeFlask2CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 2 (ms)", 225, 220)] public RangeNode<int> LifeFlask2Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Life Flask 3 Settings", 230, 200)] public EmptyNode LifeFlask3Header { get; set; }
        [Menu("Enable Life Flask 3", 231, 230)] public ToggleNode IsLifeFlask3Enabled { get; set; } = new ToggleNode(false);
        [Menu("HP % to use Life Flask 3", 232, 230)] public RangeNode<int> LifeFlask3Percentage { get; set; } = new RangeNode<int>(20, 10, 100);
        [Menu("Key 3", 233, 230)] public HotkeyNode LifeFlask3Key { get; set; } = Keys.D3;
        [Menu("Enable Cooldown 3", 234, 230)] public ToggleNode LifeFlask3CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 3 (ms)", 235, 230)] public RangeNode<int> LifeFlask3Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Life Flask 4 Settings", 240, 200)] public EmptyNode LifeFlask4Header { get; set; }
        [Menu("Enable Life Flask 4", 241, 240)] public ToggleNode IsLifeFlask4Enabled { get; set; } = new ToggleNode(false);
        [Menu("HP % to use Life Flask 4", 242, 240)] public RangeNode<int> LifeFlask4Percentage { get; set; } = new RangeNode<int>(15, 10, 100);
        [Menu("Key 4", 243, 240)] public HotkeyNode LifeFlask4Key { get; set; } = Keys.D4;
        [Menu("Enable Cooldown 4", 244, 240)] public ToggleNode LifeFlask4CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 4 (ms)", 245, 240)] public RangeNode<int> LifeFlask4Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Life Flask 5 Settings", 250, 200)] public EmptyNode LifeFlask5Header { get; set; }
        [Menu("Enable Life Flask 5", 251, 250)] public ToggleNode IsLifeFlask5Enabled { get; set; } = new ToggleNode(false);
        [Menu("HP % to use Life Flask 5", 252, 250)] public RangeNode<int> LifeFlask5Percentage { get; set; } = new RangeNode<int>(10, 10, 100);
        [Menu("Key 5", 253, 250)] public HotkeyNode LifeFlask5Key { get; set; } = Keys.D5;
        [Menu("Enable Cooldown 5", 254, 250)] public ToggleNode LifeFlask5CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 5 (ms)", 255, 250)] public RangeNode<int> LifeFlask5Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Mana Flask Settings", 400)]
        public EmptyNode ManaHeader { get; set; }
        [Menu("Enable Mana Flasks", 401, 400)] public ToggleNode IsManaFlaskEnabled { get; set; } = new ToggleNode(true);
        
        [Menu("Mana Flask 1 Settings", 410, 400)] public EmptyNode ManaFlask1Header { get; set; }
        [Menu("Enable Mana Flask 1", 411, 410)] public ToggleNode IsManaFlask1Enabled { get; set; } = new ToggleNode(true);
        [Menu("Mana % to use Mana Flask 1", 412, 410)] public RangeNode<int> ManaFlask1Percentage { get; set; } = new RangeNode<int>(40, 10, 100);
        [Menu("Key 1", 413, 410)] public HotkeyNode ManaFlask1Key { get; set; } = Keys.D1;
        [Menu("Enable Cooldown 1", 414, 410)] public ToggleNode ManaFlask1CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 1 (ms)", 415, 410)] public RangeNode<int> ManaFlask1Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Mana Flask 2 Settings", 420, 400)] public EmptyNode ManaFlask2Header { get; set; }
        [Menu("Enable Mana Flask 2", 421, 420)] public ToggleNode IsManaFlask2Enabled { get; set; } = new ToggleNode(false);
        [Menu("Mana % to use Mana Flask 2", 422, 420)] public RangeNode<int> ManaFlask2Percentage { get; set; } = new RangeNode<int>(30, 10, 100);
        [Menu("Key 2", 423, 420)] public HotkeyNode ManaFlask2Key { get; set; } = Keys.D2;
        [Menu("Enable Cooldown 2", 424, 420)] public ToggleNode ManaFlask2CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 2 (ms)", 425, 420)] public RangeNode<int> ManaFlask2Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Mana Flask 3 Settings", 430, 400)] public EmptyNode ManaFlask3Header { get; set; }
        [Menu("Enable Mana Flask 3", 431, 430)] public ToggleNode IsManaFlask3Enabled { get; set; } = new ToggleNode(false);
        [Menu("Mana % to use Mana Flask 3", 432, 430)] public RangeNode<int> ManaFlask3Percentage { get; set; } = new RangeNode<int>(20, 10, 100);
        [Menu("Key 3", 433, 430)] public HotkeyNode ManaFlask3Key { get; set; } = Keys.D3;
        [Menu("Enable Cooldown 3", 434, 430)] public ToggleNode ManaFlask3CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 3 (ms)", 435, 430)] public RangeNode<int> ManaFlask3Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Mana Flask 4 Settings", 440, 400)] public EmptyNode ManaFlask4Header { get; set; }
        [Menu("Enable Mana Flask 4", 441, 440)] public ToggleNode IsManaFlask4Enabled { get; set; } = new ToggleNode(false);
        [Menu("Mana % to use Mana Flask 4", 442, 440)] public RangeNode<int> ManaFlask4Percentage { get; set; } = new RangeNode<int>(15, 10, 100);
        [Menu("Key 4", 443, 440)] public HotkeyNode ManaFlask4Key { get; set; } = Keys.D4;
        [Menu("Enable Cooldown 4", 444, 440)] public ToggleNode ManaFlask4CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 4 (ms)", 445, 440)] public RangeNode<int> ManaFlask4Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);

        [Menu("Mana Flask 5 Settings", 450, 400)] public EmptyNode ManaFlask5Header { get; set; }
        [Menu("Enable Mana Flask 5", 451, 450)] public ToggleNode IsManaFlask5Enabled { get; set; } = new ToggleNode(false);
        [Menu("Mana % to use Mana Flask 5", 452, 450)] public RangeNode<int> ManaFlask5Percentage { get; set; } = new RangeNode<int>(10, 10, 100);
        [Menu("Key 5", 453, 450)] public HotkeyNode ManaFlask5Key { get; set; } = Keys.D5;
        [Menu("Enable Cooldown 5", 454, 450)] public ToggleNode ManaFlask5CooldownEnable { get; set; } = new ToggleNode(true);
        [Menu("Cooldown 5 (ms)", 455, 450)] public RangeNode<int> ManaFlask5Cooldown { get; set; } = new RangeNode<int>(600, 0, 10000);
        
        [Menu("Skill & Targeting Settings", 300)]
        public EmptyNode SkillHeader { get; set; }
        [Menu("Enable Skills", 301, 300)] public ToggleNode IsSkillEnabled { get; set; } = new ToggleNode(false);
        [Menu("Enable Monster Tracing", 302, 300)] public ToggleNode IsTracingMonstersEnabled { get; set; } = new ToggleNode(false);

        [Menu("Skill 1 Key", 303, 300)] public HotkeyNode Skill1Key { get; set; } = Keys.W;
        [Menu("Skill 1 Cooldown", 304, 300)] public RangeNode<int> Skill1Cooldown { get; set; } = new RangeNode<int>(5000, 100, 30000);
        
        [Menu("Skill 2 Key", 305, 300)] public HotkeyNode Skill2Key { get; set; } = Keys.E;
        [Menu("Skill 2 Cooldown", 306, 300)] public RangeNode<int> Skill2Cooldown { get; set; } = new RangeNode<int>(10000, 100, 30000);

        [Menu("Skill 3 Key", 307, 300)] public HotkeyNode Skill3Key { get; set; } = Keys.R;
        [Menu("Skill 3 Cooldown", 308, 300)] public RangeNode<int> Skill3Cooldown { get; set; } = new RangeNode<int>(4000, 100, 30000);
    }
}