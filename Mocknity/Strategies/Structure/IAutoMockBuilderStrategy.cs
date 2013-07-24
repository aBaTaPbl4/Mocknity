using System;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Structure
{
    public interface IAutoMockBuilderStrategy : IBuilderStrategy
    {
        object CreateMockByInterface(Type type);
        object CreateMockByType(Type type);
        bool IsDefault { get; set; }
        bool OnlyOneMockCreation { get; set; }
        string Name { get; set; }
    }
}