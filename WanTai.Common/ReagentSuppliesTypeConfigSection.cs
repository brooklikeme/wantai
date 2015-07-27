using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WanTai.Common
{
    class ReagentSuppliesTypeConfigSection : ConfigurationSection
    {
        /// <summary>

        /// The value of the property here "ReagentSuppliesTypes" needs to match that of the config file section

        /// </summary>

        [ConfigurationProperty("ReagentSuppliesTypes")]

        public ReagentSuppliesTypesCollection ReagentSuppliesTypeItems
        {

            get { return ((ReagentSuppliesTypesCollection)(base["ReagentSuppliesTypes"])); }

        }
    }

    /// <summary>

    /// The collection class that will store the list of each element/item that

    ///        is returned back from the configuration manager.

    /// </summary>

    [ConfigurationCollection(typeof(ReagentSuppliesTypeElement))]

    public class ReagentSuppliesTypesCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {

            return new ReagentSuppliesTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {

            return ((ReagentSuppliesTypeElement)(element)).TypeName;
        }

        public ReagentSuppliesTypeElement this[int idx]
        {
            get
            {
                return (ReagentSuppliesTypeElement)BaseGet(idx);
            }
        }

    }

    /// <summary>

    /// The class that holds onto each element returned by the configuration manager.

    /// </summary>

    public class ReagentSuppliesTypeElement : ConfigurationElement
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

        [ConfigurationProperty("unit", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Unit
        {
            get
            {
                return ((string)(base["unit"]));
            }

            set
            {
                base["unit"] = value;
            }
        }
    }
}
