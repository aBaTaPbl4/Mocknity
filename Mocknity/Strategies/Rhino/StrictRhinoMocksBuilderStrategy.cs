using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
  public class StrictRhinoMocksBuilderStrategy : AbstractRhinoMocksBuilderStrategy<StrictRhinoMocksBuilderStrategy>
  {
    public StrictRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity) : base(mocknity) { }

    public override object MockObject(Type type)
    {
      return this.repository.StrictMock(type);
    }
  }
}
