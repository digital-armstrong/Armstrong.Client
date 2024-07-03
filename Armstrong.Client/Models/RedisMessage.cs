namespace Armstrong.Client.Models
{
    class RedisMessage
    {
        public enum ARMSCommand { UpdateFromDatabase = 1, UpdateLocalData }
        public enum ARMSAction { NoAction = 1, TestChannel, RewindChannel }
        public int ChannelGlobalId { get; set; }
        public int ChannelLocalId { get; set; }
        public int ServerId { get; set; }
        public Channel Channel { get; set; }
        public ARMSCommand Command { get; set; } = ARMSCommand.UpdateFromDatabase;
        public ARMSAction Action { get; set; } = ARMSAction.NoAction;
        public string LogDescription { get; set; } = string.Empty;
    }
}
