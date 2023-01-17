using Armstrong.Client.Commands;
using Armstrong.Client.Constants;
using Armstrong.Client.Data;
using Armstrong.Client.Models;
using Armstrong.Client.Repository;
using Armstrong.Client.Utilits;
using Armstrong.Client.Views;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Armstrong.Client.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private GridLength _chartHeight = new(300);
        private GridLength _gridHeight = new(1, GridUnitType.Star);
        private ObservableCollection<ISeries> _series;
        private List<int> _serverIds;
        private string _selectedChannelName;
        private LiveChartsCore.Measure.ZoomAndPanMode _sizeMode;
        private SolidColorBrush _GrinColorBrush => new(Color.FromRgb(39, 174, 96));
        private SolidColorBrush _RedColorBrush => new(Color.FromRgb(235, 87, 87));
        private SolidColorBrush _DefaultColorBrush => new(Color.FromRgb(230, 230, 230));
        private Brush _updateStatusIconColor = new SolidColorBrush(Color.FromRgb(230, 230, 230));


        private ObservableCollection<Report> _reports = new();

        public ObservableCollection<Report> Reports
        {
            get { return _reports; }
            set
            {
                _reports = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Channel> Channels { get; set; }
        public ObservableCollection<DateTimePoint> PointsCollection { get; set; } = new();
        public ObservableCollection<ISeries> Series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Server> Servers { get; set; } = new();

        public LineSeries<DateTimePoint> LineSeries { get; set; } =
            new LineSeries<DateTimePoint>
            {
                GeometrySize = 0.1,
                Values = new ObservableCollection<DateTimePoint>()
            };
        public Axis[] XAxes { get; set; } = {
            new Axis
            {
                Labeler = value => new DateTime((long) value).ToString("HH:mm:ss"),
                LabelsPaint = new SolidColorPaint(SKColors.White),

                UnitWidth = TimeSpan.FromHours(1).Ticks,
                MinStep = TimeSpan.FromSeconds(1).Ticks
            }
        };
        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => value.ToString("E3"),
                LabelsPaint = new SolidColorPaint(SKColors.White)
            }
        };

        public List<Channel> SelectedChannels { get; set; } = new();
        public List<int> ServerIds
        {
            get => _serverIds;
            set
            {
                _serverIds = value;
                OnPropertyChanged();
            }
        }

        public LiveChartsCore.Measure.ZoomAndPanMode SizeMode
        {
            get => _sizeMode;
            set
            {
                _sizeMode = value;
                OnPropertyChanged();
            }
        }

        public GridLength GridHeight
        {
            get => _gridHeight;
            set
            {
                _gridHeight = value;
                OnPropertyChanged();
            }
        }

        public GridLength ChartHeight
        {
            get => _chartHeight;
            set
            {
                _chartHeight = value;
                OnPropertyChanged();
            }
        }

        public string SelectedChannelName
        {
            get => _selectedChannelName;
            set
            {
                _selectedChannelName = value;
                OnPropertyChanged();
            }
        }

        public Brush UpdateStatusIconColor
        {
            get => _updateStatusIconColor;
            set
            {
                _updateStatusIconColor = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView View { get; set; }
        public ICollectionViewLiveShaping LiveView { get; set; }

        public Predicate<object> Filter { get; set; } =
            new Predicate<object>(item => ((Channel)item).ServerId is not ChannelState.Untracked);


        public System.Timers.Timer MainViewUpdateTimer { get; set; } = new()
        {
            Enabled = true,
            Interval = 10000,
            AutoReset = true
        };
        public System.Timers.Timer ChartUpdateTimer { get; set; } = new()
        {
            Enabled = true,
            Interval = 20000,
            AutoReset = true
        };

        public MainViewModel()
        {
            ChartUpdateTimer.Elapsed += ChartUpdateTimerElapsed;
            MainViewUpdateTimer.Elapsed += MainViewUpdateTimerElapsed;

            Channels = GetChannels();
            ServerIds = GetServerIds();
            Servers = GetServers();
            Series = GetChartSeries();

            View = CollectionViewSource.GetDefaultView(Channels);
            View.Filter = Filter;

            LiveView = View as ICollectionViewLiveShaping;
            LiveView.LiveFilteringProperties.Add(nameof(Channel.ChannelState));
            LiveView.IsLiveFiltering = true;

            MainViewUpdateTimer.Start();
        }

        private void ChartUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            SetLineSeriesPoints(isWatching: true);
        }

        private void MainViewUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ChannelsUpdate();
            ServersIndicationUpdate();
        }

        private void ApplyFilter(Predicate<object> filter, bool isLiveFiltering)
        {
            View.Filter = filter;
            LiveView.IsLiveFiltering = isLiveFiltering;
        }

        private void ChannelsUpdate()
        {
            var repo = new ChannelRepository(new DataContext());
            var fetchedChannels = repo.GetChannels();

            foreach (var channelFromDb in fetchedChannels)
            {
                var channel = Channels.FirstOrDefault(c => c.Id == channelFromDb.Id);

                if (channel != null)
                {
                    SetChannelState(channel: channel, state: channelFromDb.ChannelState);
                    UpdateChannelInfo(channel: channel, update: channelFromDb);
                }
            }
        }

        private ObservableCollection<Channel> GetChannels()
        {
            var repo = new ChannelRepository(new DataContext());
            return new ObservableCollection<Channel>(repo.GetChannels()
                                                         .OrderBy(x => x.ServerId)
                                                         .ThenBy(n => n.ChannelId));
        }

        public ObservableCollection<ISeries> GetChartSeries()
        {
            return new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    GeometrySize = 0.1,
                    Values = PointsCollection,
                    TooltipLabelFormatter = (chartPoint)
                        => $"Name: {SelectedChannels.Select(x => x.ChannelName).FirstOrDefault()}\nDate: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yyyy HH:mm:ss}\nValue: {chartPoint.PrimaryValue:E3}",
                }
            };
        }

        private ObservableCollection<Server> GetServers()
        {
            var servers = new ObservableCollection<Server>();

            if (ServerIds.Any())
            {
                foreach (var id in ServerIds)
                {
                    servers.Add(new Server(id));
                }
            }

            return servers;
        }

        private List<int> GetServerIds()
        {
            if (Channels.Any())
            {
                return new List<int>(ServerIds = Channels.Select(x => x.ServerId)
                                                                   .Distinct()
                                                                   .ToList());
            }
            else
            {
                return new List<int>();
            }
        }

        private void ServersIndicationUpdate()
        {
            foreach (var server in Servers)
            {
                var lastUpdateTime = Channels.Where(c => c.ServerId == server.Id)
                                                  .Select(x => x.EventDateTime)
                                                  .ToList();

                server.UpdateServerState(lastUpdateTime);
            }
        }

        private static void SetChannelState(Channel channel, int state)
        {
            channel.ChannelState = state;

            switch (state)
            {
                default:
                    break;
                case ChannelState.Offline:
                    channel.StateImagePath = ImagePath.Offline;
                    break;
                case ChannelState.ChannelDown:
                    channel.StateImagePath = ImagePath.ChannelDown;
                    break;
                case ChannelState.Normal:
                    channel.StateImagePath = ImagePath.Normal;
                    break;
                case ChannelState.Warning:
                    channel.StateImagePath = ImagePath.Warning;
                    break;
                case ChannelState.Danger:
                    channel.StateImagePath = ImagePath.Danger;
                    break;
            }
        }

        private void SetLineSeriesPoints(bool isWatching)
        {
            if (SelectedChannels.Any())
            {
                using (var context = new DataContext())
                {
                    var startDate = new DateTime();
                    var pointsLimit = 100;

                    startDate = isWatching ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddDays(-1);

                    var histories = context.Histories.AsNoTracking()
                                                   .Where(x => x.Id == SelectedChannels.Select(x => x.Id).FirstOrDefault())
                                                   .Where(d => d.EventDate > startDate)
                                                   .OrderBy(x => x.EventDate)
                                                   .ToList();
                    PointsCollection.Clear();


                    if (histories.Count > pointsLimit && !isWatching)
                    {
                        var avgHistory = GetAvgHistory(histories);
                        foreach (var history in avgHistory)
                        {
                            PointsCollection.Add(new DateTimePoint(history.EventDate,
                                                                        history.SystemEventValue));
                        }
                    }
                    else
                    {
                        foreach (var history in histories)
                        {
                            PointsCollection.Add(new DateTimePoint(history.EventDate,
                                                                        history.SystemEventValue));
                        }
                    }
                }
            }
        }

        private static List<History> GetAvgHistory(List<History> histories)
        {
            int seporator = histories.Count / 100;
            List<History> tempHistories = new();
            List<History> avgHistory = new();

            foreach (var hist in histories)
            {
                if (seporator >= tempHistories.Count)
                {
                    tempHistories.Add(new() { SystemEventValue = hist.SystemEventValue, EventDate = hist.EventDate });
                }
                else
                {
                    avgHistory.Add(new History()
                    {
                        SystemEventValue = tempHistories.Select(x => x.SystemEventValue).Average(),
                        EventDate = GetAvgDateTime(tempHistories.Select(x => x.EventDate).ToList())
                    });

                    tempHistories.Clear();
                }
            }

            return avgHistory;
        }

        private static DateTime GetAvgDateTime(List<DateTime> dates)
        {
            var count = dates.Count;
            double temp = 0D;

            for (int i = 0; i < count; i++)
            {
                temp += dates[i].Ticks / (double)count;
            }

            return new DateTime((long)temp);
        }

        private static void UpdateChannelInfo(Channel channel, Channel update)
        {
            channel.ChannelName = update.ChannelName;
            channel.DeviceName = update.DeviceName;
            channel.DeviceSelfBackground = update.DeviceSelfBackground;
            channel.DeviceLocation = update.DeviceLocation;
            channel.ChannelSpecialControl = update.ChannelSpecialControl;

            channel.EventDateTime = update.EventDateTime;
            channel.SystemEventValue = update.SystemEventValue;
            channel.NotSystemEventValue = update.NotSystemEventValue;
            channel.ImpulsesEventValue = update.ImpulsesEventValue;
            channel.EventCount = update.EventCount;
            channel.ErrorEventCount = update.ErrorEventCount;
        }

        private struct FilterNodeName
        {
            public const string All = "all";
            public const string Warning = "preEmg";
            public const string Danger = "emg";
            public const string SpecialControl = "specialControl";
            public const string Important = "important";
            public const string ChannelDown = "channelDown";
            public const string ChannelOffline = "offline";
        }
        private struct FilterPredicate
        {
            public static Predicate<object> Normal
                => new(item => ((Channel)item).ChannelState is not ChannelState.Untracked);
            public static Predicate<object> Warning
                => new(item => ((Channel)item).ChannelState is ChannelState.Warning or ChannelState.Danger);
            public static Predicate<object> Danger
                => new(item => ((Channel)item).ChannelState is ChannelState.Danger);
            public static Predicate<object> SpecialControl
                => new(item => ((Channel)item).ChannelSpecialControl);
            public static Predicate<object> ChannelDown
                => new(item => ((Channel)item).ChannelState is ChannelState.ChannelDown);
            public static Predicate<object> ChannelOffline
                => new(item => ((Channel)item).ChannelState is ChannelState.Offline);
        }

        public ICommand GetChart
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ICollection<Object> channels = (ICollection<Object>)obj;

                    SelectedChannels.Clear();

                    foreach (Channel channel in channels)
                    {
                        SelectedChannels.Add(channel);
                    }

                    SelectedChannelName = SelectedChannels.Select(x => x.ChannelName).FirstOrDefault();
                    UpdateStatusIconColor = _GrinColorBrush;
                    SetLineSeriesPoints(isWatching: true);
                    ChartUpdateTimer.Start();
                });
            }
        }

        public ICommand GetDayChart
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ICollection<Object> channels = (ICollection<Object>)obj;

                    SelectedChannels.Clear();

                    foreach (Channel channel in channels)
                    {
                        SelectedChannels.Add(channel);
                    }

                    SelectedChannelName = SelectedChannels.Select(x => x.ChannelName).FirstOrDefault();
                    UpdateStatusIconColor = _RedColorBrush;
                    ChartUpdateTimer.Stop();
                    SetLineSeriesPoints(isWatching: false);
                });
            }
        }

        public ICommand MaximizeClick
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    GridHeight = new(0);
                    ChartHeight = new(1, GridUnitType.Star);
                });
            }
        }

        public ICommand ShowTimeSelector
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    TimeSelectorView timeSelector = new TimeSelectorView();
                    timeSelector.Show();
                });
            }
        }

        public ICommand MinimazeClick
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    GridHeight = new(1, GridUnitType.Star);
                    ChartHeight = new(300);
                });
            }
        }

        public ObservableCollection<Channel> SelectedChannelBindingCollection { get; set; }

        public ICommand GetDateTimePicker
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ICollection<Object> channels = (ICollection<Object>)obj;

                    SelectedChannelBindingCollection = ChartCollectionSingleton.GetInstance().SelectedChannelsCollection;
                    SelectedChannelBindingCollection.Clear();

                    foreach (Channel _channel in channels)
                    {
                        SelectedChannelBindingCollection.Add(_channel);
                    }

                    DateTimePickerView dateTimePicker = new DateTimePickerView();
                    dateTimePicker.Show();
                });
            }
        }

        public ICommand RenderChart
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                });
            }
        }

        public ObservableCollection<int> SelectedChannelId { get; set; }
        public ICommand ShowChannelInfo
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    var channels = (ICollection<Object>)obj;
                    var selectedChannels = new List<Channel>();

                    foreach (Channel _channel in channels)
                    {
                        selectedChannels.Add(_channel);
                    }

                    SelectedChannelId = ChannelCollectionSingleton.GetInstance().SelectedChannel;
                    SelectedChannelId.Add(selectedChannels.FirstOrDefault().Id);

                    ChannelInfoView channelInfoView = new();
                    channelInfoView.Show();
                });
            }
        }

        public ICommand ClickThreeView
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj.GetType() == typeof(Server))
                    {
                        var selectedNode = (Server)obj;
                        Predicate<object> predicate = new(item => ((Channel)item).ServerId == selectedNode.Id);
                        ApplyFilter(predicate, false);
                    }
                    if (obj.GetType() == typeof(StackPanel))
                    {
                        var stackPannel = (StackPanel)obj;

                        switch (stackPannel.Name)
                        {
                            case FilterNodeName.All:
                                ApplyFilter(FilterPredicate.Normal, isLiveFiltering: false);
                                break;
                            case FilterNodeName.Warning:
                                ApplyFilter(FilterPredicate.Warning, isLiveFiltering: true);
                                break;
                            case FilterNodeName.Danger:
                                ApplyFilter(FilterPredicate.Danger, isLiveFiltering: true);
                                break;
                            case FilterNodeName.SpecialControl:
                                ApplyFilter(FilterPredicate.SpecialControl, isLiveFiltering: true);
                                break;
                            case FilterNodeName.Important:
                                // TODO, add in database column and impl. props
                                break;
                            case FilterNodeName.ChannelDown:
                                ApplyFilter(FilterPredicate.ChannelDown, isLiveFiltering: true);
                                break;
                            case FilterNodeName.ChannelOffline:
                                ApplyFilter(FilterPredicate.ChannelOffline, isLiveFiltering: true);
                                break;
                        }
                    }
                });
            }
        }
    }
}
