using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Nagrywanie;
using Nagrywanie.Annotations;
using NAudio.Wave;
using PrzygotowanieDanych;
using Spektrum;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Indywidualny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        readonly Recorder _recorder;
        readonly Filter _filter;
        readonly OpenFileDialog _openFileDialog = new OpenFileDialog { Filter = "Wave sound|*.wav", DefaultExt = ".wav", Multiselect = false };
        readonly SaveFileDialog _saveFileDialog = new SaveFileDialog { AddExtension = true, DefaultExt = ".wav", Filter = "Wave sound|*.wav" };
        private string _currentFile;
        private bool _recording;
        private float _frequency = 5000f;

        public bool NotRecording
        {
            get { return !_recording; }
            set
            {
                if (value != _recording) return;
                Recording = !value;
            }
        }
        public bool Recording
        {
            get { return _recording; }
            set { if (value == _recording)return; _recording = value; OnPropertyChanged(); OnPropertyChanged("NotRecording"); }
        }
        public float Frequency
        {
            get { return _frequency; }
            set { if (value == _frequency)return; _frequency = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public MainWindow()
        {
            InitializeComponent();
            _recorder = new Recorder();
            _filter = new Filter();

            NotRecording = true;

            _recorder.PropertyChanged += _recorder_PropertyChanged;
            //this.DataContext = this;
        }

        void _recorder_PropertyChanged(object sender, EventArgs e)
        {
            Status = _recorder.Status;
            StatusText.Text = Status;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!_saveFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            _currentFile = _saveFileDialog.FileName;
            _recorder.Path = _currentFile;
            Recording = true;
            _recorder.StartRecording();
        }

        private void StopSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentFile)) return;

            _recorder.StopRecording();
            NotRecording = true;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!_openFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            _recorder.Play(_openFileDialog.FileName);
        }

        private void Rst_status(object sender, RoutedEventArgs e)
        {
            Status = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (!_openFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            if (!_saveFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            _filter.LowPass(_openFileDialog.FileName, _saveFileDialog.FileName, Frequency);
        }


        readonly FourierTransformer _transformer=new FourierTransformer();
        private void Freq_Click(object sender, RoutedEventArgs e)
        {
            const int transformSize=1024;
            if (!_openFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            var waveFile = new WaveFileReader(_openFileDialog.FileName);
            var freqW = new FrequenciesWindow();
            freqW.Show();
            var frame = new List<float>(transformSize);
            while (waveFile.CurrentTime < waveFile.TotalTime)
            {
                frame.AddRange(waveFile.ReadNextSampleFrame());
                if (frame.Count >= transformSize)
                {
                    var frequencies = _transformer.ApplyDFT(frame.ToArray());
                    freqW.ShowFrequencies(frequencies);
                    frame.Clear();
                }
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow=new Record();
            loginWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vectorWindow = new ShowVectors();
            vectorWindow.Show();
        }

    }
}
