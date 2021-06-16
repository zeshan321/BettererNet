using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;

namespace InspectCodeSnapshot
{
    public static class Program
    {
        private static Settings _settings;

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);

            _settings = builder.Build().GetSection("Settings").Get<Settings>();

            // Check if snapshot already exists
            var snapshotPath = Path.Combine(_settings.SnapshotProcessStartInfo.WorkingDirectory, "snapshot.xml");
            if (!File.Exists(snapshotPath))
            {
                Console.WriteLine("Generating first snapshot...");
                await GenerateFile(_settings.SnapshotProcessStartInfo);
                return;
            }

            Console.WriteLine("Generating compare snapshot...");
            var snapshotResultTask = GetResult(snapshotPath);
            var generateCompareTask = GenerateFile(_settings.CompareProcessStartInfo);

            await Task.WhenAll(snapshotResultTask, generateCompareTask);

            Console.WriteLine("Loading compare snapshot...");
            var snapshotResult = snapshotResultTask.Result;
            var comparePath = Path.Combine(_settings.SnapshotProcessStartInfo.WorkingDirectory, "compare.xml");
            var compareResult = await GetResult(comparePath);

            Console.WriteLine("Comparing snapshots...");

            var issues = new List<Issue>();

            foreach (var violation in _settings.Violations)
            {
                foreach (var project in compareResult.Issues.Project)
                {
                    var projectIssues = project.Issues.Where(i => i.TypeId == violation).ToList();
                    if (!projectIssues.Any())
                    {
                        continue;
                    }

                    var snapshotProject = snapshotResult.Issues.Project.SingleOrDefault(p => p.Name == project.Name);
                    if (snapshotProject == null)
                    {
                        Console.WriteLine($"{project.Name} - New project with violations found:");
                        foreach (var projectIssue in projectIssues)
                        {
                            issues.Add(projectIssue);
                            Console.WriteLine(projectIssue);
                        }
                    }
                    else
                    {
                        var snapshotIssues = snapshotProject.Issues.Where(i => i.TypeId == violation);
                        if (projectIssues.Count <= snapshotIssues.Count())
                        {
                            continue;
                        }

                        Console.WriteLine($"{project.Name} - New violations found:");
                        foreach (var projectIssue in projectIssues)
                        {
                            issues.Add(projectIssue);
                            Console.WriteLine(projectIssue);
                        }
                    }
                }
            }

            if (issues.Any())
            {
                Environment.Exit(1);
            }
            else
            {
                File.Delete(snapshotPath);
                File.Move(comparePath, snapshotPath);
                Console.WriteLine("No new violations found");
            }
        }

        private static async Task<Report> GetResult(string path)
        {
            var serializer = new XmlSerializer(typeof(Report));
            await using var fileStream = new FileStream(path, FileMode.Open);
            var snapshot = (Report) serializer.Deserialize(fileStream);

            return snapshot;
        }

        private static Task GenerateFile(ProcessStartInfo processStartInfo)
        {
            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            if (!_settings.Debug)
            {
                return process.WaitForExitAsync();
            }

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

            return process.WaitForExitAsync();
        }
    }
}