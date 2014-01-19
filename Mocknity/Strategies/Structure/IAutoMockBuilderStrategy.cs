using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace Mocknity.Strategies.Structure
{
    public interface IAutoMockBuilderStrategy : IBuilderStrategy
    {
        object CreateMockByInterface(Type type);
        object CreateMockByType(Type type);
        bool IsDefault { get; set; }
        bool OnlyOneMockCreation { get; set; }
        string Name { get; set; }
        TypedInjectionValue[] ConstructorParameters { get; set; }

        StubAction StubAction { get; set; }
    }
}