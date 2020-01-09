using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCommander.Options
{
    public class OptionsParser
    {
        public static IEnumerable<Option> Parse(string[] opts, IEnumerable<Option> possible)
        {
            var possibleOpts = possible.ToArray();
            Option curOption = null;
            foreach (var param in opts)
            {
                if (curOption == null)
                {
                    curOption = TryGetOption(param, possibleOpts);
                }
                else
                {
                    OnArgumentAddOrOptionChange(ref curOption, param,possibleOpts);
                }
            }
            var lastNotFilled = possibleOpts.FirstOrDefault(opt => (opt.IsOptionSet || opt.IsFunctionArgs) && !opt.IsArgumentsSetRequired);
            if (lastNotFilled != null)
                    throw new ArgumentNullException((lastNotFilled.FullName ?? lastNotFilled.ShortName) ?? "Function arguments", 
                        $"This option needs minimum {lastNotFilled.argumentsRequired} arguments.");

            return possibleOpts;
        }



        private static void OnArgumentAddOrOptionChange(ref Option curOption, string param, Option[] possibleOpts)
        {
            if (IsOptionNameSyntax(param))
            {
                if (!curOption.IsArgumentsSetRequired)
                    throw new ArgumentException("You can't pass arguments, that starts with '-' or '--'. Involve this into string." +
                                                $"Or there's not enough arguments for previous option. Minimal is {curOption.argumentsRequired}.");

                curOption = TryGetOption(param, possibleOpts);
            }
            else
            {
                if (!curOption.SetArgument(param))
                    throw new ArgumentOutOfRangeException(param, $"Option {curOption.FullName} can take {curOption.argumentsRequired} arguments. " +
                    $"You passed more.");
                curOption.StartSetArguments();
            }
                        
        }


        private static Option TryGetOption(string param, Option[] possibleOpts)
        {
            if (IsOptionNameSyntax(param))
            {
                var option = possibleOpts.FirstOrDefault(o => o.ShortName == param || o.FullName == param);
                if (option != null)
                {
                    option.MakeOptionSet();
                    return option;
                }
                    
                throw new ArgumentNullException(param,"Option with such name doesn't exist.");
            }

            var paramForFunctionArgs = possibleOpts.FirstOrDefault( o => o.FullName == null && o.ShortName == null);
            if (paramForFunctionArgs != null)
            {
                // это исключение никогда не упадет, так как нет возможности возниковения такой ситуации, аргументы всегда передаются сначала
                if(paramForFunctionArgs.IsOptionSet)
                    throw new ArgumentException("Function arguments can be passed only once.");
                if(!paramForFunctionArgs.SetArgument(param))
                    throw new ArgumentException("Function has 'null' option for arguments, but it doesn't take arguments.");
                paramForFunctionArgs.StartSetArguments();
                return paramForFunctionArgs;
            }

            throw new ArgumentException($"Function doesn't need arguments.");
        }

        static bool IsOptionNameSyntax(string param) => param.StartsWith("-");
    }
}
