using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nagrywanie;
using PrzygotowanieDanych;

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

        public float[] GetRecordedSamples()
        {
            if (!AreSamplesReady()) return null;

            var samples = _recorder.Samples;
            _recorder = null;
            return samples;
        }


        public float[] ProcessSamples(float[] samples)
        {
            const float cutoffFrequency = 5000f;
            var filter = new Filter();
            var detector = new SilenceDetector();
            var filtered = filter.LowPass(samples, Recorder.Format.SampleRate, cutoffFrequency);
            var voice = detector.CutSilence(filtered, 256);

            return voice;
        }


    }


    
}
