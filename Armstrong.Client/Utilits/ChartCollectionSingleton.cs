using Armstrong.Client.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace Armstrong.Client.Utilits
{
    public class ChartCollectionSingleton : NotifyPropertyChanged
    {
        private static ChartCollectionSingleton _source;
        public ObservableCollection<ISeries> SeriesCollection;
        public ObservableCollection<Axis> XAxesCollection;
        public ObservableCollection<Axis> YAxesCollection;

        private ChartCollectionSingleton()
        {
            SeriesCollection = new ObservableCollection<ISeries>();
            XAxesCollection = new ObservableCollection<Axis>();
            YAxesCollection = new ObservableCollection<Axis>();
        }

        public static ChartCollectionSingleton GetInstance()
        {
            _source ??= new ChartCollectionSingleton();

            return _source;
        }
    }
}
