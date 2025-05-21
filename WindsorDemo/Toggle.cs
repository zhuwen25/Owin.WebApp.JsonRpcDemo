using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo
{
    public class Toggle: IToggle
    {
        public bool IsToggleEnable(string toggleName)
        {
            if (string.IsNullOrEmpty(toggleName))
            {
                throw new ArgumentException("Toggle name cannot be null or empty", nameof(toggleName));
            }
            return true;
        }
    }
}
