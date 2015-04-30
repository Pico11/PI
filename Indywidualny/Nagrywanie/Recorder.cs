using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nagrywanie.Annotations;
using NAudio.Wave;

namespace Nagrywanie
{
    public class Recorder:INotifyPropertyChanged
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
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public Recorder()
        {
            Status = string.Empty;

            PropertyChanged += Recorder_PropertyChanged;
        }

        void Recorder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(Status))
            {
                Status = "hello!";
            }
            //Status += "\no";
        }


        #region copyPaste
        public WaveIn waveSource = null;
        public WaveOut WaveOut = null;
        public WaveFileWriter WaveFileWriter = null;
        public WaveFileReader WaveFileReader = null;
        public void StartBtn_Click()
        {
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            WaveFileWriter = new WaveFileWriter(Path, waveSource.WaveFormat);

            waveSource.StartRecording();
            Status = "OK\nRecording";
        }

        public void StopBtn_Click()
        {
            waveSource.StopRecording();
            Status = "OK\nStopped";
            waveSource.Dispose();
        }

        public void PlayBtn_Click(string path)
        {
            WaveOut=new WaveOut();
            WaveFileReader = new WaveFileReader(path);
            WaveOut.Init(WaveFileReader);
            WaveOut.Play();
        }


        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (WaveFileWriter != null)
            {
                WaveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                WaveFileWriter.Flush();
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (WaveFileWriter != null)
            {
                WaveFileWriter.Dispose();
                WaveFileWriter = null;
            }
        } 
        #endregion

        #region mine
        //public void StartRecording()
        //{
        //    if (string.IsNullOrWhiteSpace(Path))
        //    {
        //        Status = "Path not specified"; 
        //        return;
        //    }
        //    StartRecording(Path);
        //}

        //public void StartRecording(string path)
        //{
        //    int deviceNumber = sourceList.SelectedItems[0].Index;

        //    sourceStream = new NAudio.Wave.WaveIn();
        //    sourceStream.DeviceNumber = deviceNumber;
        //    sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);

        //    NAudio.Wave.WaveInProvider waveIn = new NAudio.Wave.WaveInProvider(sourceStream);

        //    waveOut = new NAudio.Wave.DirectSoundOut();
        //    waveOut.Init(waveIn);

        //    sourceStream.StartRecording();
        //    waveOut.Play();

        //    recordButton.Visible = false;
        //    stopRecord.Visible = true;
        //    Status = "OK\nRecording";
        //}
        //public void StopAndSave(string path)
        //{

        //    Status = "OK\nSaved";
        //}

        //public void Play(string path)
        //{
        //    //Computer computer = new Computer();
        //    //computer.Audio.Play("c:\\record.wav", AudioPlayMode.Background);
        //    var player = new SoundPlayer(path);
        //    try
        //    {
        //        player.Load();
        //        player.Play();
        //        Status = "OK";
        //    }
        //    catch (ApplicationException e)
        //    {
        //        Status = "AppException:\n" + e.Message;
        //    }
        //    catch
        //    {
        //        Status = "Unknown Exception";
        //    }
        //} 
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
