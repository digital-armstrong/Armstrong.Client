using Armstrong.Client.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace Armstrong.Client.Utilits
{
    public class ChartCollectionSingleton
    {
        private static ChartCollectionSingleton _source;
        public ObservableCollection<ISeries> SeriesCollection;
        public ObservableCollection<Axis> XAxesCollection;
        public ObservableCollection<Axis> YAxesCollection;
        public ObservableCollection<Channel> SelectedChannelsCollection;

        private ChartCollectionSingleton()
        {
            SeriesCollection = new ObservableCollection<ISeries>();
            XAxesCollection = new ObservableCollection<Axis>();
            YAxesCollection = new ObservableCollection<Axis>();
            SelectedChannelsCollection = new ObservableCollection<Channel>();
        }

        public static ChartCollectionSingleton GetInstance()
        {
            if (_source == null)
            {
                _source = new ChartCollectionSingleton();
            }

            return _source;
        }
    }
}
