using System.Threading.Tasks;
using WindsorDemo.Interfaces;

namespace WindsorDemo.RemoteHclOperation
{
    public interface IRemoteHclTranslation
    {
        Task<object> InvokeAsync(IRemoteHclConnection remoteHclConnection);
        IRemoteHclTranslation AddHypervisorPlugin(string hypervisorPlugin);
        IRemoteHclTranslation AddConnectionDetail(IConnectionDetail connectionDetail);
        IRemoteHclTranslation ConvertRawArgs(params object[] rawArguments);
    }
}
