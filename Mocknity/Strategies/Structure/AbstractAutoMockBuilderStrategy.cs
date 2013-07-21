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
        private Type _implType;
        private bool _isDefault;
        private bool _onlyOneMockCreation;
        public AbstractAutoMockBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false, bool onlyOneMockCreation = true)
        {
            this.mocknity = mocknity;
            _baseType = baseType;
            _implType = implType ?? baseType;
            _isDefault = isDefault;
            _onlyOneMockCreation = onlyOneMockCreation;
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            var buildKey = context.OriginalBuildKey;
            bool needToRegisterIfUnknown = mocknity.MockUnregisteredInterfaces && buildKey.Type.IsInterface;
            if (!mocknity.IsTypeMapped(buildKey.Type) && !needToRegisterIfUnknown)
            {
                //type is not mapped and no need to register
                return;
            }

            if (buildKey.Type != _baseType && buildKey.Type != _implType && !_isDefault)
            {
                //this strategy is not owner for received type
                return;
            }

            if (_isDefault && !buildKey.Type.IsInterface)
            {
                //default build strategy using only for mocking interfaces
                return;
            }

            if (_isDefault && mocknity.ContainsMapping(buildKey.Type))
            {
                //if mapping of the type received is registered, - dont use default build strategy
                return;
            }

            if (!String.IsNullOrEmpty(buildKey.Name))
            {
                //named instances not supported yet
            }

            if (!mocknity.getContainer().IsRegistered(buildKey.Type))
            {
                Type typeToSearch = buildKey.Type.IsInterface ? buildKey.Type : _implType;
                if (_onlyOneMockCreation && mocknity.ContainsMock(typeToSearch))
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
                object mock = null;
                bool arrivedInterfaceButWeHaveImplType = _implType != null && _implType != buildKey.Type;
                if (arrivedInterfaceButWeHaveImplType && !mocknity.ContainsMock(_implType) )
                {
                    mock = CreateMockByType(_implType);
                    mocknity.AddMock(_implType, mock);
                }
                if (mock == null)
                {
                    mock = CreateMockByInterface(buildKey.Type);
                }
                context.Existing = mock;
            }
            else
            {
                object mock = null;
                mock = CreateMockByType(_implType);
                if (_implType != _baseType && !mocknity.ContainsMock(_baseType))
                {
                    mocknity.AddMock(_baseType, mock);
                }
                context.Existing = mock;
            }
            mocknity.AddMock(buildKey.Type, context.Existing);
        }

        #region IAutoMockBuilderStrategy Members

        abstract public object CreateMockByInterface(Type type);

        abstract public object CreateMockByType(Type type);

        #endregion
    }
}
