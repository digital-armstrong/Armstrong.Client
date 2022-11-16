using System.Collections.ObjectModel;

namespace Armstrong.Client.Utilits
{
    public class ReportsCollectionSingleton
    {
        private static ReportsCollectionSingleton _source;
        public ObservableCollection<object> BlowoutCollection;
        public ObservableCollection<object> SpecialControlCollection;
        public ObservableCollection<object> IodineCollection;
        public ObservableCollection<object> CategoryCollection { get; set; }

        private ReportsCollectionSingleton()
        {
            BlowoutCollection = new ObservableCollection<object>();
            SpecialControlCollection = new ObservableCollection<object>();
            IodineCollection = new ObservableCollection<object>();
            CategoryCollection = new ObservableCollection<object>();
        }

        public static ReportsCollectionSingleton GetInstance()
        {
            if (_source == null)
            {
                _source = new ReportsCollectionSingleton();
            }

            return _source;
        }
    }
}
