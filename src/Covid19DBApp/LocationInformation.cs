using Covid19DB.Entities;
using System.ComponentModel;

namespace Covid19DBApp
{
    public class LocationInformation : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields
        private int confirmed;
        private int deaths;
        private int recoveries;
        #endregion

        #region Public Properties
        public Location Location;

        public LocationInformation(Location location)
        {
            Location = location;
        }

        public int Confirmed
        {
            get => confirmed;
            set
            {
                confirmed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Confirmed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Deaths
        {
            get => deaths;
            set
            {
                deaths = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Deaths)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Recoveries
        {
            get => recoveries;
            set
            {
                recoveries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Recoveries)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Active => Confirmed - (Deaths + Recoveries);
        #endregion
    }
}
