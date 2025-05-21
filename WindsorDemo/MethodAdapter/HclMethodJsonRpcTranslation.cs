using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using WindsorDemo.Interfaces;
using WindsorDemo.JsonRpcRequest;

namespace WindsorDemo.Services
{
    public class HclMethodJsonRpcTranslation : IHclMethodTranslation
    {
        private readonly IHclJsonRpcAdapter _hclJsonRpcAdapter;//in charge of converting HCL object to JSON-RPC Object
        private readonly IHclJsonRpcMethodMap  _hclJsonRpcMethodMap; //in charge of mapping HCL method to JSON-RPC method
        private readonly MethodInfo _hclMethod; //HCL method
        private readonly List<object> _jsonRpcArguments = new List<object>(); //JSON-RPC arguments 实参

        private MethodInfo _jsonRpcMethod; //JSON-RPC method
        private ParameterInfo[] _jsonRcpParams; //JSON-RPC parameters 形参
        private int _jsonRpcArgToParamOffset; //JSON-RPC arguments to parameters offset
        private bool _isJsonRpcMapped; //JSON-RPC method mapped


        /// <summary>
        ///
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="hclJsonRpcAdapter"></param>
        /// <param name="hclJsonRpcMethodMap"></param>
        public HclMethodJsonRpcTranslation(MethodInfo methodInfo ,IHclJsonRpcAdapter hclJsonRpcAdapter,IHclJsonRpcMethodMap hclJsonRpcMethodMap)
        {
            this._hclMethod = methodInfo;
            this._hclJsonRpcAdapter = hclJsonRpcAdapter;
            this._hclJsonRpcMethodMap = hclJsonRpcMethodMap;
            _isJsonRpcMapped = false;
        }

        private void MapJsonRpc()
        {
            if (!_isJsonRpcMapped)
            {
                _jsonRpcArgToParamOffset = 0;
                _jsonRpcMethod = _hclJsonRpcMethodMap.GetJsonRpcMethod(_hclMethod);
                this._jsonRcpParams = _jsonRpcMethod.GetParameters();

                 var firstReq =  _jsonRcpParams.FirstOrDefault();
                //
                if (firstReq == null || firstReq.ParameterType.IsSubclassOf(typeof(HclBaseRequest)))
                {
                    throw new ArgumentException();
                }

                var firstArgument = Activator.CreateInstance(_jsonRcpParams[0].ParameterType);
                _jsonRpcArguments.Add(firstArgument);
                _jsonRpcArgToParamOffset++;
                _isJsonRpcMapped = true;
            }
        }

        private bool SetFirstRequestProperty(Type subClassof, string propertyName, object value)
        {
            var firstReq =  _jsonRpcArguments.FirstOrDefault();
            if (firstReq == null || !firstReq.GetType().IsSubclassOf(subClassof))
            {
                return false;
            }

            PropertyInfo[] properties = firstReq.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var property = properties.FirstOrDefault(p => p.Name == propertyName);
            if (property != null && property.CanWrite)
            {
                property?.SetValue(firstReq, value);
            }
            return true;
        }

        public IHclMethodTranslation AddHypervisorPlugin(string hypervisorPlugin)
        {
            MapJsonRpc();
            SetFirstRequestProperty(typeof(HclBasePluginRequest), nameof(HclBasePluginRequest.PluginName), hypervisorPlugin);
            return this;
        }

        public IHclMethodTranslation TranslateAndAddArgs(IEnumerable<object> hclArguments)
        {
            MapJsonRpc();
            foreach(var hclArg in hclArguments)
            {
                var jsonRpcArg = _hclJsonRpcAdapter.ToJsonRpcObject(hclArg, _jsonRcpParams[_jsonRpcArgToParamOffset++].ParameterType);
                _jsonRpcArguments.Add(jsonRpcArg);
            }

            return this;
        }

        public IHclMethodTranslation AddRawArgs(params object[] rawArguments)
        {
            MapJsonRpc();
            // _jsonRpcArguments.AddRange(rawArguments);
            // _jsonRpcArgToParamOffset += rawArguments.Length;
            return this;
        }

        public IHclMethodTranslation AddConnectionDetail(IConnectionDetail connectionDetail)
        {
            MapJsonRpc();
            SetFirstRequestProperty(typeof(HclHostingConnectionRequest), nameof(HclHostingConnectionRequest.ConnectionDetails), connectionDetail);
            return this;
        }

        public object Invoke(IConnectionDetail connectionDetail, Guid opId)
        {
            throw new NotImplementedException();
        }

        public object Invoke(IConnectionDetail connectionDetail, CancellationToken cancellationToken, Guid opId)
        {
            throw new NotImplementedException();
        }
    }
}
