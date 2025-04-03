namespace JsonRpcContract.Contracts
{
    public class VMDiskInfo
    {
        public string DiskName { get; set; }
        public string DiskPath { get; set; }
        public int DiskSize { get; set; }
        public string DiskType { get; set; }

        public override string ToString()
        {
            return $"DiskName: {DiskName}, DiskPath: {DiskPath}, DiskSize: {DiskSize}, DiskType: {DiskType}";
        }
    }
}
