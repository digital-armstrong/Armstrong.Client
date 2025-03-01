using Armstrong.Client.Data;
using Armstrong.Client.Models;
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

namespace Armstrong.Client.Utilits
{
    public class ChartUtils : NotifyPropertyChanged
    {
        public int PointsLimit { get; private set; } = 100;
        public ObservableCollection<Channel> SelectedChannels { get; set; } = new();

        public Axis[] XAxis { get; set; } =
        {
            new Axis
            {
                Labeler = value => {
                    if (value < DateTime.MinValue.Ticks || value > DateTime.MaxValue.Ticks)
                        return string.Empty;

                    return new DateTime((long)value).ToString("HH:mm:ss");
                },
                LabelsPaint = new SolidColorPaint(SKColors.White),

                UnitWidth = TimeSpan.FromHours(1).Ticks,
                MinStep = TimeSpan.FromSeconds(1).Ticks
            }
        };
        public Axis[] YAxis { get; set; } =
        {
            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 0.3f),
                IsVisible = false,
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.White)
            },

            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 0.3f),
                IsVisible= false,
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite),
                Position = LiveChartsCore.Measure.AxisPosition.End
            },

            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 0.3f),
                IsVisible = false,
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.FloralWhite),
                Position = LiveChartsCore.Measure.AxisPosition.End
            },

            new Axis
            {
                SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 0.3f),
                IsVisible = false,
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.FloralWhite),
                Position = LiveChartsCore.Measure.AxisPosition.End
            }
        };

        public ChartUtils()
        {
            SelectedChannels = ChannelCollectionSingleton.GetInstance().SelectedChannelsCollection;
        }

        public ObservableCollection<Axis> GetXAxisCollection()
        {
            var axisCollection = new ObservableCollection<Axis>();

            foreach (var _x in XAxis)
            {
                axisCollection.Add(_x);
            }

            return axisCollection;
        }

        public ObservableCollection<Axis> GetYAxisCollection()
        {
            var axisCollection = new ObservableCollection<Axis>();

            foreach (var _y in YAxis)
            {
                axisCollection.Add(_y);
            }

            return axisCollection;
        }

        public ObservableCollection<ISeries> GetChartSeries(DateTime startDateTime, DateTime endDateTime)
        {
            var _series = new ObservableCollection<ISeries>();

            foreach (var _channel in SelectedChannels)
            {
                var scaleGroupIndex = _channel.DeviceType - 1;
                YAxis[scaleGroupIndex].IsVisible = true;

                _series.Add(new LineSeries<DateTimePoint>
                {
                    Name = _channel.ChannelName,
                    GeometrySize = 0.1,
                    Values = GetPointsCollection(_channel, startDateTime, endDateTime),
                    TooltipLabelFormatter = (chartPoint)
                        => $"Name: {_channel.ChannelName}{Environment.NewLine}" +
                        $"Device: {_channel.DeviceName}{Environment.NewLine}" +
                        $"Date: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yyyy HH:mm:ss}{Environment.NewLine}" +
                        $"Value: {chartPoint.PrimaryValue:E3}",
                    ScalesYAt = scaleGroupIndex,
                });
            }

            return _series;
        }

        private List<History> GetHistories(Channel channel, DateTime startDateTime, DateTime endDateTime)
        {
            using (var context = new DataContext())
            {
                var histories = context.Histories.AsNoTracking()
                                               .Where(x => x.Id == channel.Id)
                                               .Where(d => d.EventDate > startDateTime && d.EventDate < endDateTime)
                                               .OrderBy(x => x.EventDate)
                                               .ToList();
                return histories;
            }
        }

        private ObservableCollection<DateTimePoint> GetPointsCollection(Channel channel, DateTime startDateTime, DateTime endDateTime)
        {
            using (var context = new DataContext())
            {
                var _pointsCollection = new ObservableCollection<DateTimePoint>();
                var histories = GetHistories(channel, startDateTime, endDateTime);

                if (histories.Count > PointsLimit)
                {
                    var avgHistory = GetAvgHistory(histories);

                    foreach (var history in avgHistory)
                    {
                        _pointsCollection.Add(new DateTimePoint(history.EventDate, history.SystemEventValue));
                    }

                    return _pointsCollection;
                }
                else
                {
                    foreach (var history in histories)
                    {
                        _pointsCollection.Add(new DateTimePoint(history.EventDate, history.SystemEventValue));
                    }

                    return _pointsCollection;
                }
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
    }
}
