using System.IO;
using System.Linq;
using System.Windows;
using jxlgui.converter;
using jxlgui.wpf.Messenger;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace jxlgui.wpf.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        base.OnDrop(e);
        var droppedFileName = e.Data.GetData(DataFormats.FileDrop) as string[];

        if (droppedFileName != null && droppedFileName.Any())
            droppedFileName.ToList()
                .ForEach(path => WeakReferenceMessenger.Default.Send(new FileDroppedMessage(path)));

        e.Handled = true;
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        var droppedFileName = e.Data.GetData(DataFormats.FileDrop) as string[];

        if (droppedFileName != null
            && droppedFileName.Any()
            && droppedFileName.Select(f => Path.GetExtension(f))
                .All(e => Constants.Extensions.Any(ee => ee.ToLower() == e.ToLower())))
        {
            e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
        }

        e.Handled = true;
    }
}