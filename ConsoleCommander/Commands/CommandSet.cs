using ConsoleCommander.Commands;
using ConsoleCommander.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ConsoleCommander.Commands
{
    public static class CommandSet
    {
        public static IImmutableDictionary<string, CommandBase> GetCommands()
        { 
            var commands = new Dictionary<string, CommandBase>();

            var timer = new AppTimer();
            commands.Add(timer.Name, timer);

            var consoleClear = new ConsoleClear();
            commands.Add(consoleClear.Name, consoleClear);

            return commands.ToImmutableDictionary();
        }
    }
}
