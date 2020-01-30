using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit;
using NUnit.Framework;
using ConsoleCommander;
using ConsoleCommander.Options;

namespace ConsoleCommanderTests
{
    [TestFixture]
    public class OptionsBuilderTests_Should
    {
        private string[] parameters;
        private List<Option> possibleOpts;

        [SetUp]
        public void SetUp()
        {
            parameters = "100 -t 5 abc --d ab".Split();
            possibleOpts = new List<Option>()
            {
                new Option(1),
                new Option("-t",null,1,3),
                new Option(null,"--d",1)
            };
        }

        [Test]
        public void Parse_SimpleOptions()
        {
            ParseWithDefaults();
            Assert.That(
                possibleOpts.GetFuncArguments().GetArguments().SequenceEqual(new[] { "100" }) &&
                possibleOpts.GetOption("-t").GetArguments().SequenceEqual(new[] { "5", "abc" }) &&
                possibleOpts.GetOption("--d").GetArguments().SequenceEqual(new[] { "ab" })
            );
        }


        [Test]
        public void Parse_FunctionWithoutArguments_ExceptionOnPassArgs()
        {
            var funcArgs = possibleOpts[0];
            possibleOpts.Remove(funcArgs);
            Assert.Throws<ArgumentException>(() => ParseWithDefaults());
        }



        //seems to be strange shit
        [Test]
        public void Parse_HasFunctionArgsOption_OptionMaxZeroArguments()
        {
            var funcArgs = possibleOpts[0];
            possibleOpts.Remove(funcArgs);
            possibleOpts.Add(new Option(0));
            Assert.Throws<ArgumentException>(() => ParseWithDefaults());
        }


        [Test]
        public void Parse_TooMuchArgumentsForOption_Throws()
        {
            parameters = (string.Join(' ', parameters) + " acc").Split();
            Assert.Throws<ArgumentOutOfRangeException>(() => ParseWithDefaults());
        }


        [Test]
        public void Parse_ChangeOptionBeforeSetRequiredArguments_Throws()
        {
            parameters = "-t --d abc".Split();
            Assert.Throws<ArgumentException>(() => ParseWithDefaults());
        }


        [Test]
        public void Parse_ProvideNotExistingOption_Throws()
        {
            parameters = "-k 1 2 3".Split();
            Assert.Throws<ArgumentNullException>(()=>ParseWithDefaults());
        }


        [Test]
        public void Parse_LastOptionNotRequiredArguments_Throws()
        {
            possibleOpts[2] = new Option(null,"--d",2);
            parameters = "-t 5 --d 1".Split();
            Assert.Throws<ArgumentNullException>(() =>ParseWithDefaults());
        }


        [Test]
        public void Parse_NoFunctionArgumentButRequired_Throws()
        {
            parameters = new string[0];
            possibleOpts[0] = new Option(1);
            Assert.Throws<ArgumentNullException>(()=> ParseWithDefaults());
        }

        private void ParseWithDefaults()
        {
            OptionsParser.Parse(parameters, possibleOpts);
        }
    }

    internal static class OptsIEnumerableExtension
    {

        public static Option GetOption(this IEnumerable<Option> opts, string shortOrFullName) => opts.FirstOrDefault(o =>
            o.ShortName == shortOrFullName
            || o.FullName == shortOrFullName);

        public static Option GetFuncArguments(this IEnumerable<Option> opts) => opts.FirstOrDefault(o =>
            o.ShortName == null
            || o.FullName == null);
    }
}
