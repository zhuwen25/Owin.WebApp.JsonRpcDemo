using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using WindsorDemo.Interfaces;

namespace WindsorDemo.RemoteHclOperation
{
    public class SyncToAsyncOperations: IHclOperation
    {
        private readonly MethodInfo _hclMethod;
        private readonly string _hypervisorPlugin;
        private readonly IHclMethodTranslationFactory _hclMethodTranslationFactory;
        private readonly IConnectionDetail _connectionDetail;
        private readonly Guid _hclOperationId;
        private readonly CancellationToken _cancellationToken;
        private readonly IList<object> _hclArguments;
        private readonly IJsonRpcClientService _jsonRpcClientService;

        public SyncToAsyncOperations( MethodInfo hclMethod, IList<object> hclArguments, string hypervisorPlugin, IConnectionDetail connectionDetail,
            IHclMethodTranslationFactory hclMethodTranslationFactory , CancellationToken ? cancellationToken = null,IJsonRpcClientService jsonRpcClientService = null)
        {
            _hclMethod = hclMethod;
            _hclArguments = hclArguments;
            _hypervisorPlugin = hypervisorPlugin;
            _connectionDetail = connectionDetail;
            _hclMethodTranslationFactory = hclMethodTranslationFactory;
            _jsonRpcClientService = jsonRpcClientService;
            _hclOperationId = Guid.NewGuid();
            _cancellationToken = cancellationToken?? CancellationToken.None;
        }

        public object Execute()
        {
            var tanslation = _hclMethodTranslationFactory.Create(_hclMethod)
                .AddHypervisorPlugin(_hypervisorPlugin)
                .TranslateAndAddArgs(_hclArguments);
            if (_connectionDetail != null)
            {
                tanslation.AddConnectionDetail(_connectionDetail);
            }

            return  tanslation.InvokeAsync(_connectionDetail, _cancellationToken ).GetAwaiter().GetResult();
        }
    }
}
