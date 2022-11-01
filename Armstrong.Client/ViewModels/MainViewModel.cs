using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Armstrong.Client.Commands;
using Armstrong.Client.Constants;
using Armstrong.Client.Data;
using Armstrong.Client.Models;
using Armstrong.Client.Repository;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace Armstrong.Client.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<ISeries> _series;
        private List<int> _serverIds;

        public ObservableCollection<Channel> Channels { get; set; }
        public ObservableCollection<DateTimePoint> PointsCollection { get; set; } = new();
        public ObservableCollection<ISeries> Series
        {
            get => this._series;
            set
            {
                this._series = value;
                this.OnPropertyChanged();
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
            get => this._serverIds;
            set
            {
                this._serverIds = value;
                this.OnPropertyChanged();
            }
        }

        private LiveChartsCore.Measure.ZoomAndPanMode _sizeMode;
        public LiveChartsCore.Measure.ZoomAndPanMode SizeMode
        {
            get => this._sizeMode;
            set
            {
                this._sizeMode = value;
                this.OnPropertyChanged();
            }
        }

        private GridLength _gridHeight = new(1, GridUnitType.Star);

        public GridLength GridHeight
        {
            get => this._gridHeight;
            set
            {
                this._gridHeight = value;
                this.OnPropertyChanged();
            }
        }

        private GridLength _chartHeight = new(300);

        public GridLength ChartHeight
        {
            get => this._chartHeight;
            set
            {
                this._chartHeight = value;
                this.OnPropertyChanged();
            }
        }

        private string _selectedChannelName;

        public string SelectedChannelName
        {
            get => this._selectedChannelName;
            set
            {
                this._selectedChannelName = value;
                this.OnPropertyChanged();
            }
        }

        private string _isWatched;

        public string IsWatched
        {
            get => this._isWatched;
            set
            {
                this._isWatched = value;
                this.OnPropertyChanged();
            }
        }


        public ICollectionView View { get; set; }
        public ICollectionViewLiveShaping LiveView { get; set; }

        public Predicate<object> Filter { get; set; } =
            new Predicate<object>(item => ((Channel)item).ServerId is not ChannelState.Untracked);


        public System.Timers.Timer MainViewUpdateTimer { get; set; } = new() {
            Enabled = true,
            Interval = 10000,
            AutoReset = true
        };
        public System.Timers.Timer ChartUpdateTimer { get; set; } = new() {
            Enabled = true,
            Interval = 20000,
            AutoReset = true
        };

        public MainViewModel()
        {
            this.ChartUpdateTimer.Elapsed += this.ChartUpdateTimerElapsed;
            this.MainViewUpdateTimer.Elapsed += this.MainViewUpdateTimerElapsed;

            this.Channels = GetChannels();
            this.ServerIds = GetServerIds();
            this.Servers = GetServers();
            this.Series = GetChartSeries();

            this.View = CollectionViewSource.GetDefaultView(this.Channels);
            this.View.Filter = this.Filter;

            this.LiveView = this.View as ICollectionViewLiveShaping;
            this.LiveView.LiveFilteringProperties.Add(nameof(Channel.ChannelState));
            this.LiveView.IsLiveFiltering = true;

            this.IsWatched = ImagePath.Offline;

            this.MainViewUpdateTimer.Start();
        }

        private void ChartUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            this.SetLineSeriesPoints(isWatching: true);
        }

        private void MainViewUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            this.ChannelsUpdate();
            this.ServersIndicationUpdate();
        }

        private void ApplyFilter(Predicate<object> filter, bool isLiveFiltering)
        {
            this.View.Filter = filter;

            if (isLiveFiltering)
                this.LiveView.IsLiveFiltering = true;
            else
                this.LiveView.IsLiveFiltering = false;
        }

        private void ChannelsUpdate()
        {
            var repo = new ChannelRepository(new DataContext());
            var fetchedChannels = repo.GetChannels();

            foreach (var channelFromDb in fetchedChannels)
            {
                var channel = this.Channels.FirstOrDefault(c => c.Id == channelFromDb.Id);

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
                    Values = this.PointsCollection,
                    TooltipLabelFormatter = (chartPoint)
                        => $"Name: {this.SelectedChannels.Select(x => x.ChannelName).FirstOrDefault()}\nDate: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yyyy HH:mm:ss}\nValue: {chartPoint.PrimaryValue:E3}",
                }
            };
        }

        private ObservableCollection<Server> GetServers()
        {
            var servers = new ObservableCollection<Server>();

            if (this.ServerIds.Any())
                foreach (var id in this.ServerIds)
                    servers.Add(new Server(id));

            return servers;
        }

        private List<int> GetServerIds()
        {
            if (this.Channels.Any())
                return new List<int>(this.ServerIds = this.Channels.Select(x => x.ServerId)
                                                                   .Distinct()
                                                                   .ToList());
            else
                return new List<int>();
        }

        private void ServersIndicationUpdate()
        {
            foreach (var server in this.Servers)
            {
                var lastUpdateTime = this.Channels.Where(c => c.ServerId == server.Id)
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
            if (this.SelectedChannels.Any())
            {
                using (var context = new DataContext())
                {
                    var startDate = new DateTime();
                    var pointsLimit = 100;

                    startDate = isWatching ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddDays(-1);

                    var histories = context.Histories.AsNoTracking()
                                                   .Where(x => x.Id == this.SelectedChannels.Select(x => x.Id).FirstOrDefault())
                                                   .Where(d => d.EventDate > startDate)
                                                   .OrderBy(x => x.EventDate)
                                                   .ToList();
                    this.PointsCollection.Clear();


                    if (histories.Count > pointsLimit && !isWatching)
                    {
                        var avgHistory = GetAvgHistory(histories);
                        foreach (var history in avgHistory)
                            this.PointsCollection.Add(new DateTimePoint(history.EventDate,
                                                                        history.SystemEventValue));
                    }
                    else
                    {
                        foreach (var history in histories)
                            this.PointsCollection.Add(new DateTimePoint(history.EventDate,
                                                                        history.SystemEventValue));
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
                    tempHistories.Add(new() { SystemEventValue = hist.SystemEventValue, EventDate = hist.EventDate });
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
                temp += dates[i].Ticks / (double)count;

            return new DateTime((long)temp);
        }

        private static void UpdateChannelInfo(Channel channel, Channel update)
        {
            channel.ChannelSpecialControl = update.ChannelSpecialControl;
            channel.EventDateTime = update.EventDateTime;
            channel.SystemEventValue = update.SystemEventValue;
            channel.NotSystemEventValue = update.NotSystemEventValue;
            channel.ImpulsesEventValue = update.ImpulsesEventValue;
            channel.EventCount = update.EventCount;
            channel.ErrorEventCount = update.ErrorEventCount;
            channel.ChannelSpecialControl = update.ChannelSpecialControl;
        }

        private struct FilterNodeName
        {
            public const string All = "all";
            public const string Warning = "preEmg";
            public const string Danger  = "emg";
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

                    this.SelectedChannels.Clear();

                    foreach (Channel channel in channels)
                    {
                        this.SelectedChannels.Add(channel);
                    }

                    this.SelectedChannelName = this.SelectedChannels.Select(x => x.ChannelName).FirstOrDefault();
                    this.IsWatched = ImagePath.Normal;
                    this.SetLineSeriesPoints(isWatching: true);
                    this.ChartUpdateTimer.Start();
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

                    this.SelectedChannels.Clear();

                    foreach (Channel channel in channels)
                    {
                        this.SelectedChannels.Add(channel);
                    }

                    this.SelectedChannelName = this.SelectedChannels.Select(x => x.ChannelName).FirstOrDefault();
                    this.IsWatched = ImagePath.Danger;
                    this.ChartUpdateTimer.Stop();
                    this.SetLineSeriesPoints(isWatching: false);
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
                        this.ApplyFilter(predicate, false);
                    }
                    if (obj.GetType() == typeof(StackPanel))
                    {
                        var stackPannel = (StackPanel)obj;

                        switch (stackPannel.Name)
                        {
                            case FilterNodeName.All:
                                this.ApplyFilter(FilterPredicate.Normal, isLiveFiltering: false);
                                break;
                            case FilterNodeName.Warning:
                                this.ApplyFilter(FilterPredicate.Warning, isLiveFiltering: true);
                                break;
                            case FilterNodeName.Danger:
                                this.ApplyFilter(FilterPredicate.Danger, isLiveFiltering: true);
                                break;
                            case FilterNodeName.SpecialControl:
                                this.ApplyFilter(FilterPredicate.SpecialControl, isLiveFiltering: true);
                                break;
                            case FilterNodeName.Important:
                                // TODO, add in database column and impl. props
                                break;
                            case FilterNodeName.ChannelDown:
                                this.ApplyFilter(FilterPredicate.ChannelDown, isLiveFiltering: true);
                                break;
                            case FilterNodeName.ChannelOffline:
                                this.ApplyFilter(FilterPredicate.ChannelOffline, isLiveFiltering: true);
                                break;
                        }
                    }
                });
            }
        }
    }
}
