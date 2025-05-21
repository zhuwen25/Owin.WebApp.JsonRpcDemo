using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class HclWcfAdapter : IHclWcfAdapter
    {
        public static object ConvertObjectUsingConverterMethods(object from, Type targetType)
        {
            if (from == null)
            {
                return null;
            }

            _ = targetType ?? throw new ArgumentNullException(nameof(targetType));
            return from;
            // return targetType.IsValueType
            //     ? Activator.CreateInstance(targetType)
            //     : Activator.CreateInstance(targetType, from);
        }

        public object ToHclObject(object wcfObject, Type targetType)
        {
            if (wcfObject == null)
            {
                return null;
            }

            _ = targetType ?? throw new ArgumentNullException(nameof(targetType));

            return ConvertObjectUsingConverterMethods(wcfObject, targetType);
        }

        public object ToWcfObject(object hclObject, Type targetType)
        {
            if (hclObject == null)
            {
                return null;
            }

            _ = targetType ?? throw new ArgumentNullException(nameof(targetType));
            return ConvertObjectUsingConverterMethods(hclObject, targetType);
        }


    }
}
