using CommunityToolkit.Mvvm.Messaging.Messages;

namespace jxlgui.wpf.Messenger;

public class FileDroppedMessage : ValueChangedMessage<string>
{
    public FileDroppedMessage(string value) : base(value)
    {
    }
}