using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace AutoFlask;

public class AutoFlaskSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public ToggleNode IsAutoFlaskEnabled { get; set; } = new ToggleNode(false);
    
    [Menu("Toggle Auto Flask")] public HotkeyNode ToggleAutoFlask { get; set; } = Keys.PageDown;
    [Menu("Time Between Actions")] public RangeNode<int> TimeBetweenActions { get; set; } = new RangeNode<int>(1000, 10, 20000);
    public ToggleNode IsLifeFlaskEnabled { get; set; } = new ToggleNode(true);
    [Menu("HP Percentage to use Life Flask")] public RangeNode<int> LifeFlaskPercentage { get; set; } = new RangeNode<int>(10, 10, 100);
}

