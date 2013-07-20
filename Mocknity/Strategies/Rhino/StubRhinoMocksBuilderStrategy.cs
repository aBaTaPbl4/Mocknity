using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
  public class StubRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<StubRhinoMocksBuilderStrategy>
  {
      public StubRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false) : base(mocknity, baseType, implType, isDefault) { }

    public override object CreateMockByInterface(Type type)
    {
      return this.repository.Stub(type);
    }

      public override object CreateMockByType(Type type)
      {
          return this.repository.Stub(type, GetConstructorArguments(type));
      }
  }
}
