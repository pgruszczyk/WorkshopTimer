using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Media;
using System.Resources;
using System.Reflection;

namespace WorkshopTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimerEngine Timer { get; set; }
        readonly string TimeZeroed = "0:00";

        public MainWindow()
        {
            SoundOn = true;
            InitializeComponent();
            InitControlScreenSaver();
            Timer = new TimerEngine();
            Timer.Tick += DisplayTime;
            Timer.AlmostUpAlert += AlmostEndScreen;
            Timer.TimeIsUp += PlayTimeIsUpSound;
            Timer.TimeIsUp += ResetScreen;
            

            Timer.Tick += Progressing;
            TimerScreen.Content = TimeZeroed;

            
        }
        
        #region ScreenSaver control
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private void InitControlScreenSaver()
        {
            this.StateChanged += new EventHandler((sender, e) =>
            {
                if (WindowState == System.Windows.WindowState.Maximized)
                {
                    SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                }
                else
                {
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                }
            });
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        #endregion

        #region Menu Buttons

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            FireUpTimer();
        }

        private void FireUpTimer()
        {
            var time = TimeSet.Text;
            try
            {
                ParseTime(time);

                if (TimerSetToZero() == true)
                {
                    return;
                }
                Timer.Start();
                ResetScreen();
                ShowCountDown();
                DisplayTime();
            }
            catch
            {
                TimeSet.Focus();
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            ResetScreen();
        }

        private void ParseTime(string time)
        {
            var timeSegments = time.Split(':');
            var prefix = string.Empty;
            for (int i = timeSegments.Length; i < 3; i++)
            {
                prefix += "00:";
            }

            Timer.TimerStartValue = DateTime.Parse(prefix + time, CultureInfo.InvariantCulture);
        }

        private void FullScr_Click(object sender, RoutedEventArgs e)
        {
            var state = WindowState.Normal;
            var style = WindowStyle.ToolWindow;
            
            if (this.WindowState != WindowState.Maximized)
            {
                state = WindowState.Maximized;
                style = WindowStyle.None;
            }

            this.WindowState = state;
            this.WindowStyle = style;
            
        }

        private void Presets_Click(object sender, RoutedEventArgs e)
        {

            if (Presets.Visibility == System.Windows.Visibility.Collapsed)
            {
                Presets.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Presets.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SetTo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int minutes = 0;
            var parsed = Int32.TryParse(button.Content.ToString().Replace("m", "").Trim(), out minutes);

            if (parsed == false)
            {
                e.Handled = true;
                return;
            }

            TimeSet.Text = minutes + ":00";

            StartBtn_Click(sender, e);
            Presets.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        #region Scaling Window

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var currentWidth = Application.Current.MainWindow.ActualWidth;
            var currentHeight = Application.Current.MainWindow.ActualHeight;
            TimerScreen.FontSize = currentWidth / 6;
            TimeSet.FontSize = currentWidth / 6;

            ProgressScreen.Width = currentWidth * 0.7;
            ProgressScreen.Height = currentHeight * 0.2;
            Thickness margin = ProgressScreen.Margin;
            margin.Top = currentHeight * 0.1;
            ProgressScreen.Margin = margin;
        }

        #endregion

        #region Status Indicators

        public bool SoundOn { get; set; }

        private void SoundBtn_Click(object sender, RoutedEventArgs e)
        {
            SoundOn = !SoundOn;
            if (SoundOn)
            {
                SoundBtnOff.Visibility = Visibility.Collapsed;
                SoundBtn.Visibility = Visibility.Visible;            }
            else
            {
                SoundBtn.Visibility = Visibility.Collapsed;
                SoundBtnOff.Visibility = Visibility.Visible;
            }
        }

        private void ResetScreen()
        {
            ShowTimeSet();
            Background = Brushes.Black;

            ProgressScreen.Background = Brushes.DarkGray;
            ProgressScreen.Maximum = Timer.TimrCalculatedNumberOfTicks();
            ProgressScreen.Value = 0;
        }

        private void DisplayTime()
        {
            ShowCountDown();

            var toDisplay = Timer.TimerValue.ToString("HH:mm:ss");

            if (Timer.TimerValue.Hour == 0)
            {
                if (Timer.TimerValue.Minute == 0)
                {
                    toDisplay = Timer.TimerValue.ToString("ss");
                }
                else toDisplay = Timer.TimerValue.ToString("mm:ss");
            }

            TimerScreen.Content = toDisplay;
        }

       
        private void AlmostEndScreen()
        {
            ProgressScreen.Background = Brushes.Black ;
            Background = Brushes.DarkRed;
        }

        private void Progressing()
        {
            ProgressScreen.Value = ProgressScreen.Value + 1;

        }

        private void PlayTimeIsUpSound()
        {
            if (SoundOn)
            {
                var audio = new SoundPlayer(Properties.Resources.Alarm);
                audio.Play();
            }
        }
        #endregion

        #region Timesetting
        
        private void ShowCountDown()
        {
            TimerScreen.Visibility = Visibility.Visible;
            TimeSet.Visibility = Visibility.Collapsed;
            FocusCatcher.Visibility = Visibility.Collapsed;
        }

        private void ShowTimeSet()
        {
            TimerScreen.Visibility = Visibility.Collapsed;
            TimeSet.Visibility = Visibility.Visible;
            FocusCatcher.Visibility = Visibility.Visible;
            TimeSet.Focus();
        }

        private void TimeSet_CheckTimeValue(object sender, RoutedEventArgs e)
        {
            try
            {
                var time = DateTime.Parse(TimeSet.Text, CultureInfo.InvariantCulture);
            }
            catch
            {
                TimeSet.Focus();
            }
        }

        private void TimeSet_KeyDown(object sender, KeyEventArgs e)
        {
            TimeSet.CaretIndex = TimeSet.Text.Length;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                FireUpTimer();
                return;
            }

            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >=Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (TimeSet.Text.Length == 8) return;
                SetTimeSegmentsInTextbox();
            }

            if (e.Key >= Key.A && e.Key <= Key.Z || e.Key == Key.Decimal)
            {
                e.Handled = true;
                return;
            }

            
        }

        private void SetTimeSegmentsInTextbox()
        {
            var time = TimeSet.Text.Replace(":", "");

            if (time.Length - 4 > 0)
            {
                time = time.Insert(time.Length - 4, ":");
            }

            if (time.Length - 2 > 0)
            {
                time = time.Insert(time.Length - 2, ":");
            }

            TimeSet.Text = time;
            MoveCaretToAnEnd();
        }

        private void MoveCaretToAnEnd()
        {
            TimeSet.CaretIndex = TimeSet.Text.Length;
        }

        


        private void TimeSet_GotFocus(object sender, RoutedEventArgs e)
        {
           
            MoveCaretToAnEnd();
            if (TimerSetToZero() == true)
            {
                TimeSet.Text = string.Empty;
            }
            TimeSet.BorderThickness = new Thickness(0, 0, 0, 1);
            
        }

        private bool TimerSetToZero()
        {
            var time = TimeSet.Text.Replace(":", "");
            int timenumeric = 0;
            Int32.TryParse(time, out timenumeric);

            if (timenumeric == 0)
            {
                return true;
            }
            return false;
        }

        private void TimeSet_LostFocus(object sender, RoutedEventArgs e)
        {
            TimeSet.BorderThickness = new Thickness(0);
        }

        private void TimeSet_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetTimeSegmentsInTextbox();
        }


        private void FocusCatcher_GotFocus(object sender, MouseButtonEventArgs e)
        {
            TimeSet.Focus();
        }

        #endregion

       

        

    }

}