using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WindsorDemo.Interfaces;
using WindsorDemo.JsonRpcRequest;

namespace WindsorDemo.Services
{
    public class HclMethodJsonRpcTranslation : IHclMethodTranslation
    {
        private readonly IHclJsonRpcAdapter _hclJsonRpcAdapter; //in charge of converting HCL object to JSON-RPC Object
        private readonly IHclJsonRpcMethodMap _hclJsonRpcMethodMap; //in charge of mapping HCL method to JSON-RPC method
        private readonly MethodInfo _hclMethod; //HCL method
        private readonly ParameterInfo[] _hclParameters;

        private readonly List<object> _jsonRpcArguments = new List<object>(); //JSON-RPC arguments 实参
        private readonly IJsonRpcClientService _jsonRpcClientService;


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
        public HclMethodJsonRpcTranslation(MethodInfo methodInfo, IHclJsonRpcAdapter hclJsonRpcAdapter,
            IHclJsonRpcMethodMap hclJsonRpcMethodMap, IJsonRpcClientService jsonRpcClientService)
        {
            this._hclMethod = methodInfo;
            _hclParameters = methodInfo.GetParameters();
            this._hclJsonRpcAdapter = hclJsonRpcAdapter;
            this._hclJsonRpcMethodMap = hclJsonRpcMethodMap;
            _isJsonRpcMapped = false;
            _jsonRpcClientService = jsonRpcClientService;
        }

        private void MapJsonRpc()
        {
            if (!_isJsonRpcMapped)
            {
                _jsonRpcArgToParamOffset = 0;
                _jsonRpcMethod = _hclJsonRpcMethodMap.GetJsonRpcMethod(_hclMethod);
                this._jsonRcpParams = _jsonRpcMethod.GetParameters();

                var firstReq = _jsonRcpParams.FirstOrDefault();
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
            var firstReq = _jsonRpcArguments.FirstOrDefault();
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
            SetFirstRequestProperty(typeof(HclBasePluginRequest), nameof(HclBasePluginRequest.PluginName),
                hypervisorPlugin);
            return this;
        }

        public IHclMethodTranslation TranslateAndAddArgs(IEnumerable<object> hclArguments)
        {
            MapJsonRpc();
            CancellationToken cancellationToken = CancellationToken.None;

            _jsonRpcArguments.Add(cancellationToken);

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
            SetFirstRequestProperty(typeof(HclHostingConnectionRequest),
                nameof(HclHostingConnectionRequest.ConnectionDetails), connectionDetail);
            return this;
        }

        public object Invoke(IConnectionDetail connectionDetail, Guid opId)
        {
            throw new NotSupportedException();
        }

        public async Task<object> InvokeAsync(IConnectionDetail connectionDetail, CancellationToken cancellationToken)
        {
            MapJsonRpc(); // Ensures _jsonRpcMethod and _jsonRcpParams are initialized.

            Func<IRemoteHcl, CancellationToken, Task<object>> operation = async (remoteHclClient, ct) =>
            {
                object[] finalArgs;
                List<object> currentArgumentsList = new List<object>(_jsonRpcArguments);

                // Check if the target RPC method expects a CancellationToken as its last parameter
                // and if it needs to be added.
                if (_jsonRcpParams.Length > 0 && _jsonRcpParams.Last().ParameterType == typeof(CancellationToken))
                {
                    if (currentArgumentsList.Count == _jsonRcpParams.Length - 1)
                    {
                        // CancellationToken is expected and not yet in the list, add it.
                        currentArgumentsList.Add(ct);
                        finalArgs = currentArgumentsList.ToArray();
                    }
                    else if (currentArgumentsList.Count == _jsonRcpParams.Length)
                    {
                        // Arguments count matches parameters count.
                        // Assume CancellationToken is either already correctly placed as the last argument
                        // or the method signature is being matched as is.
                        // If last arg in currentArgumentsList is a CT, it might be one set by user.
                        // Forcing the 'ct' from lambda might be desired for consistency.
                        // For now, we trust the arguments are complete.
                        finalArgs = currentArgumentsList.ToArray();
                    }
                    else
                    {
                        throw new TargetParameterCountException(
                            $"Argument count mismatch for JSON-RPC method '{_jsonRpcMethod.Name}'. Expected {_jsonRcpParams.Length - 1} data arguments + CancellationToken, or {_jsonRcpParams.Length} arguments if CancellationToken is already included. Got {currentArgumentsList.Count}.");
                    }
                }
                else // Target method does not take a CancellationToken as its last parameter
                {
                    if (currentArgumentsList.Count != _jsonRcpParams.Length)
                    {
                        throw new TargetParameterCountException(
                            $"Argument count mismatch for JSON-RPC method '{_jsonRpcMethod.Name}'. Expected {_jsonRcpParams.Length} arguments, got {currentArgumentsList.Count}.");
                    }

                    finalArgs = currentArgumentsList.ToArray();
                }

                // Invoke the actual JSON-RPC method on the remoteHclClient.
                object taskAsObject = _jsonRpcMethod.Invoke(remoteHclClient, finalArgs);

                if (taskAsObject == null)
                {
                    throw new InvalidOperationException(
                        $"Invocation of JSON-RPC method '{_jsonRpcMethod.Name}' returned null, but a Task was expected.");
                }

                if (!(taskAsObject is Task task))
                {
                    throw new InvalidOperationException(
                        $"Invocation of JSON-RPC method '{_jsonRpcMethod.Name}' did not return a Task. Returned type: {taskAsObject.GetType().FullName}");
                }

                await task.ConfigureAwait(false); // Await the task.

                // If the task has a result (i.e., it's Task<T>), return the result (boxed as object).
                // Otherwise (it's a non-generic Task), return null.
                Type taskType = task.GetType();
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // It's Task<TResult>. Get the TResult value.
                    // Using dynamic is a concise way to access the 'Result' property.
                    // Alternatively, use reflection: taskType.GetProperty("Result").GetValue(task);
                    //return ((dynamic)task).Result;
                    return taskType.GetProperty("Result")?.GetValue(task);
                }
                else
                {
                    // It's a non-generic Task (effectively Task<void>).
                    // The operation is complete; return null as the object result.
                    return null;
                }
            };

            // Use the _jsonRpcClientService to execute the operation.
            // CallAsync<object> is used because our 'operation' Func returns Task<object>.
            // The actual result type T from Task<T> (if any) will be boxed into object.
            return await _jsonRpcClientService.CallAsync<object>(operation, cancellationToken).ConfigureAwait(false);
        }
    }
}
