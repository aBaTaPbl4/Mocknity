﻿using System;
using Microsoft.Practices.ObjectBuilder2;
using Rhino.Mocks;

namespace Mocknity.Strategies.Rhino
{
    public class StubRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy
    {
        public StubRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            var mock = repository.Stub(type);
            Stub(mock);
            if (mocknity.AutoReplayStubs)
            {
                mock.Replay();    
            }            
            return mock;
        }

        public override object CreateMockByType(Type type, IBuilderContext context)
        {
            var mock = repository.Stub(type, GetConstructorArguments(type, context));
            Stub(mock);
            if (mocknity.AutoReplayStubs)
            {
                mock.Replay();
            }
            return mock;
        }
    }
}