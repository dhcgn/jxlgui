using System.IO;
using System.Text.Json;
using System.Windows;
using jxlgui.converter;
using jxlgui.wpf.Messenger;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace jxlgui.wpf.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    public SettingsViewModel()
    {
        this.SaveCommand =
            new RelayCommand(SaveCommandHandling, () =>
            {
                if (this.Config == null)
                    return false;
                return jxlgui.converter.Config.IsJsonValid(this.Config);
            });
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
        if (this.Config == null)
        {
            System.Windows.MessageBox.Show("Config is null");
            return;
        }

        if (!jxlgui.converter.Config.IsJsonValid(this.Config))
        {
            System.Windows.MessageBox.Show("Config is not valid");
            return;
        }

        File.WriteAllText(Constants.ConfigPath, this.Config);

        var c = jxlgui.converter.Config.Load();
        if (c == null)
            return;

        this.Config = c.ToJson();
    }

    public required RelayCommand OnLoadCommand { get; set; }
    public required RelayCommand SaveCommand { get; set; }
    public required RelayCommand CancelCommand { get; set; }

    private string? config;

    public string? Config
    {
        get { return config; }
        set
        {
            this.SetProperty(ref this.config, value);
            if (this.Config == null){
                ConfigError = "Config is null";
                return;
            }
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