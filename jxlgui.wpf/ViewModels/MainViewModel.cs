using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using jxlgui.converter;
using jxlgui.wpf.Messenger;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace jxlgui.wpf.ViewModels;

internal class MainViewModel : ObservableRecipient
{
    private string avifDecVersion = "UNDEF";
    private string avifEncVersion = "UNDEF";

    private bool canEncode;

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

    public string AvifEncVersion
    {
        get => avifEncVersion;
        set => SetProperty(ref avifEncVersion, value);
    }

    public string AvifDecVersion
    {
        get => avifDecVersion;
        set => SetProperty(ref avifDecVersion, value);
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
        void SetVersion(Action<string> Set, ExternalJxlRessourceHandler.JxlFileResult avifFileResult)
        {
            if (avifFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.FileNotFound)
            {
                Set("FILE NOT FOUND");
                CanEncode = false;
            }
            else if (avifFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.VersionNotReadable)
            {
                Set("ERROR");
                CanEncode = false;
            }
            else if (avifFileResult.Result == ExternalJxlRessourceHandler.JxlFileResultEnum.OK)
            {
                Set(avifFileResult.Version);
                CanEncode = true;
            }
        }

        await Task.Factory.StartNew(() =>
        {
            SetVersion(s => AvifEncVersion = s, ExternalJxlRessourceHandler.GetEncoderInformation());
            SetVersion(s => AvifDecVersion = s, ExternalJxlRessourceHandler.GetDecoderInformation());
        });
    }
}