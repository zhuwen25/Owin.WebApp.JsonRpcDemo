using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WindsorDemo.Interfaces
{
    public interface IHclMethodTranslation
    {
        IHclMethodTranslation AddHypervisorPlugin(string hypervisorPlugin);

        IHclMethodTranslation TranslateAndAddArgs(IEnumerable<object> hclArguments);

        IHclMethodTranslation AddRawArgs(params object[] rawArguments);

        IHclMethodTranslation AddConnectionDetail(IConnectionDetail connectionDetail);

        object Invoke(IConnectionDetail connectionDetail, Guid opId);

        Task<object> InvokeAsync(IConnectionDetail connectionDetail, CancellationToken cancellationToken);

    }
}
