using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace UWPBiped
{
    class Variables
    {
        public static Speech voice;
        public static ConfigData config;
        public static ObservableCollection<PololuMaestroPort> maestroPorts;
        public static PololuMaestro maestro;
    }
}
