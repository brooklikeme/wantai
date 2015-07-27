using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.DataModel.Configuration
{
    public class LiquidType
    {
        private string typeName;
        private string color;
        private bool hasVolume;
        private int defaultVolume;
        private bool canSelectedMultiCell;
        private short typeId;

        public string TypeName
        {
            set
            {
                typeName = value;
            }
            get
            {
                return typeName;
            }
        }

        public string Color
        {
            set
            {
                color = value;
            }
            get
            {
                return color;
            }
        }

        public bool HasVolume
        {
            set
            {
                hasVolume = value;
            }
            get
            {
                return hasVolume;
            }
        }

        public int DefaultVolume
        {
            set
            {
                defaultVolume = value;
            }
            get
            {
                return defaultVolume;
            }
        }

        public bool CanSelectedMultiCell
        {
            set
            {
                canSelectedMultiCell = value;
            }
            get
            {
                return canSelectedMultiCell;
            }
        }

        public short TypeId
        {
            set
            {
                typeId = value;
            }
            get
            {
                return typeId;
            }
        }
    }
}
