using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WorkshopTimer
{
    public delegate void NoParameterVoidDelegate();

    public class TimerEngine
    {
        DateTime _privateValue;
        DateTime _alertTreshold;
        public DateTime TimerStartValue
        {
            get { return _privateValue; }
            set
            {
                _privateValue = value;
                _alertTreshold = CalculateAlertTreshold(value);
                
            }
        }

        private DateTime AlertTreshold { get { return _alertTreshold; } }
        
        public DateTime TimerValue { get; set; }
        public DispatcherTimer MyDispatcherTimer { get; set; }
        public int TickCount { get; set; }
        public bool CountingDown { get; set; }
       
        public float Progress { get; set; }
        
        public event NoParameterVoidDelegate Tick;
        public event NoParameterVoidDelegate TimeIsUp;
        public event NoParameterVoidDelegate AlmostUpAlert;

        public TimerEngine()
        {
            MyDispatcherTimer = new DispatcherTimer();
            MyDispatcherTimer.Tick += dispatcherTimer_Tick;

            TickCount = 0;
        }

        private TimeSpan CalculateTimerTickSize()
        {
            var interval = new TimeSpan(0, 0, 0, 0, 100);

            if (TimerStartValue.Minute > 1)
            {
                interval = new TimeSpan(0, 0, 0, 0, 250);
            }

            if (TimerStartValue.Minute > 2)
            {
                interval = new TimeSpan(0, 0, 0, 0, 500);
            }

            if (TimerStartValue.Minute > 4 || TimerStartValue.Hour > 0 )
            {
                interval = new TimeSpan(0, 0, 0, 1);
            }

            return interval;
        }

        internal void Stop()
        {
            MyDispatcherTimer.Stop();
            CountingDown = false;
        }

        internal void Start()
        {
            if (MyDispatcherTimer.IsEnabled == true)
            {
                Stop();
            }
            
            TimerValue = TimerStartValue;            
            MyDispatcherTimer.Interval = CalculateTimerTickSize();
            MyDispatcherTimer.Start();
            IfTimeZeroThenStop();
            CountingDown = true;
        }
        
        internal void Reset()
        {
            Stop();
            Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            TickCount++;
            TimerValue = TimerValue.Add(-MyDispatcherTimer.Interval);
            Tick();
            IfTimeZeroThenStop();
            CheckIfTimeIsEndingAlert();
            
        }

        private void CheckIfTimeIsEndingAlert()
        {
            if (TimerValue == AlertTreshold)
            {
                AlmostUpAlert();
            }
        }

        private DateTime CalculateAlertTreshold(DateTime time)
        {
            var alertInterval = "00:00:30";
            if (time.Minute == 0 || time.Second > 30) alertInterval = "00:00:10";
            if (time.Minute > 9 || time.Hour > 0) alertInterval = "00:02:00";
            return DateTime.ParseExact(alertInterval, "hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private void IfTimeZeroThenStop()
        {
            if (TimerValue == DateTime.ParseExact("00:00:00", "hh:mm:ss", CultureInfo.InvariantCulture))
            {
                Stop();
                TimeIsUp();
            }
        }

        internal double TimrCalculatedNumberOfTicks()
        {
            return ((((TimerStartValue.Hour * 60 + TimerStartValue.Minute) * 60 + TimerStartValue.Second) * 1000) /  MyDispatcherTimer.Interval.TotalMilliseconds);
        }
      
    }
}
