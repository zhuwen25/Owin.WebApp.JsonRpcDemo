using System;

namespace JsonRpcContract.Contracts
{
    public class VmwareConnectionDetail : IConnectionDetail
    {
        public VmwareConnectionDetail(string username, string password)
        {
            ConnectionId = Guid.NewGuid();
            FactoryName = IConnectionDetail.FactoryNameVmware;
            Name = "Vsphere";
            Address = "https://192.163.0.293/sdk";
            Username = username;
            Password = password;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsDisposed { get; private set; }
        public override string FactoryName { get; set; }
        public override string Name { get; set; }
        public override string Address { get; set; }
        public override Guid ConnectionId { get; set; }

        public override string ToString()
        {
            return
                $"FactoryName: {FactoryName}, Name: {Name}, Address: {Address}, ConnectionId: {ConnectionId}, Username: {Username}, Password: {Password}";
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
