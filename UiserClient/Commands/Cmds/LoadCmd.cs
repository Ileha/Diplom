using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiserClient.Commands.Cmds
{
    class LoadCmd : ICommand
    {
        public LoadCmd(CommonData data, CommandDataPattern pattern) 
            : base(data, pattern) 
        {}

        /*
            run load test with selected random distribution
            use as:
            LoadTest -r type_of_random_distribution
                normal -m double_num -d double_num
                gamma -a double_num -l double_num
                erlang -m uint_num, -l double_num
                pareto -x double_num -a double_num
        */

        //request {"cmd":"load", data: {"data_count": 1000, "distribution": "normal", "m": 0, "d": 1, "clients":[]}}
        protected override void execute(CommandData argument) {
            uint count = argument.GetKeyValue<UInt32>('c');
            string distribution_type = argument.GetKeyValue<string>('r');
            IPart dataToRequest = new PartStruct();
            if (distribution_type == "normal") {
                dataToRequest.Add("m", argument.GetKeyValue<double>('m'));
                dataToRequest.Add("d", argument.GetKeyValue<double>('d'));
            }
            else if (distribution_type == "gamma") {
                dataToRequest.Add("a", argument.GetKeyValue<double>('a'));
                dataToRequest.Add("l", argument.GetKeyValue<double>('l'));
            }
            else if (distribution_type == "erlang") {
                dataToRequest.Add("m", argument.GetKeyValue<uint>('m'));
                dataToRequest.Add("l", argument.GetKeyValue<double>('l'));
            }
            else if (distribution_type == "pareto") {
                dataToRequest.Add("x", argument.GetKeyValue<double>('x'));
                dataToRequest.Add("a", argument.GetKeyValue<double>('a'));
            }
            else {
                throw new Exception("wrong distribution type");
            }
            dataToRequest
                .Add("distribution", distribution_type)
                .Add("data_count", count)
                .Add("clients", data.GetSelectedClients());

            IPart request = new PartStruct()
                .Add("cmd", "load")
                .Add("data", dataToRequest);

            JSONParser replyData = new JSONParser(data.server.SendMessageAsync(request.ToJSON()).Result);
            IPart ok = replyData["ok"];
            IPart error = null;
            foreach (IPart enemy in ok) {
                Console.WriteLine(enemy.ByPath("name").GetValue<string>());
                if (enemy.ByPathSave("result.exception", out error)) {
                    Console.WriteLine("\t{0}", error.GetValue<string>());
                }
                else {
                    Console.WriteLine("\tjitter:\t{0}", enemy.ByPath("result.jitter").GetValue<string>());
                    Console.WriteLine("\tdelay:\t{0}", enemy.ByPath("result.delay").GetValue<string>());
                    Console.WriteLine("\tspeed:\t{0}", enemy.ByPath("result.speed").GetValue<string>());
                    Console.WriteLine("\tmissed:\t{0}", enemy.ByPath("result.missed").GetValue<string>());
                }
            }
            //Console.WriteLine(replyData.ToJSON());
        }
    }
}
