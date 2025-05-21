using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class HclMethodToWcfTranslation: IHclMethodTranslation
    {
        private readonly IHclWcfAdapter hclWcfAdapter;
        private readonly IHclWcfMethodMap hclWcfMethodMap;
        private readonly MethodInfo hclMethod;
        private readonly List<object> wcfArguments = new List<object>();

        private MethodInfo wcfMethod;
        private ParameterInfo[] wcfParams;
        private int wcfArgToParamOffset;
        private bool isWcfMapped;

        private void MapWcf()
        {
            if (!isWcfMapped)
            {
                wcfMethod = hclWcfMethodMap.GetWcfMethod(hclMethod);
                this.wcfParams = wcfMethod.GetParameters();
                wcfArgToParamOffset = 0;
                isWcfMapped = true;
            }
        }

        public HclMethodToWcfTranslation(MethodInfo methodInfo ,IHclWcfAdapter hclWcfAdapter,IHclWcfMethodMap hclWcfMethodMap)
        {
            this.hclMethod = methodInfo;
            this.hclWcfAdapter = hclWcfAdapter;
            this.hclWcfMethodMap = hclWcfMethodMap;
            isWcfMapped = false;
        }

        public IHclMethodTranslation AddHypervisorPlugin(string hypervisorPlugin)
        {
            if (wcfParams.FirstOrDefault()?.Name == "hypervisorPlugin")
            {
                wcfArguments.Insert(0,hypervisorPlugin);
                wcfArgToParamOffset++;
            }
            return this;
        }

        public IHclMethodTranslation TranslateAndAddArgs(IEnumerable<object> hclArguments)
        {
            MapWcf();
            foreach (var hclArg  in hclArguments)
            {
                var wcfArg = hclWcfAdapter.ToWcfObject(hclArg,wcfParams[wcfArgToParamOffset++].ParameterType);
                wcfArguments.Add(wcfArg);
            }

            return this;
        }

        public IHclMethodTranslation AddRawArgs(params object[] rawArguments)
        {
            MapWcf();
            wcfArguments.AddRange(rawArguments);
            wcfArgToParamOffset += rawArguments.Length;
            return this;
        }

        public IHclMethodTranslation AddConnectionDetail(IConnectionDetail connectionDetail)
        {
            wcfArguments.Add(hclWcfAdapter.ToWcfObject(connectionDetail,typeof(IConnectionDetail)));
            return this;
        }

        private object DynamicInvoke(IHypervisorCommunicationsLibraryInterface wcf)
        {
            try
            {
                return wcfMethod.Invoke(wcf, wcfArguments.ToArray());
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        public object Invoke(IConnectionDetail connectionDetail, Guid opId)
        {
            MapWcf();
            if (wcfMethod.ReturnType == typeof(void))
            {
                DynamicInvoke(new HypervisorCommunicationsLibraryService());
                return null;
            }

            return null;
        }

        public object Invoke(IConnectionDetail connectionDetail, CancellationToken cancellationToken, Guid opId)
        {
            throw new NotSupportedException();
        }
    }
}
