using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace ConsoleCommander.Interfaces
{
    public abstract class BaseCommandController : CommandBase
    {
        private readonly (string, MethodInfo)[] _controllerMethods;

        public BaseCommandController()
        {
            _controllerMethods = GetCurrentControllerMethods();
            AddOption(null, null, 0, int.MaxValue);
        }


        protected override void Invoke(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
        {
            var firstArg = funcArguments.FirstOrDefault();
            var (methodName, properMethod) = _controllerMethods.Where(name_method => firstArg != null && name_method.Item1 == firstArg).FirstOrDefault();

            if (properMethod == null)
            {
                var mainMethod = _controllerMethods.FirstOrDefault(m => m.Item1 == ControllerMethodAttribute.MAIN_CONTROLLER_METHOD);
                if (mainMethod.Item2 == null)
                    throw new Exception("There is not controller method to call");
                mainMethod.Item2.Invoke(this, new object[] { funcArguments, fullNameToOption });
                return;
            }

            properMethod.Invoke(this, new object[] { funcArguments.Skip(1), fullNameToOption });
        }


        public override IEnumerable<Option> GetPossibleOptions(IEnumerable<string> args)
        {
            var firstArg = args.First();
            var getOptionMethod = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m =>
                {
                    var atr = m.GetCustomAttribute(typeof(ControllerGetOptionAttribute));
                    if (atr != null && (atr as ControllerGetOptionAttribute).ControllerName == firstArg)
                        return true;
                    return false;
                });
            if (getOptionMethod.Count() > 1)
                throw new Exception("There is one possible get option method for controller method.");

            var optMethod = getOptionMethod.FirstOrDefault();
            if (getOptionMethod == null)
                throw new Exception("Get option is required for command controller method.");

            AddOptions((IEnumerable<Option>)optMethod.Invoke(this, new object[] { }));
            return base.GetPossibleOptions(args);
        }


        private (string, MethodInfo)[] GetCurrentControllerMethods()
        {
            var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                  .Where(m => m.GetCustomAttribute(typeof(ControllerMethodAttribute)) != null)
                  .Select(m =>
                  {
                      var atr = (ControllerMethodAttribute)m.GetCustomAttribute(typeof(ControllerMethodAttribute));
                      return (atr.Name, m);
                  })
                  .ToArray();

            if (methods.GroupBy(co => co.Item1).Count() < methods.Length)
                throw new Exception("Controller can't contain methods with same names.");

            return methods;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerMethodAttribute : Attribute
    {
        public const string MAIN_CONTROLLER_METHOD = "MAIN_CONTROLLER_METHOD";

        public readonly string Name;

        public ControllerMethodAttribute()
        {
            Name = MAIN_CONTROLLER_METHOD;
        }

        public ControllerMethodAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerGetOptionAttribute : Attribute
    {
        public const string MAIN_CONTROLLER_METHOD = "MAIN_CONTROLLER_METHOD";

        public readonly string ControllerName;

        public ControllerGetOptionAttribute()
        {
            ControllerName = MAIN_CONTROLLER_METHOD;
        }

        public ControllerGetOptionAttribute(string name)
        {
            ControllerName = name;
        }
    }
}
