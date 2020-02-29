using ConsoleCommander;
using ConsoleCommander.Interfaces;
using ConsoleCommander.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleCommanderTests
{
    [TestFixture]
    class BaseCommandController_Test
    {
        [Test]
        public void FakeController_SimpleTest()
        {
            var fakeController = new FakeController();
            var messageList = new List<string>();
            fakeController.SentOut = (newStrSent) => messageList.Add(newStrSent);
            var args = "fake -o1 1 -o2 hello".Split(' ');
            var opts = OptionsParser.Parse(args,fakeController.GetPossibleOptions(args));
            fakeController.Invoke(opts);
            Assert.That(messageList, Is.EquivalentTo(new string[] { "1","hello"}));
        }
        class FakeController : BaseCommandController
        {
            public override string Name => throw new NotImplementedException();

            public override string Info => throw new NotImplementedException();

            [ControllerMethod("fake")]
            public void FakeMethod(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
            {
                SentOut(fullNameToOption["--option1"].GetArguments().First());
                SentOut(fullNameToOption["--option2"].GetArguments().First());
            }

            [ControllerGetOption("fake")]
            public IEnumerable<Option> FakeMethodOption()
            {
                var optionList = new List<Option>();
                optionList.Add(new Option("-o1", "--option1", 1));
                optionList.Add(new Option("-o2","--option2",1));
                return optionList;
            }
        }
    }

}
