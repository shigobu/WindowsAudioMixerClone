using System.Windows;
using System.Windows.Controls;

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
