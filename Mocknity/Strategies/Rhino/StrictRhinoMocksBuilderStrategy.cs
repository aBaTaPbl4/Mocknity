using System;

namespace Mocknity.Strategies.Rhino
{
    public class StrictRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<StrictRhinoMocksBuilderStrategy>
    {
        public StrictRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            return repository.StrictMock(type);
        }

        public override object CreateMockByType(Type type)
        {
            return repository.StrictMock(type, GetConstructorArguments(type));
        }
    }
}