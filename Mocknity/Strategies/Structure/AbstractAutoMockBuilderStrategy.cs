using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Structure
{
  public abstract class AbstractAutoMockBuilderStrategy : BuilderStrategy, IAutoMockBuilderStrategy
  {
    protected IMocknityExtensionConfiguration mocknity;

    public AbstractAutoMockBuilderStrategy() { }

    public AbstractAutoMockBuilderStrategy(IMocknityExtensionConfiguration mocknity)
    {
      this.mocknity = mocknity;
    }

    public override void PreBuildUp(IBuilderContext context)
    {
      var buildKey = context.OriginalBuildKey;
      if (buildKey.Type.IsInterface && !mocknity.getContainer().IsRegistered(buildKey.Type))
      {
        context.Existing = MockObject(buildKey.Type);
        // register mock for handling
        mocknity.AddMock(buildKey.Type, context.Existing); 
      }
    }

    #region IAutoMockBuilderStrategy Members

    abstract public object MockObject(Type type);

    #endregion
  }
}
