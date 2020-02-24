using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCommander
{
    [Serializable]
    public class Option
    {
        //добавить делегат с нужным экшеном
        public readonly int argumentsRequired;
        public readonly int maxArgsPassed;
        protected List<string> arguments = new List<string>();
        private readonly OptionName optName;
        [JsonRequired]
        private List<string> defaultArguments = new List<string>();


        [JsonConstructor]
        public Option(string shortName, string fullName, int minArgsPassed, int maxArgsPassed)
        {
            optName = new OptionName(shortName, fullName);
            if (maxArgsPassed < minArgsPassed || minArgsPassed < 0)
                throw new ArgumentException("Max arguments passed can not be fewer, then min args passed", optName.fullName ?? optName.shortName);
            argumentsRequired = minArgsPassed;
            this.maxArgsPassed = maxArgsPassed;
        }


        public Option(string shortName, string fullName, int minArgsPassed) : this(shortName, fullName, minArgsPassed, minArgsPassed) { }


        public Option(int minArgs, int maxArgs) : this(null, null, minArgs, maxArgs) { }


        public Option(int minArgs): this(minArgs,minArgs) { }


        public bool IsArgumentsSetRequired => arguments.Count >= argumentsRequired;


        public bool IsOptionSet { get; private set; } = false;

        public bool IsArgumentSetStarted { get; set; }


        public bool IsArgumentsSetFully => arguments.Count == maxArgsPassed;


        public string FullName => optName.fullName;


        public string ShortName => optName.shortName;


        public bool IsFunctionArgs => ShortName == null && FullName == null;

        public void MakeOptionSet() => IsOptionSet = true;


        public void SetDefaultArguments(IEnumerable<string> defaultArgs) => defaultArguments = defaultArgs?.ToList() ?? defaultArguments;


        public IEnumerable<string> GetArguments() => IsArgumentSetStarted ? arguments : defaultArguments;

        private void ArgumentSetStarted() => IsArgumentSetStarted = true;

        protected void ResetArguments()
        {
            arguments = new List<string>();
            IsArgumentSetStarted = false;
        } 

        public bool SetArgument(string arg)
        {
            ArgumentSetStarted();
            while (arguments.Count < maxArgsPassed)
            {
                arguments.Add(arg);
                return true;
            }
            return false;
        }
    }

    public struct OptionName
    {
        public readonly string shortName;
        public readonly string fullName;

        public OptionName(string shortName = null, string fullName = null)
        {
            this.shortName = null;
            this.fullName = null;
            if (shortName!=null && !OptionUtils.CheckShortName(shortName, out this.shortName))
                throw new ArgumentException("Option short name is not right.", shortName);
            if (fullName != null && !OptionUtils.CheckForFullName(fullName, out this.fullName))
                throw new ArgumentException("Option full name is not right.", fullName);
        }
    }


    internal static class OptionUtils
    {
        public static bool CheckShortName(string name, out string shortName)
        {
            shortName = name;
            if (name.Length >= 2 && name.StartsWith("-") && char.IsLetterOrDigit(name[1]))
                return true;
            return false;
        }

        public static bool CheckForFullName(string name, out string fullName)
        {
            fullName = name;
            if (name.Length >= 3 && name.StartsWith("--") && char.IsLetterOrDigit(name[2]))
                return true;
            return false;
        }
    }
}
