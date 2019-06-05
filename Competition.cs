using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace DibcoEvaluation
{
    internal class Competition
    {
        private readonly Dictionary<string, Submission> Submissions;
        private Dictionary<string, Dictionary<string, int>> FMeasureScore;
        private Dictionary<string, Dictionary<string, int>> PseudoFMeasureScore;
        private Dictionary<string, Dictionary<string, int>> PSNRScore;
        private Dictionary<string, Dictionary<string, int>> DRDScore;
        private Dictionary<string, int> Score;
        private readonly string[] ScoreOrdered;
        private readonly string[] ImageNames;
        private readonly bool PerImage;
        private const string AverageLabel = "A";

        public Competition(IEnumerable<Submission> MySubmissions, bool PerImage = false)
        {
            this.Submissions = MySubmissions.ToDictionary(k => k.Name, v => v);
            ImageNames = Submissions.Values.FirstOrDefault().Results.Keys.OrderBy(x => x).ToArray();
            this.PerImage = PerImage;

            if (PerImage)
                CalculateMetricScoresPerImage();
            else
                CalculateMetricScores();




            // Calculate Global Score

            this.Score = this.Submissions.Keys.ToDictionary(k => k, CalculateScore);
            this.ScoreOrdered = this.Score.OrderBy(s => s.Value)
                .Select(s => s.Key).ToArray();

        }

        public override string ToString()
        {
            var output = new StringBuilder("Rank,Method,Score,F-Measure,P-FMeasure,PSNR,DRD,Recall,Precision,P-Recall,P-Precision\n");
            for (int i = 0; i < ScoreOrdered.Length; i++)
                output.AppendLine(SubmissionString(i + 1, ScoreOrdered[i]));
            return output.ToString();
        }

        public string ToDetailedString()
        {
            var output = new StringBuilder($"Rank,Method,Score,F-Measure,{ImageNamesString},P-FMeasure,{ImageNamesString},PSNR,{ImageNamesString},DRD,{ImageNamesString}\n");
            for (int i = 0; i < ScoreOrdered.Length; i++)
            {
                var name = ScoreOrdered[i];
                var o = new StringBuilder();
                o.Append($"{i + 1},");
                o.Append($"{name},");
                o.Append($"{Score[name]},");
                o.Append($"{Submissions[name].MeanResults.FMeasure.ToString("0.##", CultureInfo.InvariantCulture)},");
                if (PerImage)
                    foreach (var img in ImageNames)
                        o.Append($"{FMeasureScore[name][img]},");
                else
                    o.Append($"{FMeasureScore[name][AverageLabel]},");
                o.Append($"{Submissions[name].MeanResults.PseudoFMeasure.ToString("0.##", CultureInfo.InvariantCulture)},");

                if (PerImage)
                    foreach (var img in ImageNames)
                        o.Append($"{PseudoFMeasureScore[name][img]},");
                else
                    o.Append($"{FMeasureScore[name][AverageLabel]},");
                o.Append($"{Submissions[name].MeanResults.PSNR.ToString("0.##", CultureInfo.InvariantCulture)},");

                if (PerImage)
                    foreach (var img in ImageNames)
                        o.Append($"{PSNRScore[name][img]},");
                else
                    o.Append($"{FMeasureScore[name][AverageLabel]},");
                o.Append($"{Submissions[name].MeanResults.DRD.ToString("0.##", CultureInfo.InvariantCulture)},");

                if (PerImage)
                    foreach (var img in ImageNames)
                        o.Append($"{DRDScore[name][img]},");
                else
                    o.Append($"{FMeasureScore[name][AverageLabel]},");
                output.AppendLine(o.ToString());
            }

            return output.ToString();
        }

        private string ImageNamesString => (PerImage ? String.Join(" Score,", ImageNames) : AverageLabel) + " Score";



        private int CalculateScore(string SubmissionName) =>
            FMeasureScore[SubmissionName].Values.Sum() + PseudoFMeasureScore[SubmissionName].Values.Sum()
            + PSNRScore[SubmissionName].Values.Sum() + DRDScore[SubmissionName].Values.Sum();

        private string SubmissionString(int Index, string Name) =>
        $"{Index},{Name},{Score[Name]},{Submissions[Name].MeanResultsString}";

        private void CalculateMetricScores()
        {
            // Calculate Score based on ranking
            this.FMeasureScore =
                Submissions
                .OrderByDescending(s => s.Value.MeanResults.FMeasure)
                .Select((s, index) => new { s.Value.Name, index })
                .ToDictionary(k => k.Name, v => new Dictionary<string, int> { { AverageLabel, v.index + 1 } });


            this.PseudoFMeasureScore = Submissions
                .OrderByDescending(s => s.Value.MeanResults.PseudoFMeasure)
                .Select((s, index) => new { s.Value.Name, index })
                .ToDictionary(k => k.Name, v => new Dictionary<string, int> { { AverageLabel, v.index + 1 } });

            this.PSNRScore = Submissions
                .OrderByDescending(s => s.Value.MeanResults.PSNR)
                .Select((s, index) => new { s.Value.Name, index })
                .ToDictionary(k => k.Name, v => new Dictionary<string, int> { { AverageLabel, v.index + 1 } });

            this.DRDScore = Submissions
                .OrderBy(s => s.Value.MeanResults.DRD)
                .Select((s, index) => new { s.Value.Name, index })
                .ToDictionary(k => k.Name, v => new Dictionary<string, int> { { AverageLabel, v.index + 1 } });
        }

        private void CalculateMetricScoresPerImage()
        {


            this.FMeasureScore = ImageNames.SelectMany(img => Submissions
            .GroupBy(x => x.Value.Results[img].FMeasure)
                .OrderByDescending(s => s.Key)
                .Select((s, index) => s.Select(x => new { x.Value.Name, index, img }))
                .SelectMany(a => a))
                .GroupBy(g => g.Name)
                .ToDictionary(k => k.Key, v => v.ToDictionary(x => x.img, y => y.index + 1));

            this.PseudoFMeasureScore = ImageNames.SelectMany(img => Submissions
                .GroupBy(x => x.Value.Results[img].PseudoFMeasure)
                .OrderByDescending(s => s.Key)
                .Select((s, index) => s.Select(x => new { x.Value.Name, index, img }))
                .SelectMany(a => a))
                .GroupBy(g => g.Name)
                .ToDictionary(k => k.Key, v => v.ToDictionary(x => x.img, y => y.index + 1));

            this.PSNRScore = ImageNames.SelectMany(img => Submissions
                .GroupBy(x => x.Value.Results[img].PSNR)
                .OrderByDescending(s => s.Key)
                .Select((s, index) => s.Select(x => new { x.Value.Name, index, img }))
                .SelectMany(a => a))
                .GroupBy(g => g.Name)
                .ToDictionary(k => k.Key, v => v.ToDictionary(x => x.img, y => y.index + 1));

            this.DRDScore = ImageNames.SelectMany(img => Submissions
                .GroupBy(x => x.Value.Results[img].DRD)
                .OrderByDescending(s => s.Key)
                .Select((s, index) => s.Select(x => new { x.Value.Name, index, img }))
                .SelectMany(a => a))
                .GroupBy(g => g.Name)
                .ToDictionary(k => k.Key, v => v.ToDictionary(x => x.img, y => y.index + 1));
        }



    }
}
