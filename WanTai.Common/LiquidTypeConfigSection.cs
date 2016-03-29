using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WanTai.Common
{
    class LiquidTypeConfigSection : ConfigurationSection
    {
        /// <summary>

        /// The value of the property here "Folders" needs to match that of the config file section

        /// </summary>

        [ConfigurationProperty("LiquidTypes")]

        public LiquidTypesCollection LiquidTypeItems
        {

            get { return ((LiquidTypesCollection)(base["LiquidTypes"])); }

        }
    }

    /// <summary>

    /// The collection class that will store the list of each element/item that

    ///        is returned back from the configuration manager.

    /// </summary>

    [ConfigurationCollection(typeof(LiquidTypeElement))]

    public class LiquidTypesCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {

            return new LiquidTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {

            return ((LiquidTypeElement)(element)).TypeName;
        }

        public LiquidTypeElement this[int idx]
        {
            get
            {
                return (LiquidTypeElement)BaseGet(idx);
            }
        }

    }

    /// <summary>

    /// The class that holds onto each element returned by the configuration manager.

    /// </summary>

    public class LiquidTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("typeName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string TypeName
        {
            get
            {
                return ((string)(base["typeName"]));
            }

            set
            {
                base["typeName"] = value;
            }
        }

        [ConfigurationProperty("color", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Color
        {
            get
            {
                return ((string)(base["color"]));
            }

            set
            {
                base["color"] = value;
            }
        }

        [ConfigurationProperty("hasVolume", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string HasVolume
        {
            get
            {
                return ((string)(base["hasVolume"]));
            }

            set
            {
                base["hasVolume"] = value;
            }
        }

        [ConfigurationProperty("defaultVolume", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string DefaultVolume
        {
            get
            {
                return ((string)(base["defaultVolume"]));
            }

            set
            {
                base["defaultVolume"] = value;
            }
        }

        [ConfigurationProperty("canSelectedMultiCell", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string CanSelectedMultiCell
        {
            get
            {
                return ((string)(base["canSelectedMultiCell"]));
            }

            set
            {
                base["canSelectedMultiCell"] = value;
            }
        }

        [ConfigurationProperty("canGroup", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string CanGroup
        {
            get
            {
                return ((string)(base["canGroup"]));
            }

            set
            {
                base["canGroup"] = value;
            }
        }

        [ConfigurationProperty("typeId", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string TypeId
        {
            get
            {
                return ((string)(base["typeId"]));
            }

            set
            {
                base["typeId"] = value;
            }
        }
    }
}
