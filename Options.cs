using CommandLine;
namespace DibcoEvaluation
{
    internal class Options
    {

        [Option('s', "submissions", Required = true, HelpText = "Define the submissions' path. It must contain multiple folders (for each submission).")]
        public string SubmissionsPath { get; set; }

        [Option('g', "gt", Required = true, HelpText = "The Ground Truth Image Files.")]
        public string GTPath { get; set; }

        [Option('w', "weights", Required = true, HelpText = "The Path of the weights' directory of the ground truth image (required for evaluation) with the same name with the GT images.")]
        public string WeightsDirectory { get; set; }
        [Option('o', "outputfile", Required = true, Default = null, HelpText = "Write the results to a CSV compatible file.")]
        public string OutputFile { get; set; }

        [Option("detailedoutput", Required = false, Default = false, HelpText = "Produce a detailed scored for each metric for each submission for each ground-truth image.")]
        public bool DetailedOutput { get; set; }

        [Option("perimagescore", Required = false, Default = true, HelpText = "Calculate the final score based on image position (DIBCO Competitions) instead of position on averages (H-DIBCO 2018)")]
        public bool ScoreByImage { get; set; }


    }
}
