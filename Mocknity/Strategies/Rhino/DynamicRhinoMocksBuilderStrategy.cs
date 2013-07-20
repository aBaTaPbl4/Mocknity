using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mocknity.Strategies.Structure;
using Microsoft.Practices.Unity;
using Rhino.Mocks;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
  public class DynamicRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<DynamicRhinoMocksBuilderStrategy>
  {
      public DynamicRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false) : base(mocknity, baseType, implType, isDefault) { }

    public override object CreateMockByInterface(Type type)
    {
      return this.repository.DynamicMock(type);
    }

      public override object CreateMockByType(Type type)
      {
          return this.repository.DynamicMock(type, GetConstructorArguments(type));
      }
  }
}
