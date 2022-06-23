
using System.Text.Json;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace jxlgui.wpf.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    public SettingsViewModel()
    {
        this.SaveCommand = new RelayCommand(() => { }, () => false);
        this.CancelCommand = new RelayCommand(() => { }, () => false);

        var config = jxlgui.converter.Config.CreateEmpty();
        var jsonConfig = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var jsonString = JsonSerializer.Serialize(config, jsonConfig);

        this.Config = jsonString;
    }

    public RelayCommand OnLoadCommand { get; set; }
    public RelayCommand SaveCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }
    public string Config { get; set; }
}