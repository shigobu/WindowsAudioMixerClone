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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioMixerClone
{
    /// <summary>
    /// SliderAndIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class SliderAndIcon : UserControl
    {
        public SliderAndIcon()
        {
            InitializeComponent();

            slider.ValueChanged += (sender, e) => { SliderValueChanged?.Invoke(this, e); };
        }

        public int SessionIndex { get; set; }

        public event RoutedPropertyChangedEventHandler<double> SliderValueChanged;
    }
}
