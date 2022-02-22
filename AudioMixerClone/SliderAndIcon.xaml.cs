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

        private static string muteChar = "🔇";
        private static string notMuteChar = "🔊";

        public int SessionIndex { get; set; }

        public string ButtonText
        {
            get
            {
                return buttonTextBlock.Text;
            }
            set
            {
                buttonTextBlock.Text = value;
            }
        }

        public bool IsMute
        {
            get
            {
                return ButtonText == muteChar;
            }
            set
            {
                ButtonText = value ? muteChar : notMuteChar;
            }
        }

        public event RoutedPropertyChangedEventHandler<double> SliderValueChanged;
    }
}
