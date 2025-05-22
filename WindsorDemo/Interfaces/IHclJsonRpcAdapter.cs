using System;

namespace WindsorDemo.Interfaces
{
    public interface IHclJsonRpcAdapter
    {
        /// <summary>
        /// Converts a Wcf Object to a Hcl compatible object.
        /// </summary>
        /// <param name="jsonRpcObject">The wcf object to be converted</param>
        /// <param name="targetType">The target type for output</param>
        /// <returns>An hcl object if it supports, otherwise the original object.</returns>
        object ToHclObject(object jsonRpcObject, Type targetType);

        /// <summary>
        /// Converts a Hcl Object to a Wcf compatible object.
        /// </summary>
        /// <param name="hclObject">The hcl object to be converted</param>
        /// <param name="targetType">The target type for output</param>
        /// <returns>An wcf object if it supports, otherwise the original object.</returns>
        object ToJsonRpcObject(object hclObject, Type targetType);



    }
}
