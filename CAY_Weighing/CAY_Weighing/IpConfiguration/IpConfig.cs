using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;

namespace CAY_Weighing.IpConfiguration
{
    public class IpConfig : ConfigurationSection
    {
        [ConfigurationProperty("silo")]
        [ConfigurationCollection(typeof(IpElementCollection))]
        public IpElementCollection Silos
        {
            get
            {
                // Get the collection and parse it
                return (IpElementCollection)this["silo"];
            }
        }

    }
}
