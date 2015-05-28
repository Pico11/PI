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
            var maxSample = samples.Max(s=>Math.Abs(s));
            //var maxDiff = samples.Max(sample => sample - average);
            var normalized=samples.Select(sample=>sample/maxSample).ToArray();
            var newMax = normalized.Max();
            var newMin = normalized.Min();
            var newAvg = normalized.Average();
            return normalized;
        }
    }
}
