namespace PrzygotowanieDanych
{
    public static class OverflowRemover
    {
        public static float[] RemoveOverflows(float[] samples, float treshold = 0.1f)
        {
            var cleaned = new float[samples.Length];
            cleaned[0] = samples[0];
            var upper = 1 - treshold;
            var lower = treshold - 1;
            for (var i = 0; i < samples.Length; i++)
            {
                //var difference = samples[i] - samples[i - 1];
                cleaned[i] = samples[i];
                //if (Math.Abs(difference) < 1) continue;

                //possibly overflow
               
                if (cleaned[i - 1] > upper && samples[i] < lower) //additional check
                {
                    cleaned[i] = samples[i] + 1;
                }
                else
                {
                    if (cleaned[i - 1] < lower && samples[i] > upper)//underflow
                    {
                        cleaned[i] = samples[i] - 1;
                    }
                }
            }
            return cleaned;
        }
    }
}