﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhino.Mocks;
using Microsoft.Practices.Unity;

namespace Mocknity.Strategies.Rhino
{
    public class PartialRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
    {
        public PartialRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType) 
            : base(mocknity, baseType, implType)
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
            object mock = repository.PartialMock(type, parms);
            InitDependencyProperties(mock, type);
            if (mocknity.AutoReplayPartialMocks)
            {
                mock.Replay();    
            }            
            return mock;
            
        }
    }
}