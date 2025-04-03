using System;

namespace JsonRpcContract.Contracts
{
    public sealed class AzureConnectionDetail : IConnectionDetail
    {
        public AzureConnectionDetail()
        {
            ConnectionId = Guid.NewGuid();
            FactoryName = IConnectionDetail.FactoryNameAzure;
            Name = "Azure";
            Address = "https://management.azure.com/";
            ResourceGroup = "Default";
        }

        public string ResourceGroup { get; set; }

        public bool IsDisposed { get; private set; }
        public override string FactoryName { get; set; }
        public override string Name { get; set; }
        public override string Address { get; set; }
        public override Guid ConnectionId { get; set; }

        public override string ToString()
        {
            return
                $"FactoryName: {FactoryName}, Name: {Name}, Address: {Address}, ConnectionId: {ConnectionId}, ResourceGroup: {ResourceGroup}";
        }

        public override void Dispose()
        {
            if (IsDisposed is false)
            {
                DisposedEvent?.Invoke(this, EventArgs.Empty);
                IsDisposed = true;
            }
        }

        public event EventHandler DisposedEvent;
    }
}
