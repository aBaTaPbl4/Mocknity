using System;

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

        public override object CreateMockByType(Type type)
        {
            var mock = repository.StrictMock(type, GetConstructorArguments(type));
            Stub(mock);
            return mock;
        }
    }
}