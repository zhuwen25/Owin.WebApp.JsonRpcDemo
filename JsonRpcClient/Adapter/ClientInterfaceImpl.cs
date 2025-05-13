using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.VisualStudio.Threading;
using System.Reflection;

namespace JsonRpcClient.Adapter.Interfaces;

public class HypervisorCommunicationsLibrary: IHypervisorCommunicationsLibraryInterface
{
    public string SayHello(string name)
    {
        return $"Hello {name} from HypervisorCommunicationsLibrary";
    }

    public int GetStatus(int id)
    {
        Console.WriteLine($"HypervisorCommunicationsLibrary GetStatus: {id}");
        return id;
    }

    public string GetVersion(string versionIn)
    {
        return $"HypervisorCommunicationsLibrary version: {versionIn}";
    }
}

public class RemoteHcl : IRemoteHcl
{
    public async Task<string> SayHelloAsync(string name,CancellationToken cancellationToken = default)
    {
      return await Task.FromResult($"Hello {name} from RemoteHcl");
    }
    public async Task<int> GetStatusAsync(JsonRpcRequest<int> request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"HypervisorCommunicationsLibrary GetStatus: {request.Request}");
        return await Task.FromResult(request.Request);
    }

    public Task<JsonRpcResponse<string>> GetVersionAsync(JsonRpcRequest<string> request, CancellationToken cancellationToken = default)
    {
        var stringResponse = new JsonRpcResponse<string>($"RemoteHcl Version: {request.Request}");

        return Task.FromResult(stringResponse);
    }
}

public class WcfClientService : IWcfClientService
{
    public T Call<T>(Func<IHypervisorCommunicationsLibraryInterface, T> operation)
    {
        var hcl = new HypervisorCommunicationsLibrary();
        return operation(hcl);
    }
}
public class JsonRpcClientService:IJsonRpcClientService
{
    public async Task<T> CallAsync<T>(Func<IRemoteHcl, Task<T>> operation,CancellationToken cancellationToken = default)
    {
        // Call the operation
        return await operation(new RemoteHcl());
    }
}


public class HclAdapter : IHclAdapter
{
    private readonly IWcfClientService _wcfClientService;
    private readonly IJsonRpcClientService _jsonRpcClientService;

    public HclAdapter(IWcfClientService wcfClientService, IJsonRpcClientService jsonRpcClientService)
    {
        _wcfClientService = wcfClientService;
        _jsonRpcClientService = jsonRpcClientService;
    }

    public string SayHello(string name)
    {
        return _wcfClientService.Call(hcl => hcl.SayHello(name));
    }

    public async Task<string> SayHelloAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _jsonRpcClientService.CallAsync(hcl => hcl.SayHelloAsync(name,cancellationToken), cancellationToken);
    }

    public TResult Execute<TResult>(Func<IHypervisorCommunicationsLibraryInterface, TResult> operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));
        Console.WriteLine("AdvancedAdapter: Dispatching to synchronous WCF service.");
        return _wcfClientService.Call(operation);
    }

    public Task<TResult> Execute<TResult>(Func<IRemoteHcl, Task<TResult>> operation,CancellationToken cancellationToken = default)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));
        Console.WriteLine("AdvancedAdapter: Dispatching to asynchronous JSON-RPC service.");
        return _jsonRpcClientService.CallAsync(operation,cancellationToken);
    }
}

public class HclToRemoteHclInterceptor: IInterceptor
{
    private readonly IRemoteHcl _remoteHcl;
    private readonly Dictionary<string, MethodInfo> _methodMap;
    private readonly JoinableTaskFactory _joinableTaskFactory;

    public HclToRemoteHclInterceptor(IRemoteHcl remoteHcl,JoinableTaskFactory joinableTaskFactory)
    {
        _remoteHcl = remoteHcl ?? throw new ArgumentNullException(nameof(remoteHcl));
        _joinableTaskFactory = joinableTaskFactory ?? throw new ArgumentNullException(nameof(joinableTaskFactory));
        _methodMap = BuildMethodMap();
    }

    private Dictionary<string, string> NotStandardMethodMap()
    {
        return new Dictionary<string, string>()
        {
            { nameof(IHypervisorCommunicationsLibraryInterface.SayHello), nameof(IRemoteHcl.SayHelloAsync) },
            { nameof(IHypervisorCommunicationsLibraryInterface.GetStatus), nameof(IRemoteHcl.GetStatusAsync) },
            { nameof(IHypervisorCommunicationsLibraryInterface.GetVersion), nameof(IRemoteHcl.GetVersionAsync) },
        };
    }

    // Build a map of sync method names to async MethodInfo
    private Dictionary<string, MethodInfo> BuildMethodMap()
    {
        var map = new Dictionary<string, MethodInfo>();
        var syncMethods = typeof(IHypervisorCommunicationsLibraryInterface).GetMethods();
        var asyncMethods = typeof(IRemoteHcl).GetMethods();
        var nameMap = NotStandardMethodMap();

        foreach (var syncMethod in syncMethods)
        {
            // Assume async method name is sync method name + "Async"
            string asyncMethodName = syncMethod.Name + "Async";
            var asyncMethod = Array.Find(asyncMethods, m => m.Name == asyncMethodName);
            if (asyncMethod != null)
            {
                map[syncMethod.Name] = asyncMethod;
            }
        }
        return map;
    }

