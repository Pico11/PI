using System;

namespace PrzygotowanieDanych
{
    public class Preemphasis
    {

        public static float[] ApplyPreemphasis(float[] samples,float alpha=0.95f)
        {
            if (samples == null) throw new ArgumentNullException("samples");
            var floats = new float[samples.Length];
            floats[0] = samples[0];
            for (var i = 1; i < samples.Length; i++)
            {
                floats[i] = samples[i] - alpha*samples[i - 1];
            }
            return floats;
        }
    }
}