using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MylaFlower
{
    public class SaveSettings
    {
        public bool DeliveredFlower { get; set; } = false;
    }

    public class GlobalSettings
    {
        public RandoConnection.RandoSettings RandoSettings { get; set; } = new();
    }
}
