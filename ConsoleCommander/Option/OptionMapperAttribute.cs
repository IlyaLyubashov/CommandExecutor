using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace ConsoleCommander
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionMapProp : Attribute
    {
        public readonly string OptName;

        public OptionMapProp(string optName)
        {
            OptName = optName;
        }

    }


    public class MappedCommandSettings
    {

        public MappedCommandSettings(IEnumerable<Option> opts)
        {
            MapProperties(opts);
        }

        private void MapProperties(IEnumerable<Option> opts)
        {
            var t = GetType();
            var props = t.GetProperties().Select(p => ( p, p.GetCustomAttribute(typeof(OptionMapProp))));

            foreach (var opt in opts)
            {
                if (IsMapped(props, opt.FullName, out PropertyInfo prop))
                    prop?.TryMapValue(this, opt);
            }
        }

        private bool IsMapped(IEnumerable<(PropertyInfo,Attribute)> props, string optionFullName, out PropertyInfo posProp)
        {
            
            posProp = null;

            if (optionFullName == null)
                return false;
            var possibleMapName = optionFullName.TrimStart('-').Split('-').Aggregate( (source,acc) => source + acc  );

            posProp = props.FirstOrDefault( propNattr =>
            {
                var prop = propNattr.Item1;
                var attr = propNattr.Item2 as OptionMapProp;
                if (prop.Name.ToLower() == possibleMapName || attr?.OptName == possibleMapName)
                    return true;
                return false;
            }).Item1;

            if (posProp != null)
                return true;

            return false;
        }
    }

    public static class PropertyInfoExtensionsForMapper
    {
        public static void TryMapValue(this PropertyInfo prop, object obj, Option option)
        {
            var optionValue = option.GetArguments().FirstOrDefault();
            var propType = prop.PropertyType;

            if (propType.FullName == typeof(string).FullName)
            {            
                prop.SetValue(obj, optionValue);
                return;
            }

            if (propType.FullName == typeof(bool).FullName)
            {
                if (option.IsOptionSet)
                    prop.SetValue(obj, true);
            }


            if (propType.FullName == typeof(int).FullName)
            {
                if (int.TryParse(optionValue, out int mean))
                    prop.SetValue(obj, mean);
            }

            if (propType.FullName == typeof(double).FullName)
            {
                if (double.TryParse(optionValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double mean))
                {
                    prop.SetValue(obj, mean);
                }
            }
        }
    }
}
