using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrzygotowanieDanych
{
    public class SampleNormarisation
    {
        /// <summary>
        /// Normalize samples to [-1f;1f] range
        /// </summary>
        /// <param name="samples">samples to normalize</param>
        /// <returns>array of normalized samples</returns>
        public static float[] NormalizeSamples(float[] samples)
        {
            var average = samples.Average();
            var maxDiff = samples.Max(sample => sample - average);
            return samples.Select(sample=>(sample-average)/maxDiff).ToArray();
        }
    }
}
