using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace Mocknity.Strategies.Structure
{
    public abstract class AbstractAutoMockBuilderStrategy : BuilderStrategy, IAutoMockBuilderStrategy
    {
        private readonly Type _baseType;
        private readonly Type _implType;
        protected IMocknityExtensionConfiguration mocknity;

        public AbstractAutoMockBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType)
        {
            this.mocknity = mocknity;
            _baseType = baseType;
            _implType = implType ?? baseType;
            IsDefault = false;
            OnlyOneMockCreation = true;
        }

        public bool IsDefault { get; set; }
        public bool OnlyOneMockCreation { get; set; }

        #region IAutoMockBuilderStrategy Members

        public abstract object CreateMockByInterface(Type type);

        #endregion

        public override void PreBuildUp(IBuilderContext context)
        {
            NamedTypeBuildKey buildKey = context.OriginalBuildKey;
            bool needToRegisterIfUnknown = mocknity.MockUnregisteredInterfaces && buildKey.Type.IsInterface;
            if (!mocknity.IsTypeMapped(buildKey.Type) && !needToRegisterIfUnknown)
            {
                //type is not mapped and no need to register
                return;
            }

            if (buildKey.Type != _baseType && buildKey.Type != _implType && !IsDefault)
            {
                //this strategy is not owner for received type
                return;
            }

            if (IsDefault && !buildKey.Type.IsInterface)
            {
                //default build strategy using only for mocking interfaces
                return;
            }

            if (IsDefault && mocknity.ContainsMapping(buildKey.Type))
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
                if (OnlyOneMockCreation)
                {
                    if (mocknity.ContainsMock(typeToSearch))
                    {
                        context.Existing = mocknity.Get(typeToSearch);    
                    }
                    else
                    {
                        context.Existing = RegisterMock(buildKey);
                    }
                }
                else //registration is not required
                {
                    context.Existing = CreateMock(buildKey);
                }
                context.BuildComplete = true;
            }
        }

        private object RegisterMock(NamedTypeBuildKey buildKey)
        {
            var mock = CreateMock(buildKey);
            var baseType = _baseType ?? buildKey.Type;
            mocknity.AddMock(baseType , mock);
            if (_implType != null && _implType != baseType)
            {
                mocknity.AddMock(_implType, mock);    
            }
            return mock;
        }

        private object CreateMock(NamedTypeBuildKey buildKey)
        {
            object mock = null;
            if (buildKey.Type.IsInterface)
            {
                bool arrivedInterfaceButWeHaveImplType = _implType != null && _implType != buildKey.Type;
                if (arrivedInterfaceButWeHaveImplType)
                {
                    mock = CreateMockByType(_implType);
                }
                if (mock == null)
                {
                    mock = CreateMockByInterface(buildKey.Type);
                }
            }
            else
            {
                mock = CreateMockByType(_implType);

            }
            return mock;
        }

        public abstract object CreateMockByType(Type type);
    }
}