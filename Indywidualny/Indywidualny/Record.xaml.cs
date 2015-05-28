#define FloatSamples

#if !FloatSamples
#define ShortSamples
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ExtractionModule;
using Nagrywanie;
using NAudio.Wave;
using PrzygotowanieDanych;

namespace Indywidualny
{
    /// <summary>
    /// Interaction logic for Authenticate.xaml
    /// </summary>
    public partial class Record : Window
    {
        private const string GeneralError = "An error occured";
        enum SystemState
        {
            Error = 0,
            Idle,
            Busy,
            AwaitingInput,
            Results
        }
        private SystemState _state = SystemState.Idle;

        readonly Recorder _recorder = new Recorder();
        private readonly string _tempDir = Path.GetTempPath();
        private readonly Timer _timer;
        private const int RecordingTime = 5 * 1000;

        public Record()
        {
            InitializeComponent();
            _timer = new Timer() { AutoReset = false, Interval = RecordingTime };
            _timer.Elapsed += _timer_Elapsed;
        }

#if FloatSamples
        private float[] _speakerData;
#endif

#if ShortSamples
        private short[] _speakerData;

#endif
        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    _state = SystemState.Idle;
                    _recorder.StopRecordingPrepareSamples();
                    _recorder.PropertyChanged += Recorder_SamplesReady;//subscribe to get samples
                    SetMessage("Recording is being processed");
                    Button.IsEnabled = true;
                });
            }
            catch (Exception)
            {
                _state = SystemState.Error;
                return;
            }
        }

        void Recorder_SamplesReady(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Samples")) return;
            _speakerData = _recorder.Samples;//get samples
            _recorder.PropertyChanged -= Recorder_SamplesReady;//unsubscribe event
        }

        private float[] GetSamples(Stream stream)
        {
            var reader = new WaveFileReader(stream);
            var samples = new List<float>();
            float[] frame;
            while ((frame = reader.ReadNextSampleFrame()) != null && frame.Length > 0)
            {
                samples.AddRange(frame);
            }
            return samples.ToArray();
        }

        //static void RecordTimeout(object stateObject)
        //{
        //    var auth = stateObject as Record;
        //    if (auth == null) return;
        //    try
        //    {
        //        auth.Dispatcher.Invoke(() =>
        //        {
        //            auth._recorder.StopRecording();
        //            auth._state = SystemState.Idle;
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        auth._state = SystemState.Error;
        //        return;
        //    }
        //    PerformFiltering(auth._recorder.Path);
        //    auth.Button.IsEnabled = true;
        //}
        public void TryLogin(string username)
        {
            UserCommand.Text = string.Empty;
            //try
            {
                var extractor = new Extractor();
                var traitsFile = extractor.CreateUserTraitsFile(username);


                UserCommand.Text = "User data collected at " + traitsFile;
            }
            //catch (Exception e)
            //{
            //    UserCommand.Text = "Errors occured "+e;
            //}
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username.Text))
            {
                UserCommand.Text = "Incorrect login"; return;
            }
            TryLogin(Username.Text);
        }
        //private void Button_OnClick(object sender, RoutedEventArgs e)
        //{
        //    switch (_state)
        //    {
        //        case SystemState.Busy:
        //            return;
        //        case SystemState.Error:
        //            if (GeneralError != UserCommand.Text)
        //                SetMessage();
        //            else
        //            {
        //                UserCommand.Text = string.Empty;
        //                _state = SystemState.Idle;
        //            }
        //            return;
        //        case SystemState.Results:
        //        case SystemState.Idle:
        //        case SystemState.AwaitingInput:
        //            if (string.IsNullOrWhiteSpace(Username.Text))
        //            { SetMessage(Properties.Resources.Record_Invalid_user_name); _state = SystemState.AwaitingInput; return; }
        //            _state = SystemState.Busy;

        //            Button.IsEnabled = false;

        //            var path = Path.ChangeExtension(Path.GetRandomFileName(), "wav");
        //            path = Path.Combine(_tempDir, path);
        //            _recorder.Path = path;
        //            try
        //            {
        //                _recorder.StartRecording();
        //                SetMessage(Properties.Resources.Record_RecordingInProgress);
        //                //_timer.Change(RecordingTime, Timeout.Infinite);
        //                _timer.Start();
        //            }
        //            catch (Exception)
        //            {
        //                SetMessage(Properties.Resources.Record_RecordingError);
        //            }
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}



        private static string PerformFiltering(string path)
        {
            var filter = new Filter();
            var file = Path.GetFileName(path);
            var dir = Path.GetDirectoryName(path);
            var filteredPath = Path.Combine(dir, file.Insert(0, "_"));
            filter.LowPass(path, filteredPath, 5000);

            try
            {
                File.Delete(path);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return filteredPath;

        }
        private static float[] PerformFiltering(float[] samples, int sampleRate)
        {
            var filter = new Filter();
            return filter.LowPass(samples, sampleRate, 5000);
        }
        private void SetMessage(string message = GeneralError)
        {
            UserCommand.Text = message;
        }
    }
}
