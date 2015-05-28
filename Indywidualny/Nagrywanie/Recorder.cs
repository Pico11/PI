#define FloatSamples

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nagrywanie.Annotations;
using NAudio.SoundFont;
using NAudio.Wave;

namespace Nagrywanie
{
    public class Recorder : INotifyPropertyChanged
    {
        private string _status;
        //private NAudio.Wave.WaveIn source;
        //private NAudio.Wave.WaveFileWriter writer;
        //private NAudio.Wave.WaveRecorder recorder;

        //NAudio.Wave.WaveIn sourceStream = null;
        //NAudio.Wave.DirectSoundOut waveOut = null;
        //NAudio.Wave.WaveFileWriter waveWriter = null;
        public string Path { get; set; }
        public string Status
        {
            get { return _status; }
            private set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }



        public Recorder()
        {
            Status = string.Empty;
        }

        private WaveIn _waveSource;
        private WaveOut _waveOut;
        private WaveFileWriter _waveFileWriter;
        private WaveFileReader _waveFileReader;
        private MemoryStream _memoryStream;
        public static readonly WaveFormat WaveFormat = new WaveFormat(44100, 1);
#if FloatSamples
        private float[] _samples;
        private int _recordedBytes;
        private int _desiredBytes;

        public float[] Samples
        {
            get { return _samples; }
            private set
            {
                if (Equals(value, _samples)) return;
                _samples = value;
                OnPropertyChanged();
            }
        }
#endif

#if ShortSamples
        private short[] _samples;
        public short[] Samples
        {
            get { return _samples; }
            private set
            {
                if (Equals(value, _samples)) return;
                _samples = value;
                OnPropertyChanged();
            }
        } 
#endif

        private bool GetSamples { get; set; }

        public static WaveFormat Format
        {
            get { return WaveFormat; }
        }
        /// <summary>
        /// Start recording from default sound device 
        /// </summary>
        public void StartRecording()
        {
            _waveSource = new WaveIn { WaveFormat = Format, NumberOfBuffers = 5, BufferMilliseconds = 200 };
            _desiredBytes = int.MaxValue / 2;
            _waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_MemoryDataAvailable);
            _waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_MemoryRecordingStopped);
            _memoryStream = new MemoryStream();
            _waveFileWriter = new WaveFileWriter(_memoryStream, _waveSource.WaveFormat);
            // _memoryStream.Position = 0;
            _waveSource.StartRecording();
            Status = "OK\nRecording";
        }
        /// <summary>
        /// Start recording from default sound device 
        /// </summary>
        public void StartRecording(int sampleCount)
        {
            _waveSource = new WaveIn { WaveFormat = Format, NumberOfBuffers = 10, BufferMilliseconds = 500 };
            _desiredBytes = sampleCount * sizeof(short);
            _waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_MemoryDataAvailable);
            _waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_MemoryRecordingStopped);
            _memoryStream = new MemoryStream(_desiredBytes);
            _waveFileWriter = new WaveFileWriter(_memoryStream, _waveSource.WaveFormat);
            _memoryStream.Position = 0;
            _waveSource.StartRecording();
            Status = "OK\nRecording";
        }
        /// <summary>
        /// Start recording to given file
        /// </summary>
        /// <param name="file"></param>
        public void StartRecording(string file)
        {
            _waveSource = new WaveIn { WaveFormat = Format };

            _waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            _waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            _waveFileWriter = new WaveFileWriter(file, _waveSource.WaveFormat);

            _waveSource.StartRecording();
            Status = "OK\nRecording";
        }
        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            if (_waveSource == null) return;

            _waveSource.StopRecording();
            Status = "OK\nStopped";
            _waveSource.Dispose();
        }
        /// <summary>
        /// Stop recording and get recorded samples in an array
        /// </summary>
        /// <returns></returns>
        public void StopRecordingPrepareSamples()
        {
            if (_waveSource == null) return;
            GetSamples = true;
            try
            {
                _waveSource.StopRecording();
            }
            catch (NullReferenceException) { }

            Status = "OK\nStopped";

            _waveSource.Dispose();
        }
        /// <summary>
        /// Play an audio file
        /// </summary>
        /// <param name="path">path to .wav file</param>
        public void Play(string path)
        {
            _waveOut = new WaveOut();
            _waveFileReader = new WaveFileReader(path);
            _waveOut.Init(_waveFileReader);
            _waveOut.Play();
        }


        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveFileWriter == null) return;
            _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveFileWriter.Flush();
        }

        void waveSource_MemoryDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_memoryStream == null) return;
            lock (_memoryStream)
            {
                _memoryStream.Write(e.Buffer, 0, e.BytesRecorded);
                _recordedBytes += e.BytesRecorded;
                if (_recordedBytes >= _desiredBytes)
                {
                    StopRecordingPrepareSamples();
                    ForceGetSamples();
                }
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (_waveSource == null) return;
            try
            {
                _waveSource.Dispose();
                _waveSource = null;
            }
            catch (Exception)
            {
                _waveSource = null;
            }
            if (_waveFileWriter == null) return;
            try
            {
                _waveFileWriter.Flush();
                //if (GetSamples && _memoryStream != null && _memoryStream.CanRead)
                //{
                //    _memoryStream.Position = 0;
                //    //var reader = new WaveFileReader(_memoryStream);
                //    //var sampleList=new List<float>();
                //    //float[] frame;
                //    //while ((frame = reader.ReadNextSampleFrame()) != null && frame.Length > 0)
                //    //{
                //    //    sampleList.AddRange(frame);
                //    //}
                //    //Samples = sampleList.ToArray();
                //}
                _waveFileWriter.Dispose();
                _waveFileWriter = null;
            }
            catch (Exception)
            {
                _waveFileWriter = null;
            }
        }

        void waveSource_MemoryRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (_waveSource != null)
            {
                try
                {
                    _waveSource.Dispose();
                    _waveSource = null;
                }
                catch (Exception)
                {
                    _waveSource = null;
                }
            }
            if (_memoryStream != null)
            {
                try
                {
                    if (GetSamples && _memoryStream != null && _memoryStream.CanRead)
                    {
                        ForceGetSamples();
                    }
                    _memoryStream.Dispose();
                    _memoryStream = null;
                }
                catch (Exception)
                {
                    _waveFileWriter = null;
                }
            }
        }

        public void ForceGetSamples()
        {
            var src = _memoryStream.GetBuffer();

            const int fsize = sizeof(float);
            const int ssize = sizeof(short);
            //var samples = new float[src.Length * ssize / fsize];
            var samples = new List<float>((int)(_memoryStream.Length * ssize / fsize));
            int /*j = 0,*/ i;
            //var file = new FileStream(System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "rav"), FileMode.Create);
            //using (var writer = new BinaryWriter(file))
            {
                for (i = 0; i < src.Length - 1; i += ssize)
                {
                    var sample = (BitConverter.ToInt16(src, i));
                    samples.Add(sample / ((float)short.MaxValue));
                    //writer.Write(src[i]);
                    //writer.Write(src[i + 1]);

                }
            }

            Samples = samples.ToArray();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
