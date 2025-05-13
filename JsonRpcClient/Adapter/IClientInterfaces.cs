namespace JsonRpcClient.Adapter.Interfaces;
public interface IHypervisorCommunicationsLibraryInterface
{
    string SayHello(string name);
    int GetStatus(int id);
    public string GetVersion(string versionIn);
}

public class JsonRpcRequest<T> {
  private readonly T _request;
  public T Request => _request;

  public JsonRpcRequest(T val)
  {
      _request = val;
  }
}

public class JsonRpcResponse<T>
{
    private T _response;

    public JsonRpcResponse(T response)
    {
        _response = response;
    }
    public T Response
    {
        get
        {
            return _response;
        }
        set
        {
            _response = value;
        }
    }
}

public interface IRemoteHcl
{
    Task<string> SayHelloAsync(string name, CancellationToken cancellationToken = default);
    Task<int> GetStatusAsync(JsonRpcRequest<int> request, CancellationToken cancellationToken = default);

    Task<JsonRpcResponse<string>> GetVersionAsync(JsonRpcRequest<string> request, CancellationToken cancellationToken = default);

}


public interface IWcfClientService
{
    T Call<T>(Func<IHypervisorCommunicationsLibraryInterface, T> operation);
}

public interface IJsonRpcClientService
{
    Task<T> CallAsync<T>(Func<IRemoteHcl, Task<T>> operation,CancellationToken cancellationToken = default);
}

public interface IHclAdapter
{
    string SayHello(string name);
    Task<string> SayHelloAsync(string name, CancellationToken cancellationToken = default);
}
