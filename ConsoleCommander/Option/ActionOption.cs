using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCommander.Options
{
    [Serializable]
    public class ActionOption : Option
    {
        public ActionOption(string shortName, string fullName, int minArgsPassed, int maxArgsPassed, Action<IEnumerable<string>, IEnumerable<object>> action)
            : base(shortName, fullName, minArgsPassed, maxArgsPassed) => OptionAction = action;


        public ActionOption(string shortName, string fullName, int minArgsPassed, Action<IEnumerable<string>, IEnumerable<object>> action)
            : this(shortName, fullName, minArgsPassed, minArgsPassed, action) { }


        public event Action<IEnumerable<string>, IEnumerable<object>> OptionAction;

        public void InvokeOptionAction()
        {
            OptionAction(GetArguments(), AdditionalArgumentsForNextInvoke);
            AdditionalArgumentsForNextInvoke = null;
        }

        public IEnumerable<object> AdditionalArgumentsForNextInvoke { get; set; }

        public void SetAllArguments(IEnumerable<string> args)
            => arguments = args.ToList();


        public bool IsPreFuncOption { get; set; } = false;
    }
}
