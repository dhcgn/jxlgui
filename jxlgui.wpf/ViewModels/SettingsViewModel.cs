
using System.IO;
using System.Text.Json;
using jxlgui.converter;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace jxlgui.wpf.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    public SettingsViewModel()
    {
        this.SaveCommand = new RelayCommand(SaveCommandHandling, () => jxlgui.converter.Config.IsJsonValid(this.Config));
        this.CancelCommand = new RelayCommand(() => { }, () => false);

        var c = jxlgui.converter.Config.LoadOrCreateNew();
        this.Config = c.ToJson();
    }

    private void SaveCommandHandling()
    {
        if (!jxlgui.converter.Config.IsJsonValid(this.Config))
        {
            System.Windows.MessageBox.Show("Config is not valid");
            return;
        }

        File.WriteAllText(Constants.ConfigPath, this.Config);

        this.Config = jxlgui.converter.Config.Load()?.ToJson();
    }

    public RelayCommand OnLoadCommand { get; set; }
    public RelayCommand SaveCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }

    private string config;
    public string Config
    {
        get => config;
        set => this.SetProperty(ref this.config, value);
    }
}