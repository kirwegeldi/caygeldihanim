using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CAY_Weighing.IpConfiguration
{
    public class IpElement : ConfigurationElement
    {
        public IpElement()
        {

        }
        [ConfigurationProperty("id", DefaultValue = "0", IsKey = true, IsRequired = true)]
        public int id
        {
            get
            {
                // Return the value of the 'address' attribute as a string
                return (int)base["id"];
            }
            set
            {
                // Set the value of the 'address' attribute
                base["id"] = value;
            }
        }
        [ConfigurationProperty("IpAddress", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string IpAddress
        {
            get
            {
                // Return the value of the 'IpAddress' attribute as a string
                return (string)base["IpAddress"];
            }
            set
            {
                // Set the value of the 'IpAddress' attribute
                base["IpAddress"] = value;
            }
        }
        [ConfigurationProperty("port", DefaultValue = "4000", IsRequired = false)]
        public int port
        {
            get
            {
                // Return the value of the 'port' attribute as a string
                return (int)base["port"];
            }
            set
            {
                // Set the value of the 'port' attribute
                base["port"] = value;
            }
        }

    }
}
