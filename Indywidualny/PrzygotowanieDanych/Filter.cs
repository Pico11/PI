using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NAudio.Dsp;
using NAudio.Wave;

namespace PrzygotowanieDanych
{
    public class Filter
    {

        const float Q = 1.0f;
        public void LowPass(string pathIn, string pathOut, float maxFreq)
        {
            using (var reader = new WaveFileReader(pathIn))
            {
                var format = reader.WaveFormat;

                var filter = BiQuadFilter.LowPassFilter(format.SampleRate, maxFreq, Q);
                using (var writer = new WaveFileWriter(pathOut, format))
                {
                    float[] frame;

                    while ((frame = reader.ReadNextSampleFrame()) != null && frame.Length > 0)
                    {
                        foreach (var sample in frame)
                        {
                            writer.WriteSample(filter.Transform(sample));
                        }
                    }
                }
            }
        }

        public void HighPass(string pathIn, string pathOut, float minFreq)
        {
            using (var reader = new WaveFileReader(pathIn))
            {
                var format = reader.WaveFormat;

                var filter = BiQuadFilter.HighPassFilter(format.SampleRate, minFreq, Q);
                using (var writer = new WaveFileWriter(pathOut, format))
                {
                    float[] frame;

                    while ((frame = reader.ReadNextSampleFrame()) != null && frame.Length > 0)
                    {
                        foreach (var sample in frame)
                        {
                            writer.WriteSample(filter.Transform(sample));
                        }
                    }
                }
            }
        }

        public float[] LowPass(float[] samples, int sampleRate, float maxFreq)
        {
            var filter = BiQuadFilter.LowPassFilter(sampleRate, maxFreq, Q);
            return samples.Select(sample => filter.Transform(sample)).ToArray();
        }
        public float[] HighPass(float[] samples, int sampleRate, float minFreq)
        {
            var filter = BiQuadFilter.HighPassFilter(sampleRate, minFreq, Q);
            return samples.Select(sample => filter.Transform(sample)).ToArray();
        }

        private void CreateFrequencyFilters(int filterCount, out float[] centerFrequencies, out float[] ranges)
        {
            centerFrequencies=new float[filterCount];
            ranges = new float[filterCount];
            const int borderFrequency = 6000;
            var step = borderFrequency/filterCount;
            for (int i = 0; i < filterCount; i++)
            {
                centerFrequencies[i] = step*(i + 1);
                ranges[i] = step;
            }
        }
        //public float[] FrequencyFilter(float[] frequencies, float[] centerFrequencies, float[] ranges)
        //{
        //    if(frequencies==null) throw new ArgumentNullException();
        //    if (centerFrequencies == null) throw new ArgumentNullException();
        //    if (ranges == null) throw new ArgumentNullException();
        //    if(centerFrequencies.Length!=ranges.Length) throw new ArgumentException();
        //    var results = new float[frequencies.Length];

        //}
    }
}
