using Armstrong.Client.Commands;
using Armstrong.Client.Data;
using Armstrong.Client.Helpers;
using Armstrong.Client.Models;
using Armstrong.Client.Repository;
using Armstrong.Client.Utilits;
using Newtonsoft.Json;
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
            SelectedChannelId = ChannelCollectionSingleton.GetInstance().SelectedChannelsId.FirstOrDefault();

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
                return new DelegateCommand((obj) =>
                {
                    var redisChannelName = "server_" + SelectedChannel.ServerId.ToString();
                    var _rMessage = new RedisMessage
                    {
                        ChannelGlobalId = SelectedChannel.Id,
                        ChannelLocalId = SelectedChannel.ChannelId,
                        ServerId = SelectedChannel.ServerId,
                        Channel = SelectedChannel,
                        Command = RedisMessage.ARMSCommand.UpdateFromDatabase,
                        LogDescription = $"Save command has been sent to redis server " +
                        $"for channel: {SelectedChannel.ChannelId} / {SelectedChannel.Id}, server: {SelectedChannel.ServerId}"
                    };
                    var message = JsonConvert.SerializeObject(_rMessage);

                    RedisHelper redisHelper = new(host: EnvironmentHelper.GetEnvirovmentVariable("REDIS_HOST"));
                    redisHelper.RedisSubscriber.Publish(redisChannelName, message);

                    Window channelInfoWindow = obj as Window;
                    channelInfoWindow.Close();
                });
            }
        }
    }
}
