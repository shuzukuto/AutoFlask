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
        [Menu("Time Between Actions", 102, 100)] public RangeNode<int> TimeBetweenActions { get; set; } = new RangeNode<int>(1000, 10, 20000);
        
        [Menu("Life Flask Settings", 200)]
        public EmptyNode LifeHeader { get; set; }
        [Menu("Enable Life Flask", 201, 200)] public ToggleNode IsLifeFlaskEnabled { get; set; } = new ToggleNode(true);
        [Menu("HP % to use Life Flask", 202, 200)] public RangeNode<int> LifeFlaskPercentage { get; set; } = new RangeNode<int>(10, 10, 100);
        
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