using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using ShellProgressBar;

namespace DibcoEvaluation
{
    internal static class Program
    {
        private static readonly string ExecutableMetrics = Path.GetFullPath("./Externals/DIBCO_metrics.exe");
        private static Options CurrentOptions;
        internal static ProgressBar GlobalProgressBar;

        private static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunEvaluation);

        }

        private static void RunEvaluation(Options options)
        {
            CurrentOptions = options;

            var submissions = Directory.GetDirectories(CurrentOptions.SubmissionsPath);
            GlobalProgressBar = new ProgressBar(submissions.Count() + 1, $"Detected {submissions.Count()} Submissions. Evaluating...", new ProgressBarOptions
            {
                ProgressBarOnBottom = true,
                ProgressCharacter = '─',
                CollapseWhenFinished = false,
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,

            });

            var resultsSubmissions = submissions.AsParallel()
            .Select(RunDibcoEvaluation).ToList();

            var Output = new StringBuilder();

            var competition = new Competition(resultsSubmissions, options.ScoreByImage);
            Output.AppendLine(competition.ToString());
            GlobalProgressBar.Tick($"Finish the Evaluation of all Submissions. Writing to the File: {options.OutputFile}");

            if (options.DetailedOutput)
            {

                Output.AppendLine(competition.ToDetailedString());
                foreach (var r in resultsSubmissions)
                    Output.AppendLine(r.ToString());
            }

            try
            {
                if (CurrentOptions.OutputFile == null)
                    Console.Write(Output.ToString());
                else
                    File.WriteAllText(CurrentOptions.OutputFile, Output.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ERROR: Cannot save to file!
                 {ex.Message}
                 Writing the results to the console:
                 {Output.ToString()}               
                ");
            }
            finally
            {
                Console.Clear();
            }
        }

        private static Submission RunDibcoEvaluation(string SubmissionFolder)
        {
            var files = Directory.GetFiles(SubmissionFolder, "*.bmp");
            var child = GlobalProgressBar.Spawn(files.Count(), $"Detected {files.Count()} Images. Calculating DIBCO metrics", new ProgressBarOptions
            {
                ProgressBarOnBottom = false,
                ProgressCharacter = '─',
                CollapseWhenFinished = false,
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ForegroundColorDone = ConsoleColor.DarkYellow
            });
            var submissionName = Path.GetFileNameWithoutExtension(SubmissionFolder);
            var results = files.AsParallel()
                .Select(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var evaluation = new Evaluation(
                                               name,
                                           ExecuteCommand(
                                               ExecutableMetrics,
                                               $"{GetGTFileName(name)} {f} {GetRWeightsFileName(name)} {GetPWeightsFileName(name)}"
                                           )
                                           );
                child.Tick($"{submissionName}: {child.CurrentTick + 1} of {child.MaxTicks} Images Evaluated (Last Image: {name} )");
                return evaluation;
            }).ToList();

            var submission = new Submission(SubmissionFolder, results);
            GlobalProgressBar.Tick($"Submission {GlobalProgressBar.CurrentTick + 1} of {GlobalProgressBar.MaxTicks - 1} completed (Last Submission: {submission.Name})");
            return submission;
        }

        private static string ExecuteCommand(string FileName, string Arguments)
        {
            var results = new StringBuilder();

            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }))
            {
                while (!process.HasExited)
                    results.Append(process.StandardOutput.ReadToEnd());

            }
            return results.ToString();
        }


        private static string GetGTFileName(string Filename) =>
            Path.Combine(CurrentOptions.GTPath, Path.GetFileNameWithoutExtension(Filename) + ".bmp");

        private static string GetRWeightsFileName(string Filename) =>
            Path.Combine(CurrentOptions.WeightsDirectory, Path.GetFileNameWithoutExtension(Filename) + "_RWeights.dat");

        private static string GetPWeightsFileName(string Filename) =>
            Path.Combine(CurrentOptions.WeightsDirectory, Path.GetFileNameWithoutExtension(Filename) + "_PWeights.dat");




    }
}
