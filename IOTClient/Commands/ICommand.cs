using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * абстракция команды выполняемая сервером
 * имеет доступ к данным сервера и запросу
 */
namespace IOTClient.Commands
{
    public abstract class ICommand
    {

        public ICommand() {
            
        }

        public abstract void Execute(ClientData argument);
    }
}
