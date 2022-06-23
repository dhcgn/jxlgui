using System.IO;
using System.Text.Json;
using System.Windows;
using jxlgui.converter;
using jxlgui.wpf.Messenger;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace jxlgui.wpf.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    public SettingsViewModel()
    {
        this.SaveCommand =
            new RelayCommand(SaveCommandHandling, () => jxlgui.converter.Config.IsJsonValid(this.Config));
        this.CancelCommand = new RelayCommand(() =>
        {
            this.Messenger.Send(new WindowMessage(WindowEnum.SettingsWindowsClose));
        });

        var c = jxlgui.converter.Config.LoadOrCreateNew();
        this.Config = c.ToJson();

        if (InDesignMode())
        {
            configError = "Error";
        }
    }

    public static bool InDesignMode()
    {
        return !(Application.Current is App);
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
        get { return config; }
        set
        {
            this.SetProperty(ref this.config, value);
            ConfigError = converter.Config.IsJsonValid(this.Config) ? null : "Json is not valid";
        }
    }

    private string? configError;

    public string? ConfigError
    {
        get { return configError; }
        set { base.SetProperty(ref this.configError, value); }
    }
}