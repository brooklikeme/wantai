using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace WanTai.DataModel
{
    partial class ReagentAndSuppliesConfiguration:INotifyPropertyChanged

    {
        public static string NeedVolumeFieldName = "_NeedVolume";
        public static string CurrentVolumeFieldName = "_CurrentVolume";

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private double _NeedVolume;
        public double NeedVolume
        {
            get { return _NeedVolume; }
            set { _NeedVolume = value; }
        }

        private double _ActualSavedVolume;
        public double ActualSavedVolume
        {
            get { return _ActualSavedVolume; }
            set { _ActualSavedVolume = value; }
        }

        private double _FirstAddVolume;
        public double FirstAddVolume 
        {
            get
            {
                return _FirstAddVolume;
            }
            set 
            {
                if (value != _FirstAddVolume)
                {
                    this._FirstAddVolume = value;
                    NotifyPropertyChanged("FirstAddVolume");
                }
            }
        }

        private double _CurrentActualVolume;
        public double CurrentActualVolume
        {
            get { return _CurrentActualVolume; }
            set { _CurrentActualVolume = value; }
        }

        private double _TotalNeedValueAndConsumption;
        public double TotalNeedValueAndConsumption
        {
            get { return _TotalNeedValueAndConsumption; }
            set { _TotalNeedValueAndConsumption = value; }
        }

        private double _CurrentVolume;
        public double CurrentVolume
        {
            get 
            {
                return _CurrentVolume; 
            }
            set 
            {
                if (value != _CurrentVolume)
                {
                    _CurrentVolume = value;
                    NotifyPropertyChanged("CurrentVolume");
                }
            }
        }

        public double AddVolume { get; set; }

        private bool _Correct;
        public bool Correct 
        {
            get
            {
                return _Correct;
            }
            set 
            {
                if (value != _Correct)
                {
                    _Correct = value;
                    NotifyPropertyChanged("Correct");
                }
            } 
        }
    }
}
