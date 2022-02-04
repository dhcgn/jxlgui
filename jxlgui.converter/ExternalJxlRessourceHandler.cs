﻿using System.Diagnostics;
using System.Text.RegularExpressions;

namespace jxlgui.converter
{
    public class ExternalJxlRessourceHandler
    {
        public enum JxlFileResultEnum
        {
            OK,
            FileNotFound,
            VersionNotReadable
        }


        public static JxlFileResult GetDecoderInformation()
        {
            return GetFileInformation(Constants.DecoderFilePath);
        }

        public static JxlFileResult GetEncoderInformation()
        {
            return GetFileInformation(Constants.EncoderFilePath);
        }

        private static JxlFileResult GetFileInformation(string path)
        {
            if (!File.Exists(path))
                return new JxlFileResult
                {
                    Result = JxlFileResultEnum.FileNotFound
                };

            var version = GetExecutableVersion(path);

            if (version == null)
                return new JxlFileResult
                {
                    Result = JxlFileResultEnum.VersionNotReadable
                };

            return new JxlFileResult
            {
                Result = JxlFileResultEnum.OK,
                Version = version
            };
        }


        private static string GetExecutableVersion(string path)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var line = "";
            while (!proc.StandardOutput.EndOfStream) line += proc.StandardOutput.ReadLine();

            return ParseVersion(line);
        }

        private static string ParseVersion(string input)
        {
            var r = Regex.Match(input, @"Version: (\d{1,}.\d{1,}.\d{1,})");
            if (r.Groups.Count != 2) return null;

            return r.Groups[1].Value;
        }

        public class JxlFileResult
        {
            public JxlFileResultEnum Result { get; init; }
            public string Version { get; init; }
        }
    }
}