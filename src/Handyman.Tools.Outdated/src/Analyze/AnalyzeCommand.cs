﻿using Handyman.Tools.Outdated.Model;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Handyman.Tools.Outdated.Analyze
{
    [Command("analyze")]
    public class AnalyzeCommand
    {
        private readonly IConsole _console;
        private readonly ProjectLocator _projectLocator;
        private readonly ProjectUtil _projectUtil;
        private readonly ProjectAnalyzer _projectAnalyzer;
        private readonly IEnumerable<IFileWriter> _fileWriters;

        public AnalyzeCommand(IConsole console, ProjectLocator projectLocator, ProjectUtil projectUtil, ProjectAnalyzer projectAnalyzer, IEnumerable<IFileWriter> fileWriters)
        {
            _console = console;
            _projectLocator = projectLocator;
            _projectUtil = projectUtil;
            _projectAnalyzer = projectAnalyzer;
            _fileWriters = fileWriters;
        }

        [Argument(0, "path", Description = "Path to folder or project")]
        public string Path { get; set; }

        [Option(ShortName = "", Description = "Output file(s), supported format is .md")]
        public string[] OutputFile { get; set; } = { };

        [Option(ShortName = "", Description = "Tags filter, start with ! to exclude")]
        public string[] Tags { get; set; } = { };

        [Option(CommandOptionType.NoValue, ShortName = "", Description = "Skip dotnet restore")]
        public bool NoRestore { get; set; }

        [Option]
        public Verbosity Verbosity { get; set; }

        public int OnExecute()
        {
            var projects = _projectLocator.GetProjects(Path, Tags.ToList());

            if (ShouldWriteToConsole(Verbosity.Minimal))
            {
                _console.WriteLine();
                _console.WriteLine($"Discovered {projects.Count} projects.");
                _console.WriteLine();
            }

            var counter = 1;
            foreach (var project in projects)
            {
                if (ShouldWriteToConsole(Analyze.Verbosity.Minimal))
                    _console.WriteLine($"Analyzing {project.RelativePath} ({counter++} of {projects.Count})");

                if (NoRestore == false)
                    _projectUtil.Restore(project);

                _projectAnalyzer.Analyze(project);
            }

            new ResultConsoleWriter(_console, Verbosity).WriteResult(projects);

            _console.WriteLine("pre write to disk");
            WriteResultToFile(projects);
            WriteDriveInfo();
            _console.WriteLine("post write to disk");

            return 0;
        }

        private bool ShouldWriteToConsole(Verbosity required)
        {
            var current = Verbosity;
            return current != Verbosity.Quiet && (int)required <= (int)current;
        }

        private void WriteResultToFile(IReadOnlyCollection<Project> projects)
        {
            foreach (var outputFile in OutputFile)
            {
                var extension = System.IO.Path.GetExtension(outputFile).ToLowerInvariant();
                var fileWriters = _fileWriters.Where(x => x.CanHandle(extension)).ToList();

                if (!fileWriters.Any())
                {
                    _console.WriteLine($"Unsupported output file format '{extension}'.");
                    continue;
                }

                fileWriters.ForEach(x => x.Write(outputFile, projects));
            }
        }

        private void WriteDriveInfo()
        {
            foreach (var driveName in new[] { "C", "D", "E" })
            {
                var driveInfo = new DriveInfo(driveName);

                if (!driveInfo.IsReady)
                    continue;

                var freeSpacePercent = (driveInfo.AvailableFreeSpace / (float)driveInfo.TotalSize) * 100;

                _console.WriteLine("Drive: {0} ({1}, {2})", driveInfo.Name, driveInfo.DriveFormat, driveInfo.DriveType);
                _console.WriteLine("\tFree space:\t{0}", driveInfo.AvailableFreeSpace);
                _console.WriteLine("\tTotal space:\t{0}", driveInfo.TotalSize);
                _console.WriteLine("\tPercentage free space: {0:0.00}%.", freeSpacePercent);
            }
        }
    }
}