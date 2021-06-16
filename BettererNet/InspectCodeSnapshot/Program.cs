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
                    var snapshotProject = snapshotResult.Issues.Project.SingleOrDefault(p => p.Name == project.Name);
                    if (snapshotProject == null)
                    {
                        Console.WriteLine($"{project.Name} - violations found in new project:");
                        var items = project.Issues.Where(i => i.TypeId == violation).ToList();
                        foreach (var issue in items)
                        {
                            Console.WriteLine(issue);
                        }

                        issues.AddRange(items);
                        continue;
                    }

                    var projectIssues = project.Issues.Where(i => i.TypeId == violation).ToLookup(i => i.File);
                    var snapshotIssues = snapshotProject.Issues.Where(i => i.TypeId == violation).ToLookup(i => i.File);

                    foreach (var file in projectIssues)
                    {
                        if (!snapshotIssues.Contains(file.Key))
                        {
                            Console.WriteLine($"{project.Name}/{file.Key} - violations found in new file:");
                            var items = project.Issues.Where(i => i.TypeId == violation && i.File == file.Key).ToList();
                            foreach (var issue in items)
                            {
                                Console.WriteLine(issue);
                            }

                            issues.AddRange(items);
                            continue;
                        }

                        var projectFileIssues = projectIssues[file.Key].Count();
                        var snapshotFileIssues = snapshotIssues[file.Key].Count();

                        if (projectFileIssues <= snapshotFileIssues)
                        {
                            continue;
                        }

                        Console.WriteLine($"{project.Name}/{file.Key} - violations found in existing file:");
                        var allIssues = project.Issues.Where(i => i.TypeId == violation && i.File == file.Key).ToList();
                        foreach (var issue in allIssues)
                        {
                            Console.WriteLine(issue);
                        }

                        issues.AddRange(allIssues);
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

        private static async Task GenerateFile(ProcessStartInfo processStartInfo)
        {
            var watch = new Stopwatch();

            watch.Start();
            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            if (_settings.Debug)
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = await process.StandardOutput.ReadLineAsync();
                    Console.WriteLine(line);
                }
            }

            await process.WaitForExitAsync();
            watch.Start();
            Console.WriteLine($"Generation Time: {watch.ElapsedMilliseconds} ms");
        }
    }
}