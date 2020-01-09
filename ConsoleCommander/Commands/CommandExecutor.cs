using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using ConsoleCommander.Interfaces;
using System.IO;
using ConsoleCommander.Options;

namespace ConsoleCommander.Commands
{
    class CommandExecutor
    {
        private IImmutableDictionary<string, CommandBase> commands;
        private TextReader _inIO;
        private TextWriter _outIO;


        public CommandExecutor( TextReader inIO = null, TextWriter outIO = null)
        {
            this.commands = new Dictionary<string,CommandBase>().ToImmutableDictionary();
            _inIO = inIO ?? Console.In;
            _outIO = outIO ?? Console.Out;
        }


        public void SetCommands(IImmutableDictionary<string, CommandBase> commands)
        {
            this.commands = commands;
        }


        //TODO: как команде выводить инфу: пусть executor получает или команде передается 
        public void TryInvoke(params string[] stringParams)
        {
            if (stringParams.Length == 0)
                return;
            var cmdName = stringParams[0];
            var args = new string[stringParams.Length - 1];
            Array.Copy(stringParams,1,args,0,args.Length);
            if (commands.ContainsKey(cmdName))
            {
                var cmd = commands[cmdName];
                var opts = OptionsParser.Parse(args, cmd.GetPossibleOptions());
                cmd.Invoke(opts);
            }
                
        }


        public void ListenForever()
        {
            var input = "";
            while (true)
            {
                var k = _inIO.Read();
                var ch = char.ConvertFromUtf32(k);
                if (!char.IsControl(ch[0]))
                    input += ch;
                if (k == (int)ConsoleKey.Enter)
                {                    
                    TryInvoke(input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                    input = "";
                }
            }
        }
    }
}
