using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using Microsoft.Practices.ObjectBuilder2;
using Rhino.Mocks;
using Microsoft.Practices.Unity;

namespace Mocknity.Strategies.Rhino
{
    public class PartialRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy
    {
        public PartialRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
        {
        }

        public override object CreateMockByInterface(Type type)
        {
            //rhino throws exception
            var mock = repository.PartialMock(type);
            Stub(mock);
            return mock;
        }

        public override object CreateMockByType(Type type)
        {
            object[] parms = GetConstructorArguments(type);
            object mock = repository.PartialMock(type, parms);
            //we need to reset BuildKey with real type, to make unity to configure objects in later stage (init dependcy properties, injection method etc.
            BuilderContext.BuildKey = new NamedTypeBuildKey(type, BuilderContext.BuildKey.Name);
            Stub(mock);
            if (mocknity.AutoReplayPartialMocks)
            {
               mock.Replay(); 
            }
            return mock;
            
        }
    }
}