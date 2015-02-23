using System;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
    public class StrictRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy
    {
        public StrictRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            var mock = repository.StrictMock(type);
            Stub(mock);
            return mock;
        }

        public override object CreateMockByType(Type type, IBuilderContext context)
        {
            var mock = repository.StrictMock(type, GetConstructorArguments(type, context));
            Stub(mock);
            return mock;
        }
    }
}