using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using jxlgui.buildinfo;
using jxlgui.converter;
using jxlgui.wpf.Messenger;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace jxlgui.wpf.ViewModels;

internal class MainViewModel : ObservableRecipient
{
    private string _jxlDecVersion = "UNDEF";
    private string _jxlEncVersion = "UNDEF";

    private bool canEncode;

    public BuildInfos BuildInfos { get; } = BuildInfos.Get();

    public MainViewModel()
    {
        OnLoadCommand = new AsyncRelayCommand(OnLoadCommandHandlingAsync);
        JobManager jm = new();

        WeakReferenceMessenger.Default.Register<FileDroppedMessage>(this, (r, m) =>
        {
            if (!CanEncode)
                return;

            var job = Job.Create(m.Value);
            jm.Add(job);
            Jobs.Add(job);
        });

        ShowSettingsCommand =
            new RelayCommand(() => Messenger.Send(new WindowMessage(WindowEnum.SettingsWindows)));
        OpenEncoderInstallWikiCommand = new RelayCommand(() =>
            OpenUrl("https://github.com/dhcgn/avif_encoder_gui/wiki/Install-AVIF-Encoder-and-AVIF-Decoder"));

        Configs = new List<string> {"built in"};
        SelectedConfig = Configs.First();

        if (InDesignMode())
        {
            Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Pending));
            Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Working));
            Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Done));
            Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Error));

            CanEncode = false;
        }
    }

    public ObservableCollection<Job> Jobs { get; } = new();

    public string JxlEncVersion
    {
        get => _jxlEncVersion;
        set => SetProperty(ref _jxlEncVersion, value);
    }

    public string JxlDecVersion
    {
        get => _jxlDecVersion;
        set => SetProperty(ref _jxlDecVersion, value);
    }

    public RelayCommand ShowSettingsCommand { get; set; }
    public RelayCommand OpenEncoderInstallWikiCommand { get; set; }
    public List<string> Configs { get; }
    public string SelectedConfig { get; set; }
    public IAsyncRelayCommand OnLoadCommand { get; }

    public bool CanEncode
    {
        get => canEncode;
        set => SetProperty(ref canEncode, value);
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
        }
    }

    public static bool InDesignMode()
    {
        return !(Application.Current is App);
    }

    private async Task OnLoadCommandHandlingAsync()
    {
        void SetVersion(Action<string> Set, ExternalJxlRessourceHandler.JxlFileResult jxlFileResult)
        {
            if (jxlFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.FileNotFound)
            {
                Set("FILE NOT FOUND");
                CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.VersionNotReadable)
            {
                Set("ERROR");
                CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.OK)
            {
                Set(jxlFileResult.Version);
                CanEncode = true;
            }
        }

        ExternalJxlRessourceHandler.SaveFiles();

        await Task.Factory.StartNew(() =>
        {
            SetVersion(s => JxlEncVersion = s, ExternalJxlRessourceHandler.GetEncoderInformation());
            SetVersion(s => JxlDecVersion = s, ExternalJxlRessourceHandler.GetDecoderInformation());
        });
    }
}