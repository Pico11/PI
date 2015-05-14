using System;
using System.Collections.Generic;
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

namespace Indywidualny
{
    /// <summary>
    /// Interaction logic for FrequenciesWindow.xaml
    /// </summary>
    public partial class FrequenciesWindow : Window
    {
        public FrequenciesWindow()
        {
            InitializeComponent();
        }
        public FrequenciesWindow(float[] frequencies)
        {
            InitializeComponent();
            ShowFrequencies(frequencies);
        }

        public void ShowFrequencies(float[] frequencies)
        {
            //var values = new double[samples.Length];
            //var max = samples.Select(v=>Math.Abs(v)).Max();
            //var equalized = samples.Select(v => v / max).ToArray();

            Grid.Children.Add(PolylineForSamples(frequencies));
        }
        private Polyline PolylineForSamples(float[] frequencies)
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
