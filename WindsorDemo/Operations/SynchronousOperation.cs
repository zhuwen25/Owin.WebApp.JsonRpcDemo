using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Selector
{
    public class SynchronousOperation : IHclOperation
    {
        private readonly MethodInfo _hclMethod;
        private readonly string _hypervisorPlugin;
        private readonly IHclMethodTranslationFactory _hclMethodTranslationFactory;
        private readonly IConnectionDetail _connectionDetail;
        private readonly Guid _hclOperationId;
        private readonly CancellationToken _cancellationToken;
        private readonly IList<object> _hclArguments;

        public SynchronousOperation(MethodInfo hclMethod, IList<object> hclArguments, string hypervisorPlugin, IConnectionDetail connectionDetail,
            IHclMethodTranslationFactory hclMethodTranslationFactory,CancellationToken? cancellationToken)
        {
            _hclMethod = hclMethod;
            _hclArguments = hclArguments;
            _hypervisorPlugin = hypervisorPlugin;
            _connectionDetail = connectionDetail;
            _hclMethodTranslationFactory = hclMethodTranslationFactory;
            _hclOperationId = Guid.NewGuid();
            _cancellationToken = cancellationToken?? CancellationToken.None;
        }

        public object Execute()
        {
            var tanslation = _hclMethodTranslationFactory.Create(_hclMethod)
                .AddHypervisorPlugin(_hypervisorPlugin)
                .TranslateAndAddArgs(_hclArguments);
            return tanslation.Invoke(_connectionDetail, _cancellationToken, _hclOperationId);
            //throw new System.NotImplementedException();
        }
    }
}
