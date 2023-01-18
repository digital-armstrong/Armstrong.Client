using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;
using Ookii.Dialogs.Wpf;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
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
                    var chart = obj as CartesianChart;
                    var skChart = new SKCartesianChart(chart) { Width = 1920, Height = 1080, };
                    skChart.Background = SkiaSharp.SKColor.Parse("#FF303030");

                    skChart.LegendPosition = LegendPosition.Top;
                    skChart.LegendTextPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColors.White);

                    VistaFolderBrowserDialog vistaFolderBrowserDialog = new();
                    vistaFolderBrowserDialog.ShowDialog();
                    var filePath = $"{vistaFolderBrowserDialog.SelectedPath}\\export.png";

                    skChart.SaveImage(path: filePath, quality: 100);
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
    }
}
