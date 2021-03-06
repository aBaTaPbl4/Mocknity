﻿using System;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
    public class DynamicRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy
    {
        public DynamicRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) : 
            base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            var mock = repository.DynamicMock(type);
            Stub(mock);
            return mock;
        }

        public override object CreateMockByType(Type type, IBuilderContext context)
        {
            var mock = repository.DynamicMock(type, GetConstructorArguments(type, context));
            Stub(mock);
            return mock;
        }
    }
}