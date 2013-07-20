﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mocknity.Strategies.Structure;
using Microsoft.Practices.Unity;
using Rhino.Mocks;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
    public class PartialRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
    {
        public PartialRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity) : base(mocknity) { }

        public override object CreateMockByInterface(Type type)
        {
            return this.repository.PartialMock(type);
        }

        public override object CreateMockByType(Type type)
        {
            throw new NotImplementedException();
        }
    }
}