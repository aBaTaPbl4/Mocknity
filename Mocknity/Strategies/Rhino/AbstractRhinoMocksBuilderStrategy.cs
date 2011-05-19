using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using Mocknity.Strategies.Structure;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
  public abstract class AbstractRhinoMocksBuilderStrategy<T> : AbstractAutoMockBuilderStrategy
  {
    protected MockRepository repository;

    public AbstractRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity)
      : base(mocknity)
    {
      this.repository = mocknity.getRepository();
    }

    public override void PreBuildUp(IBuilderContext context)
    {
      bool isRegistered = false;
      if (typeof(T) == typeof(DynamicRhinoMocksBuilderStrategy))
      {
        // default builder strategy if nothing else mapped for object
        isRegistered = !mocknity.IsTypeMapped(context.OriginalBuildKey.Type);
      }
      else
      {
        // check if this builder is the expected one for the object
        isRegistered = mocknity.CheckStrategyMapping<T>(context.OriginalBuildKey.Type);
      }

      if (isRegistered)
      {
        // build up object in this builder strategy
        base.PreBuildUp(context);
      }
    }
  }
}
