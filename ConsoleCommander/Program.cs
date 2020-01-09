using System;
using System.Threading;
using ConsoleCommander.Commands;

namespace ConsoleCommander
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandExecutor = new CommandExecutor();
            commandExecutor.SetCommands(CommandSet.GetCommands());
            commandExecutor.ListenForever();
        }
    }
}
