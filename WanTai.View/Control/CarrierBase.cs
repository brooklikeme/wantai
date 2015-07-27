using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Data;

namespace WanTai.View.Control
{
    public abstract class CarrierBase : UserControl
    {
        private string carrierName;
        public string CarrierName
        {
            get { return carrierName; }
            set { carrierName = value; }
        }

        public List<PlateBase> Plates { get; set; }
        
        public abstract void UpdatePlate(List<PlateBase> plates);

        public abstract void Scan();

        public abstract void ShiningStop();
        public abstract void ShiningStop(string PlateName);
        public abstract void ShiningStop(Guid ConfigurationItemID);
        public abstract void ShiningWithGreen(string[] plates);
    }
}
