using System;

namespace Mocknity.Strategies.Rhino
{
    public class PartialRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
    {
        public PartialRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType,
                                                bool isDefault = false) : base(mocknity, baseType, implType, isDefault)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            //rhino throws exception
            return repository.PartialMock(type);
        }

        public override object CreateMockByType(Type type)
        {
            object[] parms = GetConstructorArguments(type);
            return repository.PartialMock(type, parms);
        }
    }
}