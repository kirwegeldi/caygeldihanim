using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAY_Weighing.IpConfiguration
{
    public class IpElementCollection : ConfigurationElementCollection
    {
        public IpElement this[int index]
        {
            get
            {
                // Gets the IpConfig at the specified
                // index in the collection
                return (IpElement)BaseGet(index);
            }
            set
            {
                // Check if a IpConfig exists at the
                // specified index and delete it if it does
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                // Add the new IpConfig at the specified
                // index
                BaseAdd(index, value);
            }
        }
        public new IpElement this[string key]
        {
            get
            {
                // Gets the IpConfig where the name
                // matches the string key specified
                return (IpElement)BaseGet(key);
            }
            set
            {
                // Checks if a IpConfig exists with
                // the specified name and deletes it if it does
                if (BaseGet(key) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));

                // Adds the new IpConfig
                BaseAdd(value);
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new IpElement();
        }

        // Method that must be overriden to get the key of a
        // specified element
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IpElement)element).id;
        }
    }
}
