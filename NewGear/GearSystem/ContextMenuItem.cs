using NewGear.GearSystem.Interfaces;

namespace NewGear.GearSystem;

public struct ContextMenuItem {
    public string Text;
    public ContextItemAction Action;
    public ContextItemEnabledCheck IsEnabled;
    
    public ContextMenuItem(string text, ContextItemAction action, ContextItemEnabledCheck? isEnable = null) {
        Text = text;
        Action = action;
        IsEnabled = isEnable ?? new((gear, arguments) => { return true; });
    }
}

public delegate void ContextItemAction(IGear gear, Dictionary<string, dynamic> arguments);
public delegate bool ContextItemEnabledCheck(IGear gear, Dictionary<string, dynamic> arguments);