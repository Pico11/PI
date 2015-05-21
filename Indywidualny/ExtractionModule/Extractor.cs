#define FloatSamples

#if !FloatSamples
#define ShortSamples
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nagrywanie;
using PrzygotowanieDanych;
using Spektrum;
using System.Numerics;
using Cechy;

namespace ExtractionModule
{
    public class Extractor
    {
        #region Recording
        private Recorder _recorder;
        public bool RecordingTimeElapsed { get; private set; }

        public void StartRecordingSamples(TimeSpan recordingTime)
        {
            if (_recorder != null) return;//Recording in progress
            _recorder = new Recorder();
            RecordingTimeElapsed = false;
            _recorder.StartRecording();
            Task.Run(() =>
            {
                Thread.Sleep(recordingTime);
                RecordingTimeElapsed = true;

            }).Wait();
            _recorder.StopRecordingPrepareSamples();
        }

        public void StartRecordingSamples(int numberOfSamples, int sampleRate)
        {
            StartRecordingSamples(TimeSpan.FromSeconds(numberOfSamples / (double)sampleRate));
        }

        public bool AreSamplesReady()
        {
            if (_recorder == null) throw new InvalidOperationException("Not recording");

            return RecordingTimeElapsed && _recorder.Samples != null;
        }

#if FloatSamples
        public float[] GetRecordedSamples()
        {
            if (!AreSamplesReady()) return null;

            var samples = _recorder.Samples;
            _recorder = null;
            return samples;
        }

        public float[] ForceGetRecordedSamples()
        {
            _recorder.ForceGetSamples();
            return _recorder.Samples;
        }
#endif
#if ShortSamples
        public short[] GetRecordedSamples()
        {
            if (!AreSamplesReady()) return null;

            var samples = _recorder.Samples;
            _recorder = null;
            return samples;
        } 
#endif

        #endregion


        public float[] ProcessSamples(float[] samples)
        {
            const float cutoffFrequency = 5000f;

            var filter = new Filter();
            var detector = new SilenceDetector();

            samples = OverflowRemover.RemoveOverflows(samples);
            var filtered = filter.LowPass(samples, Recorder.Format.SampleRate, cutoffFrequency);
            var norm1 = SampleNormarisation.NormalizeSamples(filtered);
            var voice = detector.CutSilence(norm1, 256);
            var normalised = SampleNormarisation.NormalizeSamples(voice);
            return normalised;
        }



        #region FrequencyCalculation
        const int TransformedWindowLength = 8192;
        const int WindowExtractionPeriod = 512;
        const int MaxDFTFreq = TransformedWindowLength / 2;
        public float[] ExtendWindowForDFT(float[] samples)
        {
            var leftLen = (TransformedWindowLength - samples.Length) / 2;
            var rightLen = TransformedWindowLength - leftLen - samples.Length;
            var resultList = new List<float>(TransformedWindowLength);
            for (var i = 0; i < leftLen; i++)
                resultList.Add(0);
            resultList.AddRange(samples);
            for (var i = 0; i < rightLen; i++)
                resultList.Add(0);

            return resultList.ToArray();
        }

        public IEnumerable<float[]> ExtractWindows(float[] samples, int extractionPeriod = WindowExtractionPeriod)
        {
            var frameExtractor = new FrameExtractor();
            var frames = frameExtractor.ExtractFrames(samples, extractionPeriod).Select(frame => ExtendWindowForDFT(Preemphasis.ApplyPreemphasis(frame)));

            return frames;
        }
        public IEnumerable<float[]> GetFrequencies(IEnumerable<float[]> frames)
        {
            var fourier = new FourierTransformer();
            var frequencies = frames.Select(frame => fourier.ApplyDFT(frame.Select(f => new Complex(f, 0)).ToArray()).Select(c => (float)c.Real).ToArray());

            return frequencies;
        }
        public IEnumerable<float[]> CalculateDFT(float[] samples)
        {
            return GetFrequencies(ExtractWindows(samples));
        }

        #endregion

        #region TraitExtraction

        private const int FilterCount = 16;

        public float[] GetRangesEnergy(float[] frequencies)
        {
            var filters = FrequencyFilter.CreateFrequencyFilters(FilterCount);
            var e = FrequencyFilter.GetMeans(FrequencyFilter.ApplyFilters(frequencies, Recorder.Format.SampleRate / (float)TransformedWindowLength, filters));
            return e;
        }

        #endregion

        private const string DataDirectory = @"\ExtractedData";

        public string CreateUserTraitsFile(string user)
        {
            if (string.IsNullOrWhiteSpace(user)) throw new ArgumentException("User name");
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
            var fileName = string.Format("{0}\\{1}_{2:yyMMdd_hhmm}.spk", DataDirectory, user, DateTime.Now);
            StartRecordingSamples(TimeSpan.FromSeconds(5));
            var tryCount = 20;
            var tryN=0;
            while (!AreSamplesReady()&&tryN<tryCount)
            {
                Thread.Sleep(10);
                tryN++;
            }
            
            var samples = GetRecordedSamples();
            samples = samples ?? ForceGetRecordedSamples();
            var processed = ProcessSamples(samples);
            var frequencies = CalculateDFT(processed).ToArray();
            var energies = frequencies.Select(GetRangesEnergy).ToArray();
            using (var file = new StreamWriter(fileName))
            {
                file.WriteLine("SpeakerData");
                file.WriteLine("Login: {0}", user);
                file.WriteLine("Trait Vectors");
                foreach (var energy in energies)
                {
                    file.Write(energy[0]);
                    for (int i = 1; i < energy.Length; i++)
                    {
                        file.Write(" {0}", energy[i]);
                    }
                    file.WriteLine();
                }
            }
            return fileName;
        }
    }



}
