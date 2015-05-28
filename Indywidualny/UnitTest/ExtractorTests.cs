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
