using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Cechy
{
    public class FrequencyFilter
    {
        public float Center { get; private set; }
        public float Range { get; private set; }
        public static FrequencyFilter[] CreateFrequencyFilters(int filterCount,int borderFrequency=4096)
        {
            var filters = new FrequencyFilter[filterCount];

            var step = borderFrequency / filterCount;
            for (var i = 0; i < filterCount; i++)
            {
                filters[i]=new FrequencyFilter{Center=step * (i + 1),Range=step};
            }

            return filters;
        }

        public float RealFrequency(int index, float ratio)
        {
            return ratio * index;
        }
        public float[] ApplyFilter(float[] frequencies, float ratio)
        {
            //real frequency is ratio*i where ratio = fsampling/sampleCount and i is index in frequencies array
            var left = Center - Range;
            var right = Center + Range;
            var filterCr = Center / Range;
            var filtered = new float[frequencies.Length];
            for (var i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] <= left || frequencies[i] >= right)
                {
                    filtered[i] = 0;
                    continue;
                }
                var freq = RealFrequency(i, ratio);
                if (freq <= Center)
                {
                    filtered[i] = freq / Range + 1 - filterCr;
                }
                else
                {
                    filtered[i] = -freq / Range + 1 + filterCr;
                }
            }

            return filtered;
        }
        public static float[][] ApplyFilters(float[] frequencies, float ratio, FrequencyFilter[] filters)
        {
            var results = new float[filters.Length][];
            for (var i = 0; i < filters.Length; i++)
            {
                results[i] = filters[i].ApplyFilter(frequencies, ratio);
            }
            return results;
        }

        public static float[] GetMeans(float[][] filteredFrequencies)
        {
            return filteredFrequencies.Select(frequency=>frequency.Average()).ToArray();
        }
    }
}
