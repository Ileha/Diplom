using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * абстракция команды выполняемая сервером
 * имеет доступ к данным сервера и запросу
 */
namespace UiserClient.Commands
{
    public abstract class ICommand
    {
        protected CommonData data { get; private set; }
        private CommandDataPattern pattern;

        public ICommand(CommonData data, CommandDataPattern pattern)
        {
            this.pattern = pattern;
            this.data = data;
        }


        public void Execute(CommandData argument) {
            argument.SetPattern(pattern);
            execute(argument);
        }
        protected abstract void execute(CommandData argument);
        protected void SetData(CommonData data) {
            this.data = data;
        }
    }
}
