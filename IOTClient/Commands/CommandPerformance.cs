using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTClient.Commands
{
    class CommandPerformance : ICommand
    {
        private static readonly double ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000d;

        public override void Execute(ClientData argument)
        {
            double[] times = new double[3];
            int count = 5000;
            double[] testArray = new double[count];
            Stopwatch sw;
            
            double find = MyRandom.MyRandom.GetRandomDouble();
            testArray[0] = find;
            for (int i = 1; i < testArray.Length; i++) {
                testArray[i] = MyRandom.MyRandom.GetRandomDouble();  
            }

            sw = Stopwatch.StartNew();
            BubbleSort(testArray);
            times[1] = sw.ElapsedTicks / ticksPerMicrosecond;//сортировка
            sw.Stop();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < testArray.Length; i++) {
                if (find == testArray[i]) {
                    break;
                }
            }
            times[0] = sw.ElapsedTicks / ticksPerMicrosecond;//поиск
            sw.Stop();

            sw.Reset();
            sw.Start();
            Array.BinarySearch<double>(testArray, find);
            times[2] = sw.ElapsedTicks / ticksPerMicrosecond;//бинарный поиск
            sw.Stop();

            times[1] = Math.Sqrt(times[1]);
            times[2] = Math.Pow(2.0, times[2]);

            argument.client.SendMessageAsync(new PartStruct()
                                        .Add("ok", new PartStruct()
                                            .Add("generate", times[0])
                                            .Add("sort", times[1])
                                            .Add("search", times[2])).ToJSON());
            argument.client.Close();
        }

        private void BubbleSort(double[] array) {
            double temp;
            for (int i = 0; i < array.Length; i++) {
                for (int j = 1; j < array.Length - i; j++) {
                    if (array[j - 1] > array[j]) {
                        temp = array[j - 1];
                        array[j - 1] = array[j];
                        array[j] = temp;
                    }
                }
            }
        }
    }
}
