using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;

namespace Nagrywanie
{
    public class Filter
    {

        const float Q = 1.0f;
        public void LowPass(string pathIn, string pathOut, float maxFreq)
        {
            var reader = new WaveFileReader(pathIn);
            var format = reader.WaveFormat;
            
            var filter=BiQuadFilter.LowPassFilter(format.SampleRate,maxFreq,Q);
            var writer = new WaveFileWriter(pathOut,format);
            float[] frame;
            
            while ((frame=reader.ReadNextSampleFrame())!=null&&frame.Length>0)
            {
                foreach (var sample in frame)
                {
                    writer.WriteSample(filter.Transform(sample));
                }
            } 
        }
    }
}
