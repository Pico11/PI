using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using System.IO;
using ExtractionModule;
using System.Collections.Generic;

namespace UnitTest
{
    class SampleListProvider : ISampleProvider
    {

        public float[] Samples { get; set; }
        private int sampleOffset { get; set; }
        public WaveFormat WaveFormat { get { return new WaveFormat(44100, 1); } }
        public int Read(float[] buffer, int offset, int count)
        {
            int c = 0, len = Math.Min(buffer.Length - offset, Samples.Length - sampleOffset);
            for (int i = 0; i < len; i++)
            {
                buffer[i + offset] = Samples[sampleOffset++];
            }

            return c;
        }
    }
    [TestClass]
    public class ExtractorTests
    {
        const string recordingsPath = @"..\nagrania\";

        const int extractionPeriod = 256;
        private bool stopped;
        private void ExtractDFTWindows(string file)
        {
            var waveReader = new WaveFileReader(Path.Combine(recordingsPath, file));
            var extractor = new Extractor();
            var samples = ReadAllSamples(waveReader);

            var expectedWindows = samples.Length / extractionPeriod;

            var preparedSamples = extractor.ProcessSamples(samples);
            var windows = extractor.ExtractWindows(samples, extractionPeriod);
            var frequencies = extractor.GetFrequencies(windows);
            var waveOut = new WaveOut();

            waveOut.Init(new SampleListProvider() { Samples = preparedSamples });
            waveOut.Play();
            waveOut.PlaybackStopped += waveOut_PlaybackStopped;
            while (!stopped) System.Threading.Thread.Sleep(1);

        }
        private void ExtractDFTWindowsShort(string file)
        {
            var waveReader = new WaveFileReader(Path.Combine(recordingsPath, file));
            var extractor = new Extractor();
            var samples = ReadAllSamples(waveReader);

            var expectedWindows = samples.Length / extractionPeriod;

            var preparedSamples = extractor.ProcessSamples(samples);
            var windows = extractor.ExtractWindows(samples, extractionPeriod);
            var frequencies = extractor.GetFrequencies(windows);
            var waveOut = new WaveOut();

            waveOut.Init(new SampleListProvider() { Samples = preparedSamples });
            waveOut.Play();
            waveOut.PlaybackStopped += waveOut_PlaybackStopped;
            while (!stopped) System.Threading.Thread.Sleep(1);

        }

        public float[] Generate(int samplesCount)
        {
            var samples = new float[samplesCount];
            var timeStep = 1 / 2048f;
            var current = 0.0;
            for (int i = 0; i < samplesCount; i++)
            {
                var s1 = Math.Sin(current * 123) * 4;
                var s2 = Math.Sin(current * 17);
                var c1 = Math.Cos(current * 43);
                var c2 = Math.Cos(current * 75) * 2;
                samples[i] = (float)(s1 + s2 + c1 + c2);
                switch (i % 4)
                {
                    case 0:
                        samples[i] += 0;
                        break;
                    case 1:
                        samples[i] += 3;
                        break;
                    case 2:
                        samples[i] += 0;
                        break;
                    case 3:
                        samples[i] += -3;
                        break;
                }
                current += timeStep;
            }

            return samples;
        }

        void waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            stopped = true;
        }
        [TestMethod]
        public void ExtractionTest1()
        {
            ExtractDFTWindows("mb1.wav");

        }
        [TestMethod]
        public void ExtractionTest2()
        {
            ExtractDFTWindows("mb2.wav");

        }
        [TestMethod]
        public void ExtractionTest3()
        {
            ExtractDFTWindows("mb3.wav");
        }
        [TestMethod]
        public void ExtractionTest4()
        {
            ExtractDFTWindows("hfdhghj.wav");

        }
        [TestMethod]
        public void ExtractionTest5()
        {
  
            
            var extractor = new Extractor();
            var samples = PrepareSamples(90000);

            var expectedWindows = samples.Length / extractionPeriod;

            var preparedSamples = extractor.ProcessSamples(samples);
            var windows = extractor.ExtractWindows(samples, extractionPeriod);
            var frequencies = extractor.GetFrequencies(windows);
            var waveOut = new WaveOut();

            waveOut.Init(new SampleListProvider() { Samples = preparedSamples });
            waveOut.Play();
            waveOut.PlaybackStopped += waveOut_PlaybackStopped;
            while (!stopped) System.Threading.Thread.Sleep(1);


        }

        private float[] PrepareSamples(int p)
        {
            var arr = new float[p];


            return arr;
        }
        private float[] ReadAllSamples(WaveFileReader waveReader)
        {
            var samplesList = new List<float>();
            float[] frame;
            while ((frame = waveReader.ReadNextSampleFrame()) != null && frame.Length > 0)
                samplesList.AddRange(frame);

            return samplesList.ToArray();
        }
    }
}
