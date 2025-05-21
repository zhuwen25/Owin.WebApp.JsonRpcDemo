using System.Reflection;

namespace WindsorDemo.Interfaces
{
    public interface IHclMethodTranslationFactory
    {
        IHclMethodTranslation Create(MethodInfo methodInfo);
    }
}
