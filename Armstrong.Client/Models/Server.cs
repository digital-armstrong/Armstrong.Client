using System;
using System.Collections.Generic;
using System.Linq;
using Armstrong.Client.Constants;

namespace Armstrong.Client.Models
{
    public class Server : NotifyPropertyChanged
    {
        private bool _serverState = false;
        private string _stateImagePath = @"pack://application:,,,/Resources/normal.png";
        private DateTime _lastUpdate = new();
        private DateTime _previosUpdate = new();

        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public int FailtureUpdateCount { get; set; } = 0;
        public bool ServerState
        {
            get => this._serverState;
            set
            {
                if (value == this._serverState) return;
                this._serverState = value;
                this.OnPropertyChanged();
            }
        }
        public string StateImagePath
        {
            get => this._stateImagePath;
            set
            {
                if (value == this._stateImagePath) return;
                this._stateImagePath = value;
                this.OnPropertyChanged();
            }
        }
        public DateTime PreviosUpdate
        {
            get => this._previosUpdate;
            set
            {
                if (value == this._previosUpdate) return;
                this._previosUpdate = value;
                this.OnPropertyChanged();
            }
        }
        public DateTime LastUpdate
        {
            get => this._lastUpdate;
            set
            {
                if (value == this._lastUpdate) return;
                this._lastUpdate = value;
                this.OnPropertyChanged();
            }
        }

        public Server(int id)
        {
            this.Id = id;
            this.Name = $"Сервер №{this.Id}";
        }

        public void SetServerState()
        {
            var failtureUpdateLimit = 10;
            var maxDelay = 2.0;
            var diff = this.LastUpdate.Subtract(this.PreviosUpdate).TotalMinutes;

            this.FailtureUpdateCount = diff > 0 && diff < maxDelay ? 0 : ++this.FailtureUpdateCount;

            this.ServerState = this.FailtureUpdateCount < failtureUpdateLimit;
            this.PreviosUpdate = this.LastUpdate;
        }

        public void SetServerImageState()
        {
            if (this.ServerState)
                this.StateImagePath = ImagePath.Normal;
            else
                this.StateImagePath = ImagePath.Danger;
        }

        public void FindMaxDate(List<DateTime> dateTimes)
        {
            if (dateTimes.Count is 0) return;

            DateTime maxDate = dateTimes.Max();
            this.LastUpdate = maxDate;
        }

        public void UpdateServerState(List<DateTime> dateTimes)
        {
            this.FindMaxDate(dateTimes);
            this.SetServerState();
            this.SetServerImageState();
        }
    }
}
