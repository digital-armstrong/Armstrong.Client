using System.Collections.ObjectModel;

namespace Armstrong.Client.Utilits
{
    public class ChannelCollectionSingleton
    {
        private static ChannelCollectionSingleton _source;
        public ObservableCollection<int> SelectedChannel;

        private ChannelCollectionSingleton()
        {
            SelectedChannel = new ObservableCollection<int>();
        }

        public static ChannelCollectionSingleton GetInstance()
        {
            _source ??= new ChannelCollectionSingleton();

            return _source;
        }
    }
}
