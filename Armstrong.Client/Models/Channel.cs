using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Armstrong.Client.Models
{
    [Table("channels")]
    public class Channel : NotifyPropertyChanged
    {
        private int _channelState;
        private bool _channelSpecialControl;
        private string _stateImagePath = @"pack://application:,,,/Resources/normal.png";
        private DateTime _eventDateTime;
        private double _systemEventValue;
        private double _notSystemEventValue;
        private double _impulsesEventValue;
        private int _eventCount;
        private int _errorEventCount;

        [Key, Column("id")]
        public int Id { get; set; }
        [Column("channel_id")]
        public int ChannelId { get; set; }
        [Column("server_id")]
        public int ServerId { get; set; }
        [Column("name_controlpoint")]
        public string? ChannelName { get; set; }
        [Column("on_off")]
        public int ChannelPowerState { get; set; }
        [Column("state_for_threeview")]
        public int ChannelState
        {
            get => _channelState;
            set
            {
                if (value == _channelState) return;
                _channelState = value;
                OnPropertyChanged();
            }
        }
        [NotMapped]
        public string StateImagePath
        {
            get => _stateImagePath;
            set
            {
                if (value == _stateImagePath) return;
                _stateImagePath = value;
                OnPropertyChanged();
            }
        }

        [Column("consumption")]
        public double ChannelConsumption { get; set; }
        [Column("special_control")]
        public bool ChannelSpecialControl
        {
            get => _channelSpecialControl;
            set
            {
                if (value == _channelSpecialControl) return;
                _channelSpecialControl = value;
                OnPropertyChanged();
            }
        }

    [Column("name_db")]
        public string? DeviceName { get; set; }
        [Column("type")]
        public int DeviceType { get; set; }
        [Column("min_nuclid_value")]
        public double DeviceCalibrateMin { get; set; }
        [Column("max_nuclid_value")]
        public double DeviceCalibrateMax { get; set; }
        [Column("background")]
        public double DeviceSelfBackground { get; set; }
        [Column("name_location")]
        public string? DeviceLocation { get; set; }

        [Column("event_date")]
        public DateTime EventDateTime
        {
            get => _eventDateTime;
            set
            {
                if (value.Equals(_eventDateTime)) return;
                _eventDateTime = value;
                OnPropertyChanged();
            }
        }
        [Column("event_value")]
        public double SystemEventValue
        {
            get => _systemEventValue;
            set
            {
                if (value == _systemEventValue) return;
                _systemEventValue = value;
                OnPropertyChanged();
            }
        }
        [Column("unit")]
        public string? Unit { get; set; }
        [Column("value_cu")]
        public double NotSystemEventValue
        {
            get => _notSystemEventValue;
            set
            {
                if (value == _notSystemEventValue) return;
                _notSystemEventValue = value;
                OnPropertyChanged();
            }
        }
        [Column("value_impulses")]
        public double ImpulsesEventValue
        {
            get => _impulsesEventValue;
            set
            {
                if (value == _impulsesEventValue) return;
                _impulsesEventValue = value;
                OnPropertyChanged();
            }
        }
        [Column("coefficient")]
        public double ConvertCoefficient { get; set; } = 1;
        [Column("pre_accident")]
        public double PreEmgLimit { get; set; }
        [Column("accident")]
        public double EmgLimit { get; set; }
        [Column("count")]
        public int EventCount
        {
            get => _eventCount;
            set
            {
                if (value == _eventCount) return;
                _eventCount = value;
                OnPropertyChanged();
            }
        }
        [Column("error_count")]
        public int ErrorEventCount
        {
            get => _errorEventCount;
            set
            {
                if (value == _errorEventCount) return;
                _errorEventCount = value;
                OnPropertyChanged();
            }
        }

        public List<History> History { get; set; }
    }
}
