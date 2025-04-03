using WcfHostApp.WcfDefinitions;
using WcfHostApp.WsDefines;

namespace WcfHostApp
{
    public class CombineService : ICalculatorService, IMyWebSocketService
    {
        private ICalculatorService _calculatorServiceImplementation;
        private IMyWebSocketService _webSocketServiceImplementation;

        public CombineService(ICalculatorService calculatorService, IMyWebSocketService webSocketService)
        {
            _calculatorServiceImplementation = calculatorService;
            _webSocketServiceImplementation = webSocketService;
        }

        public CombineService() : this(new CalculatorService(), new MyWebSocketService())
        {
        }

        public int Add(int a, int b)
        {
            return _calculatorServiceImplementation.Add(a, b);
        }

        public void Connect()
        {
            _webSocketServiceImplementation.Connect();
        }

        public void SendMessage(string message)
        {
            _webSocketServiceImplementation.SendMessage(message);
        }

    }
}
