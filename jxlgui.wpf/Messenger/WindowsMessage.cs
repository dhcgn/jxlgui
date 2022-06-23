using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace jxlgui.wpf.Messenger;

internal class WindowMessage : ValueChangedMessage<WindowEnum>
{
    public WindowMessage(WindowEnum value) : base(value)
    {
    }
}

public enum WindowEnum
{
    SettingsWindows,
    SettingsWindowsClose
}