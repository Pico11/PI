namespace PrzygotowanieDanych
{
    public static class OverflowRemover
    {
        private const float FloatMax = 1;
        private const double DoubleMax = 1;
        private const float FloatRange = 2 * FloatMax;
        private const double DoubleRange = 2 * DoubleMax;

        public static float[] RemoveOverflows(float[] samples, float treshold = 0.1f)
        {
            var cleaned = new float[samples.Length];
            cleaned[0] = samples[0];
            var upper = FloatMax - treshold;
            var lower = treshold - FloatMax;
            for (var i = 1; i < samples.Length; i++)
            {
                cleaned[i] = samples[i];

                //var difference = samples[i] - samples[i - 1];
                //if (Math.Abs(difference) < 1) continue;
                ////possibly overflow
                if (cleaned[i - 1] > upper && samples[i] < 0) //check
                {
                    cleaned[i] = samples[i] + FloatRange;
                }
                else
                {
                    if (cleaned[i - 1] < lower && samples[i] > 0)//underflow
                    {
                        cleaned[i] = samples[i] - FloatRange;
                    }
                }
            }
            return cleaned;
        }
        public static double[] RemoveOverflows(double[] samples, double treshold = 0.1d)
        {
            var cleaned = new double[samples.Length];
            cleaned[0] = samples[0];
            var upper = DoubleMax - treshold;
            var lower = treshold - DoubleMax;
            for (var i = 1; i < samples.Length; i++)
            {
                //var difference = samples[i] - samples[i - 1];
                //if (Math.Abs(difference) < 1) continue;
                cleaned[i] = samples[i];
                if (cleaned[i - 1] > upper && samples[i] < 0)
                {
                    cleaned[i] = samples[i] + DoubleRange;
                }
                else
                {
                    if (cleaned[i - 1] < lower && samples[i] > 0)//underflow
                    {
                        cleaned[i] = samples[i] - DoubleRange;
                    }
                }
            }
            return cleaned;
        }
    }
}