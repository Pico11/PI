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
using NAudioWpfDemo;

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
            DataContext = new SpectrumAnalyser(Polyline1.Points);
        }
    }
}
