using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Dsp;

namespace NAudioWpfDemo
{
    /// <summary>
    /// Interaction logic for SpectrumAnalyser.xaml
    /// </summary>
    public class SpectrumAnalyser:INotifyPropertyChanged
    {
        private double xScale = 200;

        private const int binsPerPoint = 2; // reduce the number of points we plot for a less jagged line?
        private int bins = SampleAggregator.FftLength/binsPerPoint; // FFT size, bins is half FFT size //symetry otherwise
        
        
        //private int updateCount;

        private int _height;
        private int _width;
        public int Width
        {
            get { return _width; }
            set { if(_width==value)return; _width = value; OnPropertyChanged();}
        }

        public int Height
        {
            get { return _height; }
            set { if(_height==value)return;_height = value; OnPropertyChanged();}
        }

        public SpectrumAnalyser()
        {
            polylinePoints=new List<Point>(bins);
            CalculateXScale();
            this.PropertyChanged += SpectrumAnalyser_SizeChanged;
        }
        public SpectrumAnalyser(IList<Point> polylinePoints)
        {
            this.polylinePoints = polylinePoints;
            CalculateXScale();
            this.PropertyChanged += SpectrumAnalyser_SizeChanged;
        }



        void SpectrumAnalyser_SizeChanged(object sender, PropertyChangedEventArgs e)
        {
            CalculateXScale();
        }

        private void CalculateXScale()
        {
            this.xScale = this.Width / (bins / binsPerPoint);
        }



        public void Update(Complex[] fftResults)
        {
            // no need to repaint too many frames per second
            //if (updateCount++ % 2 == 0)
            //{
            //    return;
            //}

            if (fftResults.Length / binsPerPoint != bins)
            {
                this.bins = fftResults.Length / binsPerPoint;
                CalculateXScale();
            }

            for (int n = 0; n < fftResults.Length / binsPerPoint; n += binsPerPoint)
            {
                // averaging out bins
                double yPos = 0;
                for (int b = 0; b < binsPerPoint; b++)
                {
                    yPos += GetYPosLog(fftResults[n + b]);
                }
                AddResult(n / binsPerPoint, yPos / binsPerPoint);
            }
        }

        private double GetYPosLog(Complex c)
        {
            // not entirely sure whether the multiplier should be 10 or 20 in this case.
            // going with 10 from here http://stackoverflow.com/a/10636698/7532
            double intensityDB = 10 * Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));
            double minDB = -90;
            if (intensityDB < minDB) intensityDB = minDB;
            double percent = intensityDB / minDB;
            // we want 0dB to be at the top (i.e. yPos = 0)
            double yPos = percent * this.Height;
            return yPos;
        }

        readonly IList<Point> polylinePoints;
        public IList<Point> Points { get { return polylinePoints; } }
        private void AddResult(int index, double power)
        {
            var p = new Point(CalculateXPos(index), power);
            if (index >= polylinePoints.Count)
            {
                polylinePoints.Add(p);
            }
            else
            {
                polylinePoints[index] = p;
            }
        }


        private double CalculateXPos(int bin)
        {
            if (bin == 0) return 0;
            return bin * xScale; // Math.Log10(bin) * xScale;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}