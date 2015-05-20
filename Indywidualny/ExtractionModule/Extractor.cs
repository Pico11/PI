using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nagrywanie;
using PrzygotowanieDanych;
using Spektrum;
using System.Numerics;

namespace ExtractionModule
{
    public class Extractor
    {
        private Recorder _recorder;
        private bool _recordingTimeElapsed;

        public void StartRecordingSamples(TimeSpan recordingTime)
        {
            _recorder = new Recorder();
            _recordingTimeElapsed = false;
            _recorder.StartRecording();
            Task.Run(() =>
            {
                Thread.Sleep(recordingTime);
                _recorder.StopRecordingPrepareSamples();
            });
        }

        public void StartRecordingSamples(int numberOfSamples, int sampleRate)
        {
            StartRecordingSamples(TimeSpan.FromSeconds(numberOfSamples/(double) sampleRate));
            
        }

        public bool AreSamplesReady()
        {
            if(_recorder==null) throw new InvalidOperationException("Not recording");

            return _recordingTimeElapsed && _recorder.Samples != null;
        }

        //public float[] GetRecordedSamples()
        //{
        //    if (!AreSamplesReady()) return null;

        //    var samples = _recorder.Samples;
        //    _recorder = null;
        //    return samples;
        //}
        public short[] GetRecordedSamples()
        {
            if (!AreSamplesReady()) return null;

            var samples = _recorder.Samples;
            _recorder = null;
            return samples;
        }

        const int TransformedWindowLength = 8192;
        const int MaxDFTFreq = TransformedWindowLength/2;
        const int WindowExtractionPeriod = 256;

        public float[] ExtendWindowForDFT(float[] samples)
        {
            var leftLen = (TransformedWindowLength - samples.Length) / 2;
            var rightLen = TransformedWindowLength - leftLen - samples.Length;
            var resultList = new List<float>(TransformedWindowLength);
            for (int i = 0; i < leftLen; i++)
                resultList.Add(0);
            resultList.AddRange(samples);
            for (int i = 0; i < rightLen; i++)
                resultList.Add(0);

            return resultList.ToArray();
        }

        public float[] ProcessSamples(float[] samples)
        {
            const float cutoffFrequency = 5000f;
            var filter = new Filter();
            var detector = new SilenceDetector();
            var filtered = filter.LowPass(samples, Recorder.Format.SampleRate, cutoffFrequency);
            var norm1 = SampleNormarisation.NormalizeSamples(filtered);
            var voice = detector.CutSilence(norm1, 256);
            var normalised = SampleNormarisation.NormalizeSamples(voice);
            return normalised;
        }

        public IEnumerable<float[]> ExtractWindows(float[] samples, int extractionPeriod = WindowExtractionPeriod)
        {
            var frameExtractor = new FrameExtractor();
            var frames = frameExtractor.ExtractFrames(samples, extractionPeriod).Select(frame => ExtendWindowForDFT(Preemphasis.ApplyPreemphasis(frame)));

            return frames;
        }
        public IEnumerable<float[]> GetFrequencies(IEnumerable<float[]> frames)
        {
            var fourier=new FourierTransformer();
            var frequencies=frames.Select(frame => fourier.ApplyDFT(frame.Select(f => new Complex(f, 0)).ToArray()).Select(c => (float)c.Real).ToArray());

            return frequencies;
        }
        public IEnumerable<float[]> CalculateDFT(float[] samples)
        {
            return GetFrequencies(ExtractWindows(samples));
        }




    }


    
}
