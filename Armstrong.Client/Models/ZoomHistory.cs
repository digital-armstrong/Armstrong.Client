using System.Collections.ObjectModel;

namespace Armstrong.Client.Models
{
    public class ZoomHistory : NotifyPropertyChanged
    {
        public int StepIndex { get; set; }
        public ObservableCollection<SeriesValue>? SeriesInStep { get; set; }
    }
}
