using System;
using System.Collections.Generic;

namespace WindsorDemo.Services
{
    public class HclTypeHelper
    {
        public static bool isString(Type t)
        {
            return (t == typeof(string));
        }

        public static bool isString(object o)
        {
            return HclTypeHelper.isString(o.GetType());
        }

        public static bool isEnum(Type t)
        {
            return t.IsEnum;
        }

        public static bool isEnum(object o)
        {
            return HclTypeHelper.isEnum(o.GetType());
        }

        public static bool isArray(Type t)
        {
            return t.IsArray;
        }

        public static bool isArray(object o)
        {
            return HclTypeHelper.isArray(o.GetType());
        }

        public static bool isList(Type t)
        {
            return (t.GetInterface(nameof(System.Collections.IList)) != null);
        }

        public static bool isList(object o)
        {
            return HclTypeHelper.isList(o.GetType());
        }

        public static bool isCollection(Type t)
        {
            return (t.GetInterface(nameof(System.Collections.ICollection)) != null);
        }

        public static bool isCollection(object o)
        {
            return HclTypeHelper.isCollection(o.GetType());
        }

        public static bool isEnumberable(Type t)
        {
            return (t.GetInterface(nameof(System.Collections.IEnumerable)) != null);
        }

        public static bool isEnumberable(object o)
        {
            return HclTypeHelper.isEnumberable(o.GetType());
        }

        // public static bool isIHypervisorAlerts(Type t)
        // {
        //     return ((t.GetInterface(nameof(IHypervisorAlerts)) != null) || (t == typeof(IHypervisorAlerts)));
        // }
        //
        // public static bool isIHypervisorAlerts(object o)
        // {
        //     return HclTypeHelper.isIHypervisorAlerts(o.GetType());
        // }

        // public static bool isIImagePrepVMDiagnosticsResult(Type t)
        // {
        //     return ((t.GetInterface(nameof(IImagePrepVMDiagnosticsResult)) != null) ||
        //             (t == typeof(IImagePrepVMDiagnosticsResult)));
        // }

        public static bool isIImagePrepVMDiagnosticsResult(object o)
        {
            return HclTypeHelper.isIImagePrepVMDiagnosticsResult(o.GetType());
        }

        // public static bool isIPreflightCheckResponse(Type t)
        // {
        //     return ((t.GetInterface(nameof(IPreflightCheckResponse)) != null) ||
        //             (t == typeof(IPreflightCheckResponse)));
        // }

        public static bool isIPreflightCheckResponse(object o)
        {
            return HclTypeHelper.isIPreflightCheckResponse(o.GetType());
        }

        // public static bool isIMachineSpecification(Type t)
        // {
        //     return ((t.GetInterface(nameof(IMachineSpecification)) != null) || (t == typeof(IMachineSpecification)));
        // }

        public static bool isIMachineSpecification(object o)
        {
            return HclTypeHelper.isIMachineSpecification(o.GetType());
        }

        public static bool isIDictionaryStringToString(Type t)
        {
            return ((t.GetInterface(nameof(IDictionary<string, string>)) != null) ||
                    (t == typeof(IDictionary<string, string>)));
        }

        public static bool isIDictionaryStringToString(object o)
        {
            return HclTypeHelper.isIDictionaryStringToString(o.GetType());
        }

        public static bool isIDictionaryStringToStringArray(Type t)
        {
            return ((t.GetInterface(nameof(IDictionary<string, string[]>)) != null) ||
                    (t == typeof(IDictionary<string, string[]>)));
        }

        public static bool isIDictionaryStringToStringArray(object o)
        {
            return HclTypeHelper.isIDictionaryStringToStringArray(o.GetType());
        }

        // public static bool isIDictionaryStringToMachineCapabilityArray(Type t)
        // {
        //     return (t.GetInterface(nameof(IDictionary<string, MachineCapability[]>)) != null)
        //            || (t == typeof(IDictionary<string, MachineCapability[]>))
        //            || (t.GetInterface(nameof(Dictionary<string, MachineCapability[]>)) != null)
        //            || (t == typeof(Dictionary<string, MachineCapability[]>));
        // }

        public static bool isIDictionaryStringToMachineCapabilityArray(object o)
        {
            return HclTypeHelper.isIDictionaryStringToMachineCapabilityArray(o.GetType());
        }

        public static bool isIDictionaryStringToIListString(Type t)
        {
            return ((t.GetInterface(nameof(IDictionary<string, IList<string>>)) != null) ||
                    (t == typeof(IDictionary<string, IList<string>>)));
        }

        public static bool isIDictionaryStringToIListString(object o)
        {
            return HclTypeHelper.isIDictionaryStringToIListString(o.GetType());
        }

        // public static bool isIDictionaryStringToIListMachineCapability(Type t)
        // {
        //     return (t.GetInterface(nameof(IDictionary<string, IList<MachineCapability>>)) != null) ||
        //            (t == typeof(IDictionary<string, IList<MachineCapability>>));
        // }

        public static bool isIDictionaryStringToIListMachineCapability(object o)
        {
            return HclTypeHelper.isIDictionaryStringToIListMachineCapability(o.GetType());
        }

        public static bool isIDictionaryIntToBool(Type t)
        {
            return ((t.GetInterface(nameof(IDictionary<int, bool>)) != null) || (t == typeof(IDictionary<int, bool>)));
        }

        public static bool isIDictionaryIntToBool(object o)
        {
            return HclTypeHelper.isIDictionaryIntToBool(o.GetType());
        }

        // public static bool isIDictionaryIntToIDiskImage(Type t)
        // {
        //     return ((t.GetInterface(nameof(IDictionary<int, IDiskImage>)) != null) ||
        //             (t == typeof(IDictionary<int, IDiskImage>)));
        // }

        public static bool isIDictionaryIntToIDiskImage(object o)
        {
            return HclTypeHelper.isIDictionaryIntToIDiskImage(o.GetType());
        }

        // public static bool isIDictionaryStringToIVMPriceDetails(Type t)
        // {
        //     return ((t.GetInterface(nameof(IDictionary<string, IVMPriceDetails>)) != null) ||
        //             (t == typeof(IDictionary<string, IVMPriceDetails>)));
        // }

        public static bool isIDictionaryStringToIVMPriceDetails(object o)
        {
            return HclTypeHelper.isIDictionaryStringToIVMPriceDetails(o.GetType());
        }

        public static bool isIDictionaryStringToObject(Type t)
        {
            return ((t.GetInterface(nameof(IDictionary<string, object>)) != null) ||
                    (t == typeof(IDictionary<string, object>)));
        }

        public static bool isIDictionaryStringToObject(object o)
        {
            return HclTypeHelper.isIDictionaryStringToObject(o.GetType());
        }
    }
}
