using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTServer.StatisticData
{
    public class Metrics
    {
        public double jitter;
        public double delay;
        public double speed;
        public double missed;

        public Metrics() {}

        public Metrics(double jitter, double delay, double speed, double missed)
        {
            this.jitter = jitter;
            this.delay = delay;
            this.speed = speed;
            this.missed = missed;
        }

        public IPart GetJsonData() {
            return new PartStruct()
                .Add("jitter", String.Format("{0} c", jitter))
                .Add("delay", String.Format("{0} c", delay))
                .Add("speed", String.Format("{0} Mbit/c", speed))
                .Add("missed", String.Format("{0}%", missed));
        }
    }
}
