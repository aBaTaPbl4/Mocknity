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
    public DynamicRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity) : base(mocknity) { }

    public override object MockObject(Type type)
    {
      return this.repository.DynamicMock(type);
    }
  }
}
