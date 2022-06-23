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



    public MainViewModel()
    {
        this.OnLoadCommand = new AsyncRelayCommand(this.OnLoadCommandHandlingAsync);
        JobManager jm = new();

        WeakReferenceMessenger.Default.Register<FileDroppedMessage>(this, (r, m) =>
        {
            if (!this.CanEncode)
                return;

            var job = Job.Create(m.Value, Config.Load());
            jm.Add(job);
            this.Jobs.Add(job);
        });

        this.ShowSettingsCommand =
            new RelayCommand(() => this.Messenger.Send(new WindowMessage(WindowEnum.SettingsWindows)));
        this.OpenHelpCommand = new RelayCommand(() => OpenUrl(@"https://github.com/dhcgn/jxlgui/wiki"));

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

    private string jxlEncVersion = "UNDEF";
    public string JxlEncVersion
    {
        get => this.jxlEncVersion;
        set => this.SetProperty(ref this.jxlEncVersion, value);
    }

    private string jxlEncCommit = "0000000";
    public string JxlEncCommit
    {
        get => this.jxlEncCommit;
        set => this.SetProperty(ref this.jxlEncCommit, value);
    }

    private string jxlDecVersion = "UNDEF";
    public string JxlDecVersion
    {
        get => this.jxlDecVersion;
        set => this.SetProperty(ref this.jxlDecVersion, value);
    }

    private string jxlDecCommit = "0000000";
    public string JxlDecCommit
    {
        get => this.jxlDecCommit;
        set => this.SetProperty(ref this.jxlDecCommit, value);
    }

    public RelayCommand ShowSettingsCommand { get; set; }
    public RelayCommand OpenHelpCommand { get; set; }
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
        void SetVersionCommit(Action<string> SetVersion, Action<string> SetCommit, ExternalJxlResourceHandler.JxlFileResult jxlFileResult)
        {
            if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.FileNotFound)
            {
                SetVersion("FILE NOT FOUND");
                SetCommit("");
                this.CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.VersionNotReadable)
            {
                SetVersion("ERROR");
                SetCommit("");
                this.CanEncode = false;
            }
            else if (jxlFileResult.Result == ExternalJxlResourceHandler.JxlFileResultEnum.OK)
            {
                SetVersion(jxlFileResult.Version);
                SetCommit(jxlFileResult.Commit);
                this.CanEncode = true;
            }
        }

        ExternalJxlResourceHandler.SaveFiles();

        await Task.Factory.StartNew(() =>
        {
            SetVersionCommit(v => this.JxlEncVersion = v,c => this.JxlEncCommit = c,  ExternalJxlResourceHandler.GetEncoderInformation());
            SetVersionCommit(v => this.JxlDecVersion = v,c => this.JxlDecCommit = c, ExternalJxlResourceHandler.GetDecoderInformation());
        });
    }
}