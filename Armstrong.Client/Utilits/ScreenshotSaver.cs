using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;
using Ookii.Dialogs.Wpf;
using SkiaSharp;
using System;

namespace Armstrong.Client.Utilits
{
    public static class ScreenshotSaver
    {
        struct Quality
        {
            public static int Hight => 100;
            public static int Medium => 85;
            public static int Low => 50;
        }
        private static string FileName => $"Export_{DateTime.Now.Ticks}.png";

        public static void SavePng(CartesianChart chartObject)
        {
            var dir = GetFileDir();

            if (dir != string.Empty)
            {
                var path = $"{dir}\\{FileName}";
                var skChart = new SKCartesianChart(chartObject)
                {
                    Width = 1920,
                    Height = 1080,

                    LegendPosition = LegendPosition.Top,
                    LegendTextPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SKColors.Black)
                };

                skChart.SaveImage(path: path, quality: Quality.Hight);
            }
        }

        private static string GetFileDir()
        {
            VistaFolderBrowserDialog vistaFolderBrowserDialog = new();
            vistaFolderBrowserDialog.ShowDialog();

            return vistaFolderBrowserDialog.SelectedPath;
        }
    }
}
