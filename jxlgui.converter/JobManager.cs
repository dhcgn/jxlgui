using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace jxlgui.converter;

public class JobManager
{
    private readonly BufferBlock<Job> jobs = new();

    public JobManager()
    {
        var consumerTask = ConsumeAsync(this.jobs);
    }

    public void Add(Job job)
    {
        this.jobs.Post(job);
    }

    private static async Task<int> ConsumeAsync(ISourceBlock<Job> source)
    {
        while (await source.OutputAvailableAsync())
        {
            var job = await source.ReceiveAsync();
            job.State = Job.JobStateEnum.Working;
            var result = await ExecuteImageOperationAsync(job);

            job.State = result.State;
            job.ProcessOutput = result.Output;
        }

        return 0;
    }

    private static async Task<ExecuteImageOperationResult> ExecuteImageOperationAsync(Job job)
    {
        var filename = GetFileName(job);
        var arguments = GetArguments(job);

        var r = await RunProcessAsync(filename, arguments);


        var state = r.returnCode == 0 ? Job.JobStateEnum.Done : Job.JobStateEnum.Error;

        return new ExecuteImageOperationResult
        {
            State = state,
            Output = r.output
        };
    }

    private static string GetArguments(Job job)
    {
        var targetFilePath = "";
        switch (job.Operation)
        {
            case Job.OperationEnum.Encode:
                targetFilePath = $"{Path.Combine(new FileInfo(job.FilePath).DirectoryName, job.FileName)}.jxl";
                break;
            case Job.OperationEnum.Decode:
                targetFilePath = $"{Path.Combine(new FileInfo(job.FilePath).DirectoryName, job.FileName)}.png";
                break;
            default:
                throw new Exception($"{job.Operation} should be Encode or Decode");
        }

        job.TargetFilePath = targetFilePath;

        switch (job.Operation)
        {
            case Job.OperationEnum.Encode:
                return $"-q {job.Config.Quality} -e {job.Config.Effort} \"{job.FilePath}\" \"{job.TargetFilePath}\"";
            case Job.OperationEnum.Decode:
                return $" \"{job.FilePath}\" \"{job.TargetFilePath}\"";
            default:
                throw new Exception($"{job.Operation} should be Encode or Decode");
        }
    }

    private static string GetFileName(Job job)
    {
        switch (job.Operation)
        {
            case Job.OperationEnum.Encode:
                return Constants.EncoderFilePath;
            case Job.OperationEnum.Decode:
                return Constants.DecoderFilePath;
            default:
                throw new Exception($"{job.Operation} should be Encode or Decode");
        }
    }

    private static Task<(int returnCode, string output)> RunProcessAsync(string fileName, string arguments)
    {
        var tcs = new TaskCompletionSource<(int returnCode, string output)>();

        var process = new Process
        {
            StartInfo =
            {
                FileName = fileName,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };


        process.Exited += (sender, args) =>
        {
            var line = "";
            while (!process.StandardOutput.EndOfStream)
                line += process.StandardOutput.ReadLine() + Environment.NewLine;
            while (!process.StandardError.EndOfStream)
                line += process.StandardError.ReadLine() + Environment.NewLine;

            tcs.SetResult((process.ExitCode, line));
            process.Dispose();
        };

        process.Start();

        return tcs.Task;
    }

    private class ExecuteImageOperationResult
    {
        public Job.JobStateEnum State { get; internal set; }
        public string Output { get; set; }
    }
}

/// <summary>
///     TODO ObservableObject should not be here
/// </summary>
public class Job : ObservableObject
{
    public enum JobStateEnum
    {
        Pending,
        Done,
        Error,
        Working
    }

    public enum OperationEnum
    {
        Undef,
        Encode,
        Decode
    }

    private JobStateEnum state;
    private string targetFileFormattedLength;


    public string FilePath { get; init; }
    public string FileName { get; init; }
    public long Length { get; init; }

    public JobStateEnum State
    {
        get => this.state;
        internal set
        {
            this.SetProperty(ref this.state, value);
            if (value == JobStateEnum.Done && this.TargetFilePath != null)
            {
                var fi = new FileInfo(this.TargetFilePath);
                if (fi.Exists)
                {
                    this.TargetFileLength = fi.Length;
                    this.TargetFileFormattedLength = GetFormattedLength(fi.Length);
                }
            }
        }
    }

    public FileInfo FileInfo { get; init; }

    public OperationEnum Operation => GetOperation(this.FileInfo);

    public string FormattedLength { get; init; }

    private Config? config;
    public Config? Config
    {
        get => config;
        set => base.SetProperty(ref config, value);
    }

    public string TargetFilePath { get; internal set; }
    public long TargetFileLength { get; internal set; }

    public string TargetFileFormattedLength
    {
        get => this.targetFileFormattedLength;
        internal set => this.SetProperty(ref this.targetFileFormattedLength, value);
    }

    public string ProcessOutput { get; set; }

    public static Job Create(string filepath, Config config)
    {
        var fi = new FileInfo(filepath);
        var job = new Job
        {
            FilePath = fi.FullName,
            FileName = fi.Name,
            Length = fi.Length,
            FileInfo = fi,
            FormattedLength = GetFormattedLength(fi.Length),
            Config = GetOperation(fi) == OperationEnum.Encode ? config : null,
        };

        return job;
    }

    private static string GetFormattedLength(double len)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
        // show a single decimal place, and no space.
        var result = $"{len:0.##} {sizes[order]}";
        return result;
    }

    private static OperationEnum GetOperation(FileInfo fileInfo)
    {
        if (fileInfo == null)
            return OperationEnum.Undef;

        var ext = fileInfo.Extension.ToLowerInvariant();

        if (Constants.ExtensionsDecode.Any(e => e == ext)) return OperationEnum.Decode;
        if (Constants.ExtensionsEncode.Any(e => e == ext)) return OperationEnum.Encode;
        return OperationEnum.Undef;
    }

    public static Job GetDesignDate(JobStateEnum state)
    {
        if (state is JobStateEnum.Pending)
        {
            return new Job
            {
                FileName = "pic1.png",
                FilePath = "C:\\Users\\User\\Pictures\\pic1.png",
                TargetFilePath = "C:\\Users\\User\\Pictures\\pic1.png.avif",
                State = state,
                FormattedLength = "131 KB",
                Config = null
            };
        }

        if (state is JobStateEnum.Working)
        {
            return new Job
            {
                FileName = "pic1.png",
                FilePath = "C:\\Users\\User\\Pictures\\pic1.png",
                TargetFilePath = "C:\\Users\\User\\Pictures\\pic1.png.avif",
                State = state,
                FormattedLength = "131 KB",
                Config = new Config
                {
                    Quality = 90,
                    Effort = 6,
                }
            };
        }

        return new Job
        {
            FileName = "pic1.png",
            FilePath = "C:\\Users\\User\\Pictures\\pic1.png",
            TargetFilePath = "C:\\Users\\User\\Pictures\\pic1.png.avif",
            State = state,
            FormattedLength = "132 KB",
            TargetFileFormattedLength = "80 KB",
            Config = new Config
            {
                Quality = 90,
                Effort = 6,
            }
        };
    }
}