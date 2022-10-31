using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Armstrong.Client.Data;
using Armstrong.Client.Models;
using Microsoft.EntityFrameworkCore;

namespace Armstrong.Client.Repository
{
    internal class ChannelRepository : IChannelRepository
    {
        private DataContext context;
        public ChannelRepository(DataContext context)
            => this.context = context;

        public void InsertChannel(Channel channel)
            => context.Channels.Add(channel);

        public IEnumerable<Channel> GetChannels()
            => context.Channels.ToList();

        public void UpdateChannel(Channel channel)
            => context.Entry(channel).State = EntityState.Modified;

        public void DeleteChannel(int id)
            => context.Channels.Remove(context.Channels.Find(id));

        public void Save() => context.SaveChanges();
    }
}
