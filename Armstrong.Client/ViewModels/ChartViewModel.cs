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
    public class XAxisLimits : NotifyPropertyChanged
    {
        private double? _minLimit;

        public double? MinLimit
        {
            get { return _minLimit; }
            set { _minLimit = value; OnPropertyChanged(); }
        }

        private double? _maxLimit;

        public double? MaxLimit
        {
            get { return _maxLimit; }
            set { _maxLimit = value; OnPropertyChanged(); }
        }

        private DateTime? _minDateTime;

        public DateTime? MinDateTime
        {
            get => _minDateTime;
            set { _minDateTime = value; OnPropertyChanged(); }
        }

        private DateTime? _maxDateTime;

        public DateTime? MaxDateTime
        {
            get => _maxDateTime;
            set { _maxDateTime = value; OnPropertyChanged(); }
        }

        public void ConvertToDateTime()
        {
            if (MaxLimit is not null && MinLimit is not null)
            {
                MinDateTime = new DateTime((long)MinLimit);
                MaxDateTime = new DateTime((long)MaxLimit);
            }
        }
    }
    public class ChartViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ISeries> SeriesBindigCollection { get; set; }
        public ObservableCollection<Axis> XAxesBindingCollection { get; set; }
        public ObservableCollection<Axis> YAxesBindingCollection { get; set; }

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

        private XAxisLimits _xAxisLimits = new();

        public XAxisLimits XAxisLimits
        {
            get { return _xAxisLimits; }
            set { _xAxisLimits = value; OnPropertyChanged(); }
        }


        public ChartViewModel()
        {
            SeriesBindigCollection = ChartCollectionSingleton.GetInstance().SeriesCollection;
            XAxesBindingCollection = ChartCollectionSingleton.GetInstance().XAxesCollection;
            YAxesBindingCollection = ChartCollectionSingleton.GetInstance().YAxesCollection;
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

        public ICommand LoadAnyPoints
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                    var min = XAxesBindingCollection.SingleOrDefault().MinLimit;
                    var max = XAxesBindingCollection.SingleOrDefault().MaxLimit;

                    XAxisLimits.MinLimit = min;
                    XAxisLimits.MaxLimit = max;

                    XAxisLimits.ConvertToDateTime();
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
    }
}
