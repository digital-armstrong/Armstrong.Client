using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class ChartViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<ISeries> _series;
        public ObservableCollection<ISeries> SeriesBindigCollection
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Axis> _xAxisBindingCollection;
        public ObservableCollection<Axis> XAxesBindingCollection
        {
            get => _xAxisBindingCollection;
            set
            {
                _xAxisBindingCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Axis> _yAxesBindingCollection;
        public ObservableCollection<Axis> YAxesBindingCollection
        {
            get => _yAxesBindingCollection;
            set
            {
                _yAxesBindingCollection = value;
                OnPropertyChanged();
            }
        }

        private TooltipPosition _toolTipPosition = TooltipPosition.Left;
        public TooltipPosition ToolTipPosition
        {
            get => _toolTipPosition;
            set
            {
                _toolTipPosition = value;
                OnPropertyChanged();
            }
        }

        public bool IsToolTipHiden { get; set; }

        public ChartViewModel()
        {
            ChartUtils chartUtils = new();

            SeriesBindigCollection = chartUtils.GetChartSeries(startDateTime: SelectedDateTimeRange.StartUtcDateTime,
                                                               endDateTime: SelectedDateTimeRange.EndUtcDateTime);
            XAxesBindingCollection = chartUtils.GetXAxisCollection();
            YAxesBindingCollection = chartUtils.GetYAxisCollection();
        }

        public ICommand CloseWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        Window chartViewWindow = obj as Window;
                        chartViewWindow.Close();
                    }
                });
            }
        }

        public ICommand MakeScreenshot
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        ScreenshotSaver.SavePng(chartObject: obj as CartesianChart);
                    }
                });
            }
        }

        public ICommand HideShowToolTip
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsToolTipHiden = !IsToolTipHiden;

                    switch (IsToolTipHiden)
                    {
                        case true:
                            ToolTipPosition = TooltipPosition.Hidden;
                            break;
                        case false:
                            ToolTipPosition = TooltipPosition.Left;
                            break;
                    }
                });
            }
        }

        public ICommand ResetZoom
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach (var x in XAxesBindingCollection)
                    {
                        x.MaxLimit = null;
                        x.MinLimit = null;
                    }

                    foreach (var y in YAxesBindingCollection)
                    {
                        y.MaxLimit = null;
                        y.MinLimit = null;
                    }
                });
            }
        }

        public ICommand LoadPoints
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        var chart = obj as CartesianChart;

                        var xAxis = chart.XAxes.SingleOrDefault();
                        var MinLimit = xAxis.MinLimit;
                        var MaxLimit = xAxis.MaxLimit;

                        if (MinLimit is not null && MaxLimit is not null)
                        {

                            SelectedDateTimeRange.StartUtcDateTime = new DateTime((long)xAxis.MinLimit).ToUniversalTime();
                            SelectedDateTimeRange.EndUtcDateTime = new DateTime((long)xAxis.MaxLimit).ToUniversalTime();

                            ChartUtils chartUtils = new();

                            var newSeries = chartUtils.GetChartSeries(startDateTime: SelectedDateTimeRange.StartUtcDateTime,
                                                               endDateTime: SelectedDateTimeRange.EndUtcDateTime);

                            foreach (var series in newSeries)
                            {
                                var value = SeriesBindigCollection.Where(x => x.Name == series.Name).FirstOrDefault();
                                value.Values = series.Values;
                            }

                            XAxesBindingCollection = chartUtils.GetXAxisCollection();
                            YAxesBindingCollection = chartUtils.GetYAxisCollection();
                        }
                    }
                });
            }
        }
    }
}
