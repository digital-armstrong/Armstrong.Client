using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Armstrong.Client.Models
{
    [Table("histories")]
    public class History : NotifyPropertyChanged
    {
        [Key, Column("channel_id")]
        public int Id { get; set; }
        [Column("event_value")]
        public double SystemEventValue { get; set; }
        [Column("event_date")]
        public DateTime EventDate { get; set; }

        public Channel Channel { get; set; }
        [Column("channel_id")]
        public int ChannelId { get; set; }
    }
}
