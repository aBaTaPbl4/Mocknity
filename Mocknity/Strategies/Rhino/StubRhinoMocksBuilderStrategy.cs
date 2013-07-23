using System;

namespace Mocknity.Strategies.Rhino
{
    public class StubRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<StubRhinoMocksBuilderStrategy>
    {
        public StubRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            return repository.Stub(type);
        }

        public override object CreateMockByType(Type type)
        {
            return repository.Stub(type, GetConstructorArguments(type));
        }
    }
}