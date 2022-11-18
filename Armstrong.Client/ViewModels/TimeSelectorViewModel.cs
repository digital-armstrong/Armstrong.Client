using Armstrong.Client.Commands;
using Armstrong.Client.Models;
using Armstrong.Client.Utilits;
using Armstrong.Client.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Armstrong.Client.ViewModels
{
    public class TimeSelectorViewModel : NotifyPropertyChanged
    {
        private bool _isDaysRangeSelected = true;
        private bool _isShiftRangeSelected;
        private bool _isHoursRangeSelected;
        private ObservableCollection<Report> _reports = new();
        private DateTime _startDateTime = DateTime.UtcNow;

        public bool IsDaysRangeSelected
        {
            get { return _isDaysRangeSelected; }
            set { _isDaysRangeSelected = value; OnPropertyChanged(); }
        }

        public bool IsShiftRangeSelected
        {
            get { return _isShiftRangeSelected; }
            set { _isShiftRangeSelected = value; OnPropertyChanged(); }
        }

        public bool IsHoursRangeSelected
        {
            get { return _isHoursRangeSelected; }
            set { _isHoursRangeSelected = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Report> Reports
        {
            get { return _reports; }
            set
            {
                _reports = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> BlowoutBindigCollection { get; set; }
        public ObservableCollection<object> SpecialControlBindigCollection { get; set; }
        public ObservableCollection<object> IodineBindigCollection { get; set; }
        public ObservableCollection<object> CategoryBlowoutBindingCollection { get; set; }

        public ObservableCollection<Report> BlowoutReport { get; set; } = new();
        public ObservableCollection<Report> SpecialControlReport { get; set; } = new();
        public ObservableCollection<Report> IodineReport { get; set; } = new();

        public DateTime Days { get; private set; } = DateTime.UtcNow.AddDays(-1);
        public DateTime Shift { get; private set; } = DateTime.UtcNow.AddHours(-6);
        public DateTime TwoHours { get; private set; } = DateTime.UtcNow.AddHours(-2);

        public TimeSelectorViewModel()
        {
            _startDateTime = Days;
        }

        private void StartReport()
        {
            int V1 = 275, V2 = 276, V3 = 277, V4 = 278, V4_ = 279, V5 = 280, V6 = 281, V7 = 282;

            int[] blowoutGroup = new int[] { V1, V2, V3, V4, V4_, V5, V6, V7 };
            int[] specialControlGroup = new int[] { 267, 268, 270, 271, 300, 301, 333, 343 };
            int[] iodineGroup = new int[] { 235, 236, 237, 238, 229, 230 };

            foreach (var channel in blowoutGroup)
            {
                BlowoutReport.Add(new(id: channel, startDateTime: _startDateTime));
            }

            foreach (var channel in specialControlGroup)
            {
                SpecialControlReport.Add(new(id: channel, startDateTime: _startDateTime));
            }

            foreach (var channel in iodineGroup)
            {
                IodineReport.Add(new(id: channel, startDateTime: _startDateTime));
            }

            var I = new Channel()
            {
                ChannelName = "Категория I",
                SystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 1).Sum(x => x.BlowoutSystemValue),
                NotSystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 1).Sum(x => x.BlowoutNotSystemValue)
            };
            var II = new Channel()
            {
                ChannelName = "Категория II",
                SystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 2).Sum(x => x.BlowoutSystemValue),
                NotSystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 2).Sum(x => x.BlowoutNotSystemValue)
            };
            var III = new Channel()
            {
                ChannelName = "Категория III",
                SystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 3).Sum(x => x.BlowoutSystemValue),
                NotSystemEventValue = BlowoutReport.Where(x => x.BlowoutReportCategory == 3).Sum(x => x.BlowoutNotSystemValue)
            };
            var summ = new Channel()
            {
                ChannelName = "Суммарная",
                SystemEventValue = I.SystemEventValue + II.SystemEventValue + III.SystemEventValue,
                NotSystemEventValue = I.NotSystemEventValue + II.NotSystemEventValue + III.NotSystemEventValue,
            };

            CategoryBlowoutBindingCollection = ReportsCollectionSingleton.GetInstance().CategoryCollection;
            CategoryBlowoutBindingCollection.Clear();

            CategoryBlowoutBindingCollection.Add(I);
            CategoryBlowoutBindingCollection.Add(II);
            CategoryBlowoutBindingCollection.Add(III);
            CategoryBlowoutBindingCollection.Add(summ);
        }

        public ICommand RadioButtonClick
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (obj is not null)
                    {
                        _startDateTime = (DateTime)obj;
                    }
                });
            }
        }

        public ICommand GetReport
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Reports.Clear();
                    StartReport();

                    BlowoutBindigCollection = ReportsCollectionSingleton.GetInstance().BlowoutCollection;
                    SpecialControlBindigCollection = ReportsCollectionSingleton.GetInstance().SpecialControlCollection;
                    IodineBindigCollection = ReportsCollectionSingleton.GetInstance().IodineCollection;

                    BlowoutBindigCollection.Clear();
                    SpecialControlBindigCollection.Clear();
                    IodineBindigCollection.Clear();

                    foreach (var r in BlowoutReport)
                    {
                        BlowoutBindigCollection.Add(r);
                    }

                    foreach (var r in SpecialControlReport)
                    {
                        SpecialControlBindigCollection.Add(r);
                    }

                    foreach (var r in IodineReport)
                    {
                        IodineBindigCollection.Add(r);
                    }

                    ReportView reportView = new();
                    reportView.Show();

                    if (obj is not null)
                    {
                        Window timeSelectorWindow = obj as Window;
                        timeSelectorWindow.Close();
                    }
                });
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
                        Window timeSelectorWindow = obj as Window;
                        timeSelectorWindow.Close();
                    }
                });
            }
        }
    }
}