    public void Intercept(IInvocation invocation)
    {
        MethodInfo syncMethod = invocation.Method;
        //Check if the method is in the map
        if (_methodMap.TryGetValue(syncMethod.Name, out var asyncMethod))
        {
            //Prepare arguments for the async method
            var parameters = asyncMethod.GetParameters();
            var args = new object[parameters.Length];
            Array.Copy(invocation.Arguments,args, invocation.Arguments.Length);
            if (parameters.Length > invocation.Arguments.Length &&
                parameters[^1].ParameterType == typeof(CancellationToken))
            {
                args[^1] = CancellationToken.None;
            }

            try
            {

                //Parameters for the async method converted to the async method parameters
                // Map parameters based on method
                if (syncMethod.Name == nameof(IHypervisorCommunicationsLibraryInterface.GetStatus))
                {
                    // Map int id to JsonRpcRequest<int>
                    int id = (int)invocation.Arguments[0];
                    args[0] = new JsonRpcRequest<int>(id);
                }
                else if (syncMethod.Name == nameof(IHypervisorCommunicationsLibraryInterface.GetVersion))
                {
                    // Map string versionIn to JsonRpcRequest<string>
                    string versionIn = (string)invocation.Arguments[0];
                    args[0] = new JsonRpcRequest<string>(versionIn);
                }
                else
                {
                    // Default: Copy arguments as-is (e.g., for SayHello)
                    Array.Copy(invocation.Arguments, args, invocation.Arguments.Length);
                }



                //Invoke the async method and block to get the result
                if (asyncMethod.ReturnType == typeof(Task))
                {
                    _joinableTaskFactory.Run(async () =>
                    {
                        await ((Task)asyncMethod.Invoke(_remoteHcl, args)!).ConfigureAwait(false);
                    });
                    invocation.ReturnValue = null;
                }
                else  if (asyncMethod.ReturnType.IsGenericType && asyncMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    //For Task<T> methods, we need to extract the result
                    var result = _joinableTaskFactory.Run(async () =>
                    {
                        var task = (Task)asyncMethod.Invoke(_remoteHcl, args)!;
                        await task.ConfigureAwait(false);
                        var taskResult = asyncMethod.ReturnType.GetProperty("Result")?.GetValue(task);
                        if (syncMethod.Name == nameof(IHypervisorCommunicationsLibraryInterface.GetVersion))
                        {
                            // Extract Response from JsonRpcResponse<string>
                            return typeof(JsonRpcResponse<string>).GetProperty("Response")?.GetValue(taskResult);
                        }
                        return taskResult;
                    });

                    // Special handling for JsonRpcResponse<T>


                    invocation.ReturnValue = result;
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected return type: {asyncMethod.ReturnType}");
                }
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex; // Unwrap reflection exceptions
            }
        }
        else
        {
            // Unmapped method: proceed with original implementation
            invocation.Proceed();
        }

    }
}

public class TestHclClientWithWindsorCastel()
{
    public static void TestTclCallConverter()
    {
        var container = new WindsorContainer();

        container.Register(Component.For<JoinableTaskFactory>()
            .Instance(new JoinableTaskFactory(new JoinableTaskContext()))
            .LifestyleSingleton());

        // Register the WCF client service
        container.Register(Component.For<IRemoteHcl>()
            .ImplementedBy<RemoteHcl>()
            .LifestyleTransient());

        container.Register(Component.For<HclToRemoteHclInterceptor>()
            .LifestyleTransient());


        container.Register(Component.For<IHypervisorCommunicationsLibraryInterface>()
        .ImplementedBy<HypervisorCommunicationsLibrary>()
        .Interceptors<HclToRemoteHclInterceptor>()
        .LifestyleTransient());


        // Test the setup
         var service = container.Resolve<IHypervisorCommunicationsLibraryInterface>();
         Console.WriteLine(service.SayHello("Alice")); // Outputs: Hello Alice from RemoteHcl
         Console.WriteLine(service.GetStatus(42));     // Outputs: (e.g., 42 from RemoteHcl)
         Console.WriteLine(service.GetVersion("new-config"));           // Calls UpdateConfigAsync
        //
        // container.Dispose();

    }


    public static  void TestHcl()
    {
        var wcfClientService = new WcfClientService();
        var jsonRpcClientService = new JsonRpcClientService();
        var hclAdapter = new HclAdapter(wcfClientService, jsonRpcClientService);

        // Call the synchronous method
        string syncResult = hclAdapter.SayHello("World");
        Console.WriteLine(syncResult); // Output: Hello World from HypervisorCommunicationsLibrary

        // Call the asynchronous method
        string asyncResult = hclAdapter.SayHelloAsync("World").GetAwaiter().GetResult();
        Console.WriteLine(asyncResult); // Output: Hello World from RemoteHcl

        Console.WriteLine("=======================Using Execute method:=========================");

        // Call the synchronous method using Execute
        string executeSyncResult = hclAdapter.Execute(hcl => hcl.SayHello("World"));
        Console.WriteLine(executeSyncResult); // Output: Hello World from HypervisorCommunicationsLibrary

        // Call the asynchronous method using Execute
        string executeAsyncResult = hclAdapter.Execute(hcl => hcl.SayHelloAsync("World"), CancellationToken.None).GetAwaiter().GetResult();
        Console.WriteLine(executeAsyncResult); // Output: Hello World from RemoteHcl

    }
}
