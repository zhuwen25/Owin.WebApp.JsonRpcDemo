using System;
using System.Collections.Generic;
using System.Threading;

namespace WindsorDemo.Interfaces
{
    public interface IHclMethodTranslation
    {
        IHclMethodTranslation AddHypervisorPlugin(string hypervisorPlugin);

        IHclMethodTranslation TranslateAndAddArgs(IEnumerable<object> hclArguments);

        IHclMethodTranslation AddRawArgs(params object[] rawArguments);

        IHclMethodTranslation AddConnectionDetail(IConnectionDetail connectionDetail);

        object Invoke(IConnectionDetail connectionDetail, Guid opId);

        object Invoke(IConnectionDetail connectionDetail, CancellationToken cancellationToken, Guid opId);

        //IHclMethodTranslation AddConnectionDetails(IConnectionDetails connectionDetails);
        //object Invoke(IRemoteHclConnection remoteHclConnection, System.Guid opId);
    }
}
