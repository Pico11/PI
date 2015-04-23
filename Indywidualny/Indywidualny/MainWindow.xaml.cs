using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Nagrywanie;
using Nagrywanie.Annotations;

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
            set { if (value == _recording)return; _recording = value; OnPropertyChanged("Recording"); OnPropertyChanged("NotRecording"); }
        }
        public float Frequency
        {
            get { return _frequency; }
            set { if (value == _frequency)return; _frequency = value; OnPropertyChanged("Frequency"); }
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
            _recorder.StartBtn_Click();
        }

        private void StopSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentFile)) return;

            _recorder.StopBtn_Click();
            NotRecording = true;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!_openFileDialog.ShowDialog().GetValueOrDefault(false)) return;
            _recorder.PlayBtn_Click(_openFileDialog.FileName);
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

        private void TestFilter_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var tester = new FilterTester();
            var dir = fbd.SelectedPath;
            tester.Convert(dir);
            //foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.wav"))
            //{
            //    for (int i = 1; i <= 10; i++)
            //    {
            //        var newFile = file.Remove(file.IndexOf(".wav")) + "_" + i.ToString();
            //        _filter.LowPass(file, newFile, i * 1000);
            //    }
            //}
        }
    }
}
