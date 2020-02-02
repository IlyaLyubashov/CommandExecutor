using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCommander.Options
{
    [Serializable]
    public class ActionOption : Option
    {
        [JsonConstructor]
        public ActionOption(string shortName, string fullName, int minArgsPassed, int maxArgsPassed, Action<IEnumerable<string>, IEnumerable<object>> action)
            : base(shortName, fullName, minArgsPassed, maxArgsPassed) => OptionAction = action;


        public ActionOption(string shortName, string fullName, int minArgsPassed, Action<IEnumerable<string>, IEnumerable<object>> action)
            : this(shortName, fullName, minArgsPassed, minArgsPassed, action) { }


        public event Action<IEnumerable<string>, IEnumerable<object>> OptionAction;


        /// <summary>
        ///ActionOption у нас синглтоны получаются
        /// </summary>
        public void InvokeOptionAction()
        {
            OptionAction(GetArguments(), AdditionalArgumentsForNextInvoke);
            AdditionalArgumentsForNextInvoke = null;
            ResetArguments();
        }


        public IEnumerable<object> AdditionalArgumentsForNextInvoke { get; set; }

        public void SetAllArguments(IEnumerable<string> args)
        {
            args.ToList().ForEach(arg => SetArgument(arg));
        }

        public Action<IEnumerable<string>, IEnumerable<object>> GetOptionAction()
            => OptionAction;

        public bool IsPreFuncOption { get; set; } = false;
    }
}
