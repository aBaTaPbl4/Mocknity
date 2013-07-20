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
            bool needToRegisterIfUnknown = mocknity.MockUnregisteredInterfaces && buildKey.Type.IsInterface;
            if (!mocknity.IsTypeMapped(buildKey.Type) && !needToRegisterIfUnknown)
            {
                return;
            }

            if (!mocknity.getContainer().IsRegistered(buildKey.Type))
            {
                if (buildKey.Type.IsInterface)
                {
                    context.Existing = CreateMockByInterface(buildKey.Type);
                }
                else
                {
                    context.Existing = CreateMockByType(buildKey.Type);
                }

                // register mock for handling
                mocknity.AddMock(buildKey.Type, context.Existing);
                context.BuildComplete = true;
            }
        }

        #region IAutoMockBuilderStrategy Members

        abstract public object CreateMockByInterface(Type type);

        abstract public object CreateMockByType(Type type);

        #endregion
    }
}
