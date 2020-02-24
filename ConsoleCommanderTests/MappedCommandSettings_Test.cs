using System;
using System.Collections.Generic;
using System.Text;
using ConsoleCommander;
using NUnit.Framework;
using ConsoleCommander.Options;

namespace ConsoleCommanderTests
{
    [TestFixture]
    class MappedCommandSettings_Test
    {
        SomeOption obj;

        [SetUp]
        public void SetUp()
        {
            var opts = OptionsParser.Parse("-n Ilya -b -in 1 -dn 2.553 -s hello".Split(), new Option[]
                {
                    new Option("-n","--name",1,1),
                    new Option("-b","--isok",0),
                    new Option("-in","--intnum",1,1),
                    new Option("-dn","--doublenum",1,1),
                    new Option("-s","--shorty",1,1)
                });

            obj = new SomeOption(opts);
        }


        [Test]
        public void Map_Property_String()
        {
            Assert.That(obj.Name == "Ilya");
        }

        [Test]
        public void Map_Property_Bool()
        {
            Assert.That(obj.IsOk == true);
        }

        [Test]
        public void Map_Property_Int()
        {
            Assert.That(obj.IntNum == 1);
        }

        [Test]
        public void Map_Property_Double()
        {
            Assert.That(obj.DoubleNum == 2.553);
        }

        [Test]
        public void Map_Property_AttrSetName()
        {
            Assert.That(obj.SomeVeryLongAttribute == "hello");
        }

        class SomeOption : MappedCommandSettings
        {
            public string Name { get; set; }

            public bool IsOk { get; set; }

            public int IntNum { get; set; }

            public double DoubleNum { get; set; }

            [OptionMapProp("shorty")]
            public string SomeVeryLongAttribute { get; set; }

            public SomeOption(IEnumerable<Option> options) : base(options)
            {

            }
        }
    }
}
