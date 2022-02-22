using NAudio.CoreAudioApi;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

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
                //マスターの設定
                masterSlider.displayNameTextBlock.Text = "マスター";
                masterSlider.slider.Value = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                using(Icon icon = GetIconFromIconPath(device.IconPath))
                {
                    if (icon != null)
                    {
                        masterSlider.mainImage.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, null);
                    }
                }

                //各アプリの追加
                SessionCollection sessionCollection = device.AudioSessionManager.Sessions;
                for (int i = 0; i < sessionCollection.Count; i++)
                {
                    AudioSessionControl audioSession = sessionCollection[i];
                    SliderAndIcon sliderAndIcon = new SliderAndIcon();
                    sliderAndIcon.SessionIndex = i;
                    sliderAndIcon.SliderValueChanged += Slider_ValueChanged;

                    if (audioSession.IsSystemSoundsSession)
                    {
                        sliderAndIcon.displayNameTextBlock.Text = "システム音";
                        string iconpath = audioSession.IconPath;
                        using (Icon icon = GetIconFromIconPath(iconpath))
                        {
                            if (icon == null)
                            {
                                continue;
                            }
                            sliderAndIcon.mainImage.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, null);
                        }
                        sliderAndIcon.slider.Value = audioSession.SimpleAudioVolume.Volume * 100;
                        appStackPanel.Children.Insert(0, sliderAndIcon); //システム音声は、アプリの最初に追加
                    }
                    else
                    {
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
                        using (Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exeName))
                        {
                            sliderAndIcon.mainImage.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, null);
                        }
                        sliderAndIcon.slider.Value = audioSession.SimpleAudioVolume.Volume * 100;
                        appStackPanel.Children.Add(sliderAndIcon);
                    }
                }
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            using (MMDeviceEnumerator DevEnum = new MMDeviceEnumerator())
            using (MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                SessionCollection sessionCollection = device.AudioSessionManager.Sessions;
                sessionCollection[((SliderAndIcon)sender).SessionIndex].SimpleAudioVolume.Volume = (float)e.NewValue / 100;
            }

        }

        /// <summary>
        /// wasapiのIconPathからアイコンを取得します。
        /// </summary>
        /// <param name="iconPath"></param>
        /// <returns></returns>
        private Icon GetIconFromIconPath(string iconPath)
        {
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                return null;
            }
            if (System.IO.File.Exists(iconPath))
            {
                return System.Drawing.Icon.ExtractAssociatedIcon(iconPath);
            }
            else
            {
                string[] sprintedPath = iconPath.Split(',');
                string path = Environment.ExpandEnvironmentVariables(sprintedPath[0]).Replace("@", "");
                Icon[] tempIcons = Native.ExtractIconEx(path, int.Parse(sprintedPath[1]), 1);
                if (tempIcons.Length == 0)
                {
                    return null;
                }
                else
                {
                    return tempIcons[0];
                }
            }
        }
    }
}
