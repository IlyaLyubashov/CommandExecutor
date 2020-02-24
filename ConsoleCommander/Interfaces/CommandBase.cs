using ConsoleCommander.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleCommander.Options;

namespace ConsoleCommander.Interfaces
{
    public abstract class CommandBase
    {
        private List<Option> options;
        private List<ActionOption> preFunc = new List<ActionOption>();
        private List<ActionOption> postFunc = new List<ActionOption>();

        public CommandBase()
        {
            InitializeOptions();
        }


        protected virtual void InitializeOptions()
        {
            options = new List<Option>();
        }


        public abstract string Name { get; }
        

        public abstract string Info { get; }


        public Action<string> SentOut { get; set; }


        public void Invoke(IEnumerable<Option> optsForInvoke)
        {

            //переписывать все под option, потому что передаются копии, а мы тут пытаемся оригинал вызывать
            // as OptionAction => if(OptionAction and )         
            InvokeOptionsActions(optsForInvoke, true);
            Invoke(GetFuncArguments(optsForInvoke), optsForInvoke.ToDictionary(o => o.FullName ?? "function arguments", o => o));
            InvokeOptionsActions(optsForInvoke, false);
        }

        protected abstract void Invoke(IEnumerable<string> funcArguments,IDictionary<string, Option> fullNameToOption);

        /// <summary>
        /// От парсера получаем набор Option, а не ActionOption. Передаем нужным ActionOption'aм аргументы.
        /// </summary>
        /// <param name="optsForInvoke"></param>
        /// <param name="isPreFunc"></param>
        private void InvokeOptionsActions(IEnumerable<Option> optsForInvoke, bool isPreFunc)
        {
            var optionsToWorkWith = isPreFunc ? preFunc : postFunc;
            optsForInvoke.ToList().ForEach(o =>
            {
                if (o.IsOptionSet)
                {
                    var properOpt = optionsToWorkWith.Where(op => op.FullName == o.FullName).FirstOrDefault();
                    if (properOpt != null)
                    {
                        properOpt.SetAllArguments(o.GetArguments());
                        properOpt.InvokeOptionAction();
                    }
                }
            });
        } 


        protected IEnumerable<string> GetFuncArguments(IEnumerable<Option> optsForInvoke)
        {
            return optsForInvoke.FirstOrDefault(o => o.FullName == null && o.ShortName == null)?.GetArguments();
        }


        protected void AddOption(string shortName, string fullName, int minArgs, int maxArgs, IEnumerable<string> defaults = null)
        {
            if (string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(shortName))
                throw new ArgumentNullException("Command must have full name.");

            var opt = new Option(shortName, fullName, minArgs, maxArgs);
            opt.SetDefaultArguments(defaults);
            if (!IsOptionUnique(options,opt))
                throw new ArgumentException("Option names must be unique.");
            options.Add(opt);
        }


        protected void AddOption(string shortName, string fullName, int minArgs, IEnumerable<string> defaults = null)
            => AddOption(shortName, fullName, minArgs, minArgs, defaults);

        protected void AddOption(string shortName, string fullName)
            => AddOption(shortName, fullName, 0, null);

        public IEnumerable<Option> GetPossibleOptions() =>  
            DeepCopy.Make(options.Concat(preFunc).Concat(postFunc));


        protected void AddPreFuncOption(ActionOption actOpt) => AddActionOption(preFunc, actOpt);


        protected void AddPostFuncOption(ActionOption actOpt) => AddActionOption(postFunc, actOpt);


        private bool IsOptionUnique(IEnumerable<Option> opts, Option opt) =>
            !opts.Any(o => o.ShortName == opt.ShortName || o.FullName == opt.FullName);

            


        private void AddActionOption(List<ActionOption> opts, ActionOption opt)
        {
            if (!IsOptionUnique(opts, opt))
                throw new ArgumentException("Option names must be unique.");
            opts.Add(opt);
        }


        protected void AddFunctionArguments(int minArgsCount) => AddFunctionArguments(minArgsCount, null);


        protected void AddFunctionArguments(int minArgs, IEnumerable<string> defaults) => AddFunctionArguments(minArgs, minArgs, null);


        protected void AddFunctionArguments(int minArgsCount,int maxArgsCount, IEnumerable<string> defaults)
            => AddOption(null, null, minArgsCount, maxArgsCount, defaults);


        protected void AddFunctionArguments(int minArgs, int maxArgs) => AddFunctionArguments(minArgs, maxArgs, null);        
    }
}
