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
        private Type _baseType;
        private Type _ImplType;
        private bool _isDefault;
        private bool _onlyOneMockCreation;
        public AbstractAutoMockBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false, bool onlyOneMockCreation = true)
        {
            this.mocknity = mocknity;
            _baseType = baseType;
            _ImplType = implType ?? baseType;
            _isDefault = isDefault;
            _onlyOneMockCreation = onlyOneMockCreation;
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            var buildKey = context.OriginalBuildKey;
            bool needToRegisterIfUnknown = mocknity.MockUnregisteredInterfaces && buildKey.Type.IsInterface;
            if (!mocknity.IsTypeMapped(buildKey.Type) && !needToRegisterIfUnknown)
            {
                return;
            }

            if (buildKey.Type != _baseType && buildKey.Type != _ImplType && !_isDefault)
            {
                return;
            }

            if (!String.IsNullOrEmpty(buildKey.Name))
            {
                //named instances not supported yet
            }

            if (!mocknity.getContainer().IsRegistered(buildKey.Type))
            {
                Type typeToSearch = buildKey.Type.IsInterface ? buildKey.Type : _ImplType;
                if (_onlyOneMockCreation && mocknity.Contains(typeToSearch))
                {
                    context.Existing = mocknity.Get(typeToSearch);
                }
                else
                {
                    RegisterMock(context, buildKey);
                }
                context.BuildComplete = true;
            }
        }

        private void RegisterMock(IBuilderContext context, NamedTypeBuildKey buildKey)
        {
            if (buildKey.Type.IsInterface)
            {
                context.Existing = CreateMockByInterface(buildKey.Type);
            }
            else
            {
                context.Existing = CreateMockByType(_ImplType);
            }
            mocknity.AddMock(buildKey.Type, context.Existing);
        }

        #region IAutoMockBuilderStrategy Members

        abstract public object CreateMockByInterface(Type type);

        abstract public object CreateMockByType(Type type);

        #endregion
    }
}
