using System.Windows;
using jxlgui.wpf.Messenger;
using jxlgui.wpf.Windows;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace jxlgui.wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        WeakReferenceMessenger.Default.Register<WindowMessage>(this, (r, m) =>
        {
            if (m.Value == WindowEnum.SettingsWindows) _ = new SettingsWindow().ShowDialog();
        });
    }
}