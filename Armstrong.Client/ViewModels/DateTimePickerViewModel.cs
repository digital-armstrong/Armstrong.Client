using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using Armstrong.Client.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class DateTimePickerViewModel : NotifyPropertyChanged
    {
        private DateTime? _startDate = DateTime.Now.Date.AddDays(-1);
        private DateTime? _endDate = DateTime.Now.Date;
        private DateTime? _startTime = DateTime.Now;
        private DateTime? _endTime = DateTime.Now;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; OnPropertyChanged(); }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; OnPropertyChanged(); }
        }

        public DateTime? StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(); }
        }

        public DateTime? EndTime
        {
            get { return _endTime; }
            set { _endTime = value; OnPropertyChanged(); }
        }

        public DateTimePickerViewModel()
        {
        }

        public ICommand CloseWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        Window datePickerWindow = obj as Window;
                        datePickerWindow.Close();
                    }
                });
            }
        }

        public ICommand GetTimeRange
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    var startTime = StartTime.Value.TimeOfDay;
                    var startDay = StartDate.Value.Date;
                    var endTime = EndTime.Value.TimeOfDay;
                    var endDay = EndDate.Value.Date;

                    SelectedDateTimeRange.StartUtcDateTime = (startDay + startTime).ToUniversalTime();
                    SelectedDateTimeRange.EndUtcDateTime = (endDay + endTime).ToUniversalTime();

                    ChartView chartView = new();
                    chartView.Show();


                    if (obj is not null)
                    {
                        Window datePickerWindow = obj as Window;
                        datePickerWindow.Close();
                    }
                });
            }
        }
    }
}
