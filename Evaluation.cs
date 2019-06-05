using System;
using System.Globalization;

namespace DibcoEvaluation
{
    internal class Evaluation
    {
        public float FMeasure { get; set; }
        public float FMeasureRecall { get; set; }
        public float FMeasurePrecision { get; set; }
        public float PseudoFMeasure { get; set; }
        public float PseudoFMeasureRecall { get; set; }
        public float PseudoFMeasurePrecision { get; set; }
        public float PSNR { get; set; }

        public float DRD { get; set; }

        public string ID { get; set; }

        public Evaluation(string ID, string EvaluationResultsString)
        {
            if (EvaluationResultsString.Length == 0) return;
            this.ID = ID;
            var lines = EvaluationResultsString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != 2) throw new FormatException($"Evaluation Output not the expected format. Lines number: {lines.Length} instead of 2");
            var values = lines[1].Split(';');
            if (values.Length != 8) throw new FormatException($"Evaluation Output not the expected format. values number: {values.Length} instead of 7");
            FMeasure = Convert.ToSingle(values[0] == "NaN" || values[0] == "Inf" ? "0" : values[0], CultureInfo.InvariantCulture);
            PseudoFMeasure = Convert.ToSingle(values[1] == "NaN" || values[1] == "Inf" ? "0" : values[1], CultureInfo.InvariantCulture);
            PSNR = Convert.ToSingle(values[2] == "NaN" || values[2] == "Inf" ? "0" : values[2], CultureInfo.InvariantCulture);
            DRD = Convert.ToSingle(values[3] == "NaN" || values[3] == "Inf" ? "1000" : values[3], CultureInfo.InvariantCulture);
            FMeasureRecall = Convert.ToSingle(values[4] == "NaN" || values[4] == "Inf" ? "0" : values[4], CultureInfo.InvariantCulture);
            FMeasurePrecision = Convert.ToSingle(values[5] == "NaN" || values[5] == "Inf" ? "0" : values[5], CultureInfo.InvariantCulture);
            PseudoFMeasureRecall = Convert.ToSingle(values[6] == "NaN" || values[6] == "Inf" ? "0" : values[6], CultureInfo.InvariantCulture);
            PseudoFMeasurePrecision = Convert.ToSingle(values[7] == "NaN" || values[7] == "Inf" ? "0" : values[7], CultureInfo.InvariantCulture);

        }

        public Evaluation() { }

    }
}
