using System;

namespace Mocknity.Strategies.Rhino
{
    public class DynamicRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
    {
        public DynamicRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType,
                                                bool isDefault = false) : base(mocknity, baseType, implType, isDefault)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            return repository.DynamicMock(type);
        }

        public override object CreateMockByType(Type type)
        {
            return repository.DynamicMock(type, GetConstructorArguments(type));
        }
    }
}