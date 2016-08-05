using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace UWPBiped
{
    public sealed class Basic
    {
        private readonly ManualResetEventSlim waitEvent = new ManualResetEventSlim(false);
        private readonly DateTime startTime = DateTime.Now;


        public Basic()
        {
        }

        public void Pause(double milliseconds)
        {
            waitEvent.Wait(TimeSpan.FromMilliseconds(milliseconds));
        }

        public double RunningTime()
        {
            return (DateTime.Now - startTime).TotalMilliseconds;
        }
    }
}
