using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOTClient.Commands
{
    class CommandLoad : ICommand
    {
        const double MIN_TIME = 500;
        const double MAX_TIME = 50000;
        /*
        run load test with selected random distribution
        use as:
        LoadTest -r type_of_random_distribution
            normal -m double_num -d double_num
            gamma -a double_num -l double_num
            erlang -m uint_num, -l double_num
            pareto -x double_num -a double_num
         */

        /*
         requ	{"data_count": 1000, "distribution": "normal", "m": 0, "d": 1}
         */
        private double[] randomDelays(IPart data, out double min, out double max) {
            double[] delays = new double[data.Get("data_count").GetValue<int>()];

            string distibution = data.Get("distribution").GetValue<string>();
            Func<double> getNext = null;
            if (distibution == "normal") {
                double m = data.Get("m").GetValue<double>();
                double d = data.Get("d").GetValue<double>();
                getNext = () => { return MyRandom.MyRandom.NormalDistribution(m, d); };
            }
            else if (distibution == "gamma") {
                double a = data.Get("a").GetValue<double>();
                double l = data.Get("l").GetValue<double>();
                getNext = () => { return MyRandom.MyRandom.GammaDistribution(a, l); };
            }
            else if (distibution == "erlang") {
                uint m = data.Get("m").GetValue<uint>();
                double l = data.Get("l").GetValue<double>();
                getNext = () => { return MyRandom.MyRandom.ErlangDistribution(m, l); };
            }
            else if (distibution == "pareto") {
                double x = data.Get("x").GetValue<double>();
                double a = data.Get("a").GetValue<double>();
                getNext = () => { return MyRandom.MyRandom.ParetoDistribution(x, a); };
            }
            else {
                throw new Exception("unknown distribution");
            }

            max = Double.MinValue;
            min = Double.MaxValue;

            for (int i = 0; i < delays.Length; i++) {
                delays[i] = getNext();
                if (delays[i] < min) { min = delays[i]; }
                if (delays[i] > max) { max = delays[i]; }
            }

            return delays;
        }

        public override void Execute(ClientData argument)
        {
            double ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000d;
            long last = 0;

            double max;
            double min;

            double[] delays = randomDelays(argument.data.Data, out min, out max);
            //long[] delays = new long[] { 500, 40000, 30000, 1000, 500, 50000 }; //в микросекундах

            uint sessionID = MyRandom.MyRandom.GetRandomUInt32();
            IPEndPoint server = new IPEndPoint(argument.client.address, MainClass.UDP_STATISTICS_SERVER_PORT);
            UdpClient Client = new UdpClient();
            MemoryStream sendData = new MemoryStream(new byte[512]);

            using (BinaryWriter writer = new BinaryWriter(sendData)) {
                writer.Write(sessionID);
            }
            byte[] data = sendData.ToArray();

            Stopwatch sw;
            sw = Stopwatch.StartNew();

            for (int i = 0; i < delays.Length; i++) {
                last += (long)(remap(delays[i], min, max, MIN_TIME, MAX_TIME) * ticksPerMicrosecond);
                while (sw.Elapsed.Ticks < last) {}
                Client.Send(data, data.Length, server);
                //Console.WriteLine((double)sw.Elapsed.Ticks / (double)TimeSpan.TicksPerMillisecond);
            }  

            sw.Stop();
            Thread.Sleep((int)MAX_TIME/1000);
            argument.client.SendMessageAsync(new PartStruct()
                                        .Add("ok", new PartStruct()
                                             .Add("session_id", sessionID)).ToJSON());
            argument.client.Close();
        }

        private double remap(double value, double low1, double high1, double low2, double high2) {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }
    }
}
