using System;

namespace WcfHostApp.WcfDefinitions
{
    public class CalculatorService : ICalculatorService
    {
        public int Add(int a, int b)
        {
            Console.WriteLine("Add called");
            return a + b;
        }
    }
}
