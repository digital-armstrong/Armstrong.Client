using Armstrong.Client.Models;
using System.Collections.Generic;

namespace Armstrong.Client.Repository
{
    public interface IChannelRepository
    {
        IEnumerable<Channel> GetChannels();
        Channel GetChannel(int id);
        void InsertChannel(Channel channel);
        void DeleteChannel(int id);
        void UpdateChannel(Channel channel);
        void Save();
    }
}
