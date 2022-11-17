using Armstrong.Client.Commands;
using Armstrong.Client.Data;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using Armstrong.Client.Views;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class DateTimePickerViewModel : NotifyPropertyChanged
    {
        private DateTime? _startDate = DateTime.Now.Date.AddDays(-1);
        private DateTime? _endDate = DateTime.Now.Date;
        private DateTime? _startTime = new();
        private DateTime? _endTime = new();

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
            SelectedChannels = ChartCollectionSingleton.GetInstance().SelectedChannelsCollection;
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

        public ICommand GetChart
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    var startTime = StartTime.Value.TimeOfDay;
                    var startDay = StartDate.Value.Date;
                    var startDateTime = (startDay + startTime).ToUniversalTime();

                    var endTime = EndTime.Value.TimeOfDay;
                    var endDay = EndDate.Value.Date;
                    var endDateTime = (endDay + endTime).ToUniversalTime();

                    Series = GetChartSeries(startDateTime, endDateTime);

                    FillingProps();

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


        public ObservableCollection<ISeries> SeriesBindingCollection { get; set; }
        public ObservableCollection<Axis> XAxesBindingCollection { get; set; }
        public ObservableCollection<Axis> YAxesBindingCollection { get; set; }

        public ObservableCollection<Channel> SelectedChannels { get; set; } = new();
        private ObservableCollection<ISeries> _series;

        public ObservableCollection<ISeries> Series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }
        public LineSeries<DateTimePoint> LineSeries { get; set; } =
            new LineSeries<DateTimePoint>
            {
                GeometrySize = 0.1,
                Values = new ObservableCollection<DateTimePoint>()
            };
        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => new DateTime((long) value).ToString("HH:mm:ss"),
                LabelsPaint = new SolidColorPaint(SKColors.White),

                UnitWidth = TimeSpan.FromHours(1).Ticks,
                MinStep = TimeSpan.FromSeconds(1).Ticks
            }
        };
        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.White)
            }
        };

        public ObservableCollection<ISeries> GetChartSeries(DateTime startDateTime, DateTime endDateTime)
        {
            var _series = new ObservableCollection<ISeries>();

            foreach (var _channel in SelectedChannels)
            {
                _series.Add(new LineSeries<DateTimePoint>
                {
                    GeometrySize = 0.1,
                    Values = GetPointsCollection(_channel, startDateTime, endDateTime),
                    TooltipLabelFormatter = (chartPoint)
                        => $"Name: {_channel.ChannelName}\nDate: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yyyy HH:mm:ss}\nValue: {chartPoint.PrimaryValue:E3}",
                });
            }

            return _series;
        }

        private ObservableCollection<DateTimePoint> GetPointsCollection(Channel channel, DateTime startDateTime, DateTime endDateTime)
        {
            using (var context = new DataContext())
            {
                var _pointsLimit = 100;
                var _pointsCollection = new ObservableCollection<DateTimePoint>();

                var histories = context.Histories.AsNoTracking()
                                               .Where(x => x.Id == channel.Id)
                                               .Where(d => d.EventDate > startDateTime && d.EventDate < endDateTime)
                                               .OrderBy(x => x.EventDate)
                                               .ToList();

                if (histories.Count > _pointsLimit)
                {
                    var avgHistory = GetAvgHistory(histories);
                    foreach (var history in avgHistory)
                    {
                        _pointsCollection.Add(new DateTimePoint(history.EventDate, history.SystemEventValue));
                    }

                    return _pointsCollection;
                }

                return _pointsCollection;
            }
        }

        private static List<History> GetAvgHistory(List<History> histories)
        {
            int seporator = histories.Count / 100;
            List<History> tempHistories = new();
            List<History> avgHistory = new();

            foreach (var hist in histories)
            {
                if (seporator >= tempHistories.Count)
                {
                    tempHistories.Add(new() { SystemEventValue = hist.SystemEventValue, EventDate = hist.EventDate });
                }
                else
                {
                    avgHistory.Add(new History()
                    {
                        SystemEventValue = tempHistories.Select(x => x.SystemEventValue).Average(),
                        EventDate = GetAvgDateTime(tempHistories.Select(x => x.EventDate).ToList())
                    });

                    tempHistories.Clear();
                }
            }

            return avgHistory;
        }

        private static DateTime GetAvgDateTime(List<DateTime> dates)
        {
            var count = dates.Count;
            double temp = 0D;

            for (int i = 0; i < count; i++)
            {
                temp += dates[i].Ticks / (double)count;
            }

            return new DateTime((long)temp);
        }

        private void FillingProps()
        {
            SeriesBindingCollection = ChartCollectionSingleton.GetInstance().SeriesCollection;
            XAxesBindingCollection = ChartCollectionSingleton.GetInstance().XAxesCollection;
            YAxesBindingCollection = ChartCollectionSingleton.GetInstance().YAxesCollection;

            SeriesBindingCollection.Clear();
            XAxesBindingCollection.Clear();
            YAxesBindingCollection.Clear();

            foreach (var _series in Series)
            {
                SeriesBindingCollection.Add(_series);
            }

            foreach (var _xAxes in XAxes)
            {
                XAxesBindingCollection.Add(_xAxes);
            }

            foreach (var _yAxes in YAxes)
            {
                YAxesBindingCollection.Add(_yAxes);
            }
        }
    }
}
