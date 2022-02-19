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
    private bool canEncode;
    private string jxlDecVersion = "UNDEF";
    private string jxlEncVersion = "UNDEF";

    public MainViewModel()
    {
        this.OnLoadCommand = new AsyncRelayCommand(this.OnLoadCommandHandlingAsync);
        JobManager jm = new();

        WeakReferenceMessenger.Default.Register<FileDroppedMessage>(this, (r, m) =>
        {
            if (!this.CanEncode)
                return;

            var job = Job.Create(m.Value);
            jm.Add(job);
            this.Jobs.Add(job);
        });

        this.ShowSettingsCommand =
            new RelayCommand(() => this.Messenger.Send(new WindowMessage(WindowEnum.SettingsWindows)));

        this.Configs = new List<string> { "built in" };
        this.SelectedConfig = this.Configs.First();

        if (InDesignMode())
        {
            this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Pending));
            this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Working));
            this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Done));
            this.Jobs.Add(Job.GetDesignDate(Job.JobStateEnum.Error));

            this.CanEncode = false;
        }
    }

    public BuildInfos BuildInfos { get; } = BuildInfos.Get();

    public ObservableCollection<Job> Jobs { get; } = new();

    public string JxlEncVersion
    {
        get => this.jxlEncVersion;
        set => this.SetProperty(ref this.jxlEncVersion, value);
    }

    public string JxlDecVersion
    {
        get => this.jxlDecVersion;
        set => this.SetProperty(ref this.jxlDecVersion, value);
    }

    public RelayCommand ShowSettingsCommand { get; set; }
    public List<string> Configs { get; }
    public string SelectedConfig { get; set; }
    public IAsyncRelayCommand OnLoadCommand { get; }

    public bool CanEncode
    {
        get => this.canEncode;
        set => this.SetProperty(ref this.canEncode, value);
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
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }

    public static bool InDesignMode()
    {
        return !(Application.Current is App);
    }

    private async Task OnLoadCommandHandlingAsync()
    {
        void SetVersion(Action<string> Set, ExternalJxlResourceHandler.JxlFileResult jxlFileResult)
        {
            if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.FileNotFound)
            {
                Set("FILE NOT FOUND");
                this.CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.VersionNotReadable)
            {
                Set("ERROR");
                this.CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.OK)
            {
                Set(jxlFileResult.Version);
                this.CanEncode = true;
            }
        }

        ExternalJxlResourceHandler.SaveFiles();

        await Task.Factory.StartNew(() =>
        {
            SetVersion(s => this.JxlEncVersion = s, ExternalJxlResourceHandler.GetEncoderInformation());
            SetVersion(s => this.JxlDecVersion = s, ExternalJxlResourceHandler.GetDecoderInformation());
        });
    }
}