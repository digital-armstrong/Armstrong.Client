using Armstrong.Client.Models;
using System.Collections.ObjectModel;

namespace Armstrong.Client.Utilits
{
    public class ChannelCollectionSingleton
    {
        private static ChannelCollectionSingleton _source;
        public ObservableCollection<int> SelectedChannelsId;
        public ObservableCollection<Channel> SelectedChannelsCollection;

        private ChannelCollectionSingleton()
        {
            SelectedChannelsId = new ObservableCollection<int>();
            SelectedChannelsCollection = new ObservableCollection<Channel>();
        }

        public static ChannelCollectionSingleton GetInstance()
        {
            _source ??= new ChannelCollectionSingleton();

            return _source;
        }
    }
}
