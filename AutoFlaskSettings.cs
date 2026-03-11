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
        
        public ToggleNode IsUltilityFlaskEnabled { get; set; } = new ToggleNode(true);
        [Menu("Time Between Actions")] public RangeNode<int> TimeBetweenActions { get; set; } = new RangeNode<int>(1000, 10, 20000);
        
        public ToggleNode IsLifeFlaskEnabled { get; set; } = new ToggleNode(true);
        [Menu("HP Percentage to use Life Flask")] public RangeNode<int> LifeFlaskPercentage { get; set; } = new RangeNode<int>(10, 10, 100);
        
        public ToggleNode IsSkillEnabled { get; set; } = new ToggleNode(false);
        public ToggleNode IsTracingMonstersEnabled { get; set; } = new ToggleNode(false);
        public Skill Skill1 { get; set; } = new Skill { Hotkey = (HotkeyNode)Keys.W };
        [Menu("Skill 1 Cooldown")] public RangeNode<int> Skill1Cooldown { get; set; } = new RangeNode<int>(100, 10, 3000);
        public Skill Skill2 { get; set; } = new Skill { Hotkey = (HotkeyNode)Keys.E };
        [Menu("Skill 2 Cooldown")] public RangeNode<int> Skill2Cooldown { get; set; } = new RangeNode<int>(100, 10, 3000);

        public class Skill
        {
            public HotkeyNode Hotkey { get; set; }
        }
    }
}