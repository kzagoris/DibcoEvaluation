using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DibcoEvaluation
{
    internal class Submission
    {

        internal string Name { get; set; }
        internal string Folder { get; set; }
        internal Evaluation MeanResults { get; set; }
        internal Dictionary<string, Evaluation> Results { get; set; }


        internal Submission(string Folder, IEnumerable<Evaluation> Results)
        {
            this.Folder = Folder;
            this.Name = Path.GetFileNameWithoutExtension(Folder);
            this.MeanResults = new Evaluation
            {
                FMeasure = Results.Average(x => x.FMeasure),
                PseudoFMeasure = Results.Average(x => x.PseudoFMeasure),
                PSNR = Results.Average(x => x.PSNR),
                DRD = Results.Average(x => x.DRD),
                FMeasureRecall = Results.Average(x => x.FMeasureRecall),
                FMeasurePrecision = Results.Average(x => x.FMeasurePrecision),
                PseudoFMeasureRecall = Results.Average(x => x.PseudoFMeasureRecall),
                PseudoFMeasurePrecision = Results.Average(x => x.PseudoFMeasurePrecision),
                ID = Name
            };
            this.Results = Results.ToDictionary(k => k.ID, v => v);
        }

        public override string ToString()
        {
            var output = new StringBuilder("Image,F-Measure,P-FMeasure,PSNR,DRD,Recall,Precision,P-Recall, P-Precision\n");
            output.AppendLine(ResultsString);
            output.AppendLine(MeanResultsStringWithID);
            return output.ToString();
        }

        public string MeanResultsString { get => Output(MeanResults); }

        private string ResultsString
        {
            get
            {
                var output = new StringBuilder();
                foreach (var r in this.Results.Values.OrderBy(x => x.ID))
                    output.AppendLine(OutputWithID(r));
                return output.ToString();
            }

        }

        private string MeanResultsStringWithID { get => OutputWithID(MeanResults); }



        private string OutputWithID(Evaluation r) =>
        $"{r.ID},{Output(r)}";

        private string Output(Evaluation r) =>
        $"{r.FMeasure.ToString("0.##", CultureInfo.InvariantCulture)},{r.PseudoFMeasure.ToString("0.##", CultureInfo.InvariantCulture)},{r.PSNR.ToString("0.##", CultureInfo.InvariantCulture)},{r.DRD.ToString("0.##", CultureInfo.InvariantCulture)},{r.FMeasureRecall.ToString("0.##", CultureInfo.InvariantCulture)},{r.FMeasurePrecision.ToString("0.##", CultureInfo.InvariantCulture)},{r.PseudoFMeasureRecall.ToString("0.##", CultureInfo.InvariantCulture)},{r.PseudoFMeasurePrecision.ToString("0.##", CultureInfo.InvariantCulture)}";
    }
}
