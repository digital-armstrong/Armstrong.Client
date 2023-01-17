using Armstrong.Client.Commands;
using Armstrong.Client.Data;
using Armstrong.Client.Models;
using Armstrong.Client.Repository;
using Armstrong.Client.Utilits;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class ChannelInfoViewModel : NotifyPropertyChanged
    {
        private Channel _selectedChannel;
        public int SelectedChannelId { get; set; }
        public Channel SelectedChannel { get; set; }

        public ChannelInfoViewModel()
        {
            SelectedChannelId = ChannelCollectionSingleton.GetInstance().SelectedChannel.FirstOrDefault();

            if (SelectedChannelId is not 0)
            {
                var repo = new ChannelRepository(new DataContext());
                SelectedChannel = repo.GetChannel(SelectedChannelId);
            }
        }

        public ICommand CloseWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        Window channelInfoWindow = obj as Window;
                        channelInfoWindow.Close();
                    }
                });
            }
        }

        public ICommand UnlockFill
        {
            get
            {
                return new DelegateCommand((obj) => { });
            }
        }

        public ICommand Save
        {
            get
            {
                return new DelegateCommand((obj) => { });
            }
        }
    }
}
