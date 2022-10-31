using System.Collections;
using System.Collections.Generic;
using Armstrong.Client.Models;

namespace Armstrong.Client.Repository
{
    public interface IChannelRepository
    {
        IEnumerable<Channel> GetChannels();
        void InsertChannel(Channel channel);
        void DeleteChannel(int id);
        void UpdateChannel(Channel channel);
        void Save();
    }
}
