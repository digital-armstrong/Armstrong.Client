using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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
    }
}
