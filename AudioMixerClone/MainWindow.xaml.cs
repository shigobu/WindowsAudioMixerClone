using NAudio.CoreAudioApi;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioMixerClone
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();

            using (MMDeviceEnumerator DevEnum = new MMDeviceEnumerator())
            using (MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                SessionCollection sessionCollection = device.AudioSessionManager.Sessions;
                for (int i = 0; i < sessionCollection.Count; i++)
                {
                    AudioSessionControl audioSession = sessionCollection[i];
                    SliderAndIcon sliderAndIcon = new SliderAndIcon();
                    if (audioSession.IsSystemSoundsSession)
                    {
                        sliderAndIcon.displayNameTextBlock.Text = "システム音";
                        appStackPanel.Children.Insert(0, sliderAndIcon);
                        continue;
                    }

                    string displayName = audioSession.DisplayName;
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = ps.First(x => x.Id == audioSession.GetProcessID).MainWindowTitle;
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = ps.First(x => x.Id == audioSession.GetProcessID).ProcessName;
                        }
                    }
                    sliderAndIcon.displayNameTextBlock.Text = displayName;

                    string exeName = ps.First(x => x.Id == audioSession.GetProcessID).MainModule.FileName;
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exeName);
                    sliderAndIcon.mainImage.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, null);


                    appStackPanel.Children.Add(sliderAndIcon);
                }
            }

        }
    }
}
