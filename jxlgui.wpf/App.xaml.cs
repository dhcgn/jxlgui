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

        SettingsWindow? w = null;

        WeakReferenceMessenger.Default.Register<WindowMessage>(this, (r, m) =>
        {
            if (m.Value == WindowEnum.SettingsWindows)
            {
                w = new SettingsWindow();
                w.ShowDialog();
            }

            if (m.Value == WindowEnum.SettingsWindowsClose)
            {
                w?.Close();
            }
        });
    }
}