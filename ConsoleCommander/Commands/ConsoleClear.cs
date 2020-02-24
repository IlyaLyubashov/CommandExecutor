using ConsoleCommander.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCommander.Commands
{
    class ConsoleClear : CommandBase
    {
        public override string Name => "clear";

        public override string Info => throw new NotImplementedException();

        protected override void Invoke(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
        {
            Console.Clear();
        }
    }
}
