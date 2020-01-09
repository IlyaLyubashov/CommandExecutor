using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleCommander.Utils
{
    internal static class DeepCopy
    {
        //впринципе лучше это через бинарную сериализацию сделать
        public static T Make<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
