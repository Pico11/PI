using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Indywidualny
{
    /// <summary>
    /// Interaction logic for ShowVectors.xaml
    /// </summary>
    public partial class ShowVectors : Window
    {
        public ShowVectors()
        {
            InitializeComponent();
        }
        readonly OpenFileDialog _openFileDialog = new OpenFileDialog { Filter = "XML document|*.xml", DefaultExt = ".xml", Multiselect = false };
        private void Freq_Click(object sender, RoutedEventArgs e)
        {
            if (!_openFileDialog.ShowDialog().GetValueOrDefault()) return;
            var serializer = new XmlSerializer(typeof(List<ExtractionModule.Extractor.Frame>));
            var list = (List<ExtractionModule.Extractor.Frame>)serializer.Deserialize(new FileStream(_openFileDialog.FileName, FileMode.Open, FileAccess.Read));
            var clusters = list.Select(f => f.Cluster).ToArray();
            foreach (var item in Grid.Children.OfType<Polyline>())
            {
                Grid.Children.Remove(item);
            }

            ShowClusters(clusters);
        }


        public void ShowClusters(int[] frequencies)
        {
            //var values = new double[samples.Length];
            //var max = samples.Select(v=>Math.Abs(v)).Max();
            //var equalized = samples.Select(v => v / max).ToArray();

            Grid.Children.Add(PolylineForSamples(frequencies));
        }
        private Polyline PolylineForSamples(int[] frequencies)
        {
            var line = new Polyline
            {
                Stroke = new SolidColorBrush(GetColor()),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //MinHeight = 100,
                //MinWidth = 200,
                Stretch = Stretch.Fill
            };
            for (int i = 0; i < frequencies.Length; i++)
            {
                line.Points.Add(new Point(i, -frequencies[i]));
            }
            return line;
        }

        private static readonly Color[] Colors =
        {
            System.Windows.Media.Colors.Aqua, System.Windows.Media.Colors.Blue, System.Windows.Media.Colors.BlueViolet, System.Windows.Media.Colors.Brown, System.Windows.Media.Colors.CadetBlue,
            System.Windows.Media.Colors.Coral, System.Windows.Media.Colors.CornflowerBlue, System.Windows.Media.Colors.Crimson, System.Windows.Media.Colors.Cyan, System.Windows.Media.Colors.DarkCyan, System.Windows.Media.Colors.DarkGreen,
            System.Windows.Media.Colors.ForestGreen,
        };

        private int _currentColor;
        private static readonly Random Random = new Random();
        private Color GetColor()
        {
            if (_currentColor < Colors.Length)
                return Colors[_currentColor++];
            var bytes = new byte[3];
            Random.NextBytes(bytes);
            return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
        }
    }
}
