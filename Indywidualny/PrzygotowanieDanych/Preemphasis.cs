namespace PrzygotowanieDanych
{
    public class Preemphasis
    {

        public static float[] ApplyPreemphasis(float[] samples,float alpha=0.95f)
        {
            var floats = new float[samples.Length];
            for (int i = 1; i < samples.Length; i++)
            {
                floats[i] = samples[i] - alpha*samples[i - 1];
            }
            return floats;
        }
    }
}