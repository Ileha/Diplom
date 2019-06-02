using JSONParserLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IOTClient.Commands
{
    class CommandStress : ICommand
    {
        public override void Execute(ClientData argument)
        {
            //requ	{"data_count": 1000}
            int count = argument.data["data_count"].GetValue<int>();

            uint sessionID = MyRandom.MyRandom.GetRandomUInt32();
            IPEndPoint server = new IPEndPoint(argument.client.address, MainClass.UDP_STATISTICS_SERVER_PORT);
            UdpClient Client = new UdpClient();
            MemoryStream sendData = new MemoryStream(new byte[512]);

            using (BinaryWriter writer = new BinaryWriter(sendData))
            {
                writer.Write(sessionID);
            }
            byte[] data = sendData.ToArray();

            for (int i = 0; i < count; i++)
            {
                Client.Send(data, data.Length, server);
            }

            argument.client.SendMessageAsync(new PartStruct()
                                        .Add("ok", new PartStruct()
                                             .Add("session_id", sessionID)).ToJSON());
            argument.client.Close();
        }
    }
}
