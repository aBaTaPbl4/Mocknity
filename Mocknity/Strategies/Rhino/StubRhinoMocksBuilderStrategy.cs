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
    public StubRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity) : base(mocknity) { }

    public override object MockObject(Type type)
    {
      return this.repository.Stub(type);
    }
  }
}
