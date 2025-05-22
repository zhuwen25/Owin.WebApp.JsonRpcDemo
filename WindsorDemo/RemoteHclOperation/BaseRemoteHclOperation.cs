using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using WindsorDemo.Interfaces;
using WindsorDemo.JsonRpcRequest;

namespace WindsorDemo.RemoteHclOperation
{
    public class HclProgressUpdate
    {
        /// <summary>
        /// Progress percentage
        /// </summary>
        public int Percentage { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// Progress details
        /// the message of the progress to be displayed to the user
        /// </summary>
        public string Details { get; set; }
    }

    public abstract class BaseRemoteHclOperation<T> :IHclOperation  where T: HclBaseRequest, new()
    {
        protected readonly MethodInfo _hclMethod;
        protected readonly string _hypervisorPlugin;
        protected readonly IHclMethodTranslationFactory _hclMethodTranslationFactory;
        protected readonly IConnectionDetail _connectionDetail;
        protected readonly CancellationToken _cancellationToken;
        protected readonly IJsonRpcClientService _jsonRpcClientService;
        protected T _hclRequest;

        protected BaseRemoteHclOperation(MethodInfo hclMethod, IList<object> hclArguments, string hypervisorPlugin, IConnectionDetail connectionDetail,
            IHclMethodTranslationFactory hclMethodTranslationFactory, CancellationToken? cancellationToken = null, IJsonRpcClientService jsonRpcClientService = null)
        {
            _hclMethod = hclMethod;
            _hypervisorPlugin = hypervisorPlugin;
            _hclMethodTranslationFactory = hclMethodTranslationFactory;
            _connectionDetail = connectionDetail;
            _cancellationToken = cancellationToken ?? CancellationToken.None;
            _jsonRpcClientService = jsonRpcClientService?? throw new ArgumentNullException(nameof(jsonRpcClientService));
            _hclRequest = new T();
        }

        public virtual bool ConvertArgumentToJsonRpc( IList<object> hclArguments)
        {
            if (hclArguments == null || hclArguments.Count == 0)
            {
                throw new ArgumentException("Hcl arguments cannot be null or empty", nameof(hclArguments));
            }

            // Implement the logic to explain the arguments here
            // This is just a placeholder implementation
            foreach (var arg in hclArguments)
            {
                Console.WriteLine($"Argument: {arg}");
            }
            return true;
        }



        public virtual object Execute()=>
            throw new NotImplementedException("Execute method not implemented in the base class. Please override it in the derived class.");

    }
}
