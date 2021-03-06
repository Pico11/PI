﻿#define WriteDebugFiles
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
using System.Xml.Serialization;

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
            _recorder.StartRecording((int)(44100 * recordingTime.TotalSeconds));
            var thisThread = Thread.CurrentThread;
            Task.Run(() =>
            {
                Thread.Sleep(recordingTime);
                RecordingTimeElapsed = true;
            });
            while (!RecordingTimeElapsed)
                ;//Thread.Sleep(5);
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
            var voice = detector.CutSilence(samples, 256);
            var filtered = filter.LowPass(voice, Recorder.Format.SampleRate, cutoffFrequency);

            var normalised = SampleNormarisation.NormalizeSamples(filtered);
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

            var frames = frameExtractor.ExtractFrames(samples, extractionPeriod).ToArray();
            var extended = frames.Select(frame => ExtendWindowForDFT(Preemphasis.ApplyPreemphasis(frame)));

            return extended;
        }

        private static float[] GetFrequencies(float[] frame)
        {
            var fourier = new FourierTransformer();
            return fourier.ApplyFFT(frame.Select(f => new Complex(f, 0)).ToArray()).Select(c => (float)c.Magnitude).ToArray();
        }
        public IEnumerable<float[]> GetFrequencies(IEnumerable<float[]> frames)
        {
            return frames.Select(GetFrequencies);
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
            var current = Directory.GetCurrentDirectory();
            var dir = current + DataDirectory;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var fileName = string.Format("{0}_{1:yyMMdd_hhmm}.spk", user, DateTime.Now);
            var filePath = Path.Combine(dir, fileName);

            StartRecordingSamples(TimeSpan.FromSeconds(5));

            var samples = GetRecordedSamples();
            samples = samples ?? ForceGetRecordedSamples();
#if WriteDebugFiles
            var tmpFile = System.IO.Path.GetRandomFileName();
            using (var fileStream = new FileStream(System.IO.Path.ChangeExtension(tmpFile, "rav"), FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var sample in samples)
                    {
                        writer.Write(sample);
                    }
                }
            }
#endif
            var processed = ProcessSamples(samples);
#if WriteDebugFiles
            using (var fileStream = new FileStream(System.IO.Path.ChangeExtension(tmpFile, "proc"), FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var sample in processed)
                    {
                        writer.Write(sample);
                    }
                }
            }
#endif

            var frequencies = CalculateDFT(processed).ToArray();
            var energies = frequencies.Select(GetRangesEnergy).ToArray();
            var energies2 = frequencies.Select(fT => GetRangesEnergy(fT.Take(fT.Length / 2).ToArray())).ToArray();
            var clustering = KMeans.Cluster(energies2, 16);
            using (var file = new StreamWriter(filePath))
            {
                file.WriteLine("SpeakerData");
                file.WriteLine("Login: {0}", user);
                file.WriteLine("Trait Vectors");
                for (int i = 0; i < energies2.Length; i++)
                {
                    var energy = energies2[i];
                    for (int j = 0; j < energy.Length; j++)
                    {
                        file.Write("{0} ", energy[j]);
                    }
                    file.WriteLine(clustering[i]);
                }
                //foreach (var energy in energies)
                //{
                //    file.Write(energy[0]);
                //    for (int i = 1; i < energy.Length; i++)
                //    {
                //        file.Write(" {0}", energy[i]);
                //    }
                //    file.WriteLine();
                //}

            }
            return fileName;
        }
        public string CreateUserTraitsFile(string user, Action<string> setStatusAction)
        {
            if (string.IsNullOrWhiteSpace(user)) throw new ArgumentException("User name");
            var current = Directory.GetCurrentDirectory();
            var dir = current + DataDirectory;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var fileName = string.Format("{0}_{1:yyMMdd_hhmm}.spk", user, DateTime.Now);
            var filePath = Path.Combine(dir, fileName);

            setStatusAction("Please speak");
            StartRecordingSamples(TimeSpan.FromSeconds(5));
            setStatusAction("Thank you\nNow processing");

            var samples = GetRecordedSamples();
            samples = samples ?? ForceGetRecordedSamples();
#if WriteDebugFiles
            var tmpFile = System.IO.Path.GetRandomFileName();
            using (var fileStream = new FileStream(System.IO.Path.ChangeExtension(tmpFile, "rav"), FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var sample in samples)
                    {
                        writer.Write(sample);
                    }
                }
            }
#endif
            var processed = ProcessSamples(samples);
#if WriteDebugFiles
            using (var fileStream = new FileStream(System.IO.Path.ChangeExtension(tmpFile, "proc"), FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var sample in processed)
                    {
                        writer.Write(sample);
                    }
                }
            }
#endif

            var frequencies = CalculateDFT(processed).ToArray();
            var energies = frequencies.Select(GetRangesEnergy).ToArray();
            var energies2 = frequencies.Select(fT => GetRangesEnergy(fT.Take(fT.Length / 2).ToArray())).ToArray();
            var clustering = KMeans.Cluster(energies2, 16);
            var frameList = new List<Frame>();
            using (var file = new StreamWriter(filePath))
            {
                file.WriteLine("SpeakerData");
                file.WriteLine("Login: {0}", user);
                file.WriteLine("Trait Vectors");
                for (int i = 0; i < energies2.Length; i++)
                {
                    var energy = energies2[i];
                    for (int j = 0; j < energy.Length; j++)
                    {
                        file.Write("{0} ", energy[j]);
                    }
                    file.WriteLine(clustering[i]);
                    frameList.Add(new Frame { TraitVector = energy, Cluster = clustering[i] });
                }

                //foreach (var energy in energies)
                //{
                //    file.Write(energy[0]);
                //    for (int i = 1; i < energy.Length; i++)
                //    {
                //        file.Write(" {0}", energy[i]);
                //    }
                //    file.WriteLine();
                //}


            }
            var serializer = new XmlSerializer(typeof(List<Frame>));
            serializer.Serialize(new FileStream(fileName.Replace(".spk", ".xml"), FileMode.Create), frameList);

            setStatusAction("Verification complete\nResult: ");
            return fileName;
        }

        public class Frame
        {
            public float[] TraitVector { get; set; }
            public int Cluster { get; set; }
        }
    }



}
