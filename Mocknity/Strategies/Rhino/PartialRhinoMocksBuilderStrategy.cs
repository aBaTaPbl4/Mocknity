using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Mocknity.Strategies.Structure;
using Microsoft.Practices.Unity;
using Rhino.Mocks;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
    public class PartialRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
    {
        public PartialRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false) : base(mocknity, baseType, implType, isDefault) { }

        public override object CreateMockByInterface(Type type)
        {
            //rhino throws exception
            return this.repository.PartialMock(type);
        }

        public override object CreateMockByType(Type type)
        {
            object[] parms = GetConstructorArguments(type);
            return this.repository.PartialMock(type, parms );
        }

    }
}