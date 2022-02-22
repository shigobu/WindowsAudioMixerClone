using NAudio.CoreAudioApi;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace AudioMixerClone
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // タイマのインスタンス
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            // タイマのインスタンスを生成
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer.Tick += _timer_Tick;
            timer.Start();

            using (MMDeviceEnumerator DevEnum = new MMDeviceEnumerator())
            using (MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                //マスターの設定
                masterSlider.displayNameTextBlock.Text = "マスター";
                masterSlider.slider.Value = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                masterSlider.IsMute = device.AudioEndpointVolume.Mute;
                using(Icon icon = GetIconFromIconPath(device.IconPath))
                {
                    if (icon != null)
                    {
                        masterSlider.mainImage.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, null);
                    }
                }

                //各アプリの追加
                SessionCollection sessionCollection = device.AudioSessionManager.Sessions;
                UpdateAppStackPanel(sessionCollection);
            }

        }

        private void UpdateAppStackPanel(SessionCollection sessionCollection)
        {
            //一度Sliderを削除
            appStackPanel.Children.Clear();

            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();

            for (int i = 0; i < sessionCollection.Count; i++)
            {
                AudioSessionControl audioSession = sessionCollection[i];
                SliderAndIcon sliderAndIcon = new SliderAndIcon();
                sliderAndIcon.SessionIndex = i;
                sliderAndIcon.SliderValueChanged += Slider_ValueChanged;
                sliderAndIcon.IsMute = audioSession.SimpleAudioVolume.Mute;

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

        private void _timer_Tick(object sender, EventArgs e)
        {
            //オーディオセッションを監視して、変更が有った場合に画面の更新
            using (MMDeviceEnumerator DevEnum = new MMDeviceEnumerator())
            using (MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                SessionCollection sessionCollection = device.AudioSessionManager.Sessions;
                if (sessionCollection.Count != appStackPanel.Children.Count)
                {
                    UpdateAppStackPanel(sessionCollection);
                }

                //各オーディオセッションの音量やミュート状態を更新
                foreach (SliderAndIcon sliderAndIcon in appStackPanel.Children)
                {
                    SimpleAudioVolume audioVolume = sessionCollection[sliderAndIcon.SessionIndex].SimpleAudioVolume;
                    if (!IsEqual(sliderAndIcon.slider.Value, audioVolume.Volume * 100))
                    {
                        sliderAndIcon.slider.Value = audioVolume.Volume * 100;
                    }

                    if (sliderAndIcon.IsMute != audioVolume.Mute)
                    {
                        sliderAndIcon.IsMute = audioVolume.Mute;
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

        private bool IsEqual(double value1, double value2)
        {
            double tol = 1e-5;
            return Math.Abs(value1 - value2) < tol;
        }
    }
}
