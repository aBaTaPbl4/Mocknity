using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace Mocknity.Strategies.Structure
{
    public abstract class AbstractAutoMockBuilderStrategy : BuilderStrategy, IAutoMockBuilderStrategy
    {
        private readonly Type _baseType;
        private readonly Type _implType;

        private Dictionary<Type, int> _typeIndexMap; 
        protected IMocknityExtensionConfiguration mocknity;

        public AbstractAutoMockBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType)
        {
            this.mocknity = mocknity;
            _baseType = baseType;
            _implType = implType ?? baseType;
            IsDefault = false;
            OnlyOneMockCreation = true;
            Name = "";            
            _typeIndexMap = new Dictionary<Type, int>();
        }
        
        
        public bool IsDefault { get; set; }
        public bool OnlyOneMockCreation { get; set; }
        public string Name { get; set; }
        public TypedInjectionValue[] ConstructorParameters { get; set; }
        protected IBuilderContext BuilderContext { get; private set; }

        protected TypedInjectionValue GetOverridenParameter(Type paramType)
        {
            if (ConstructorParameters == null || 
                ConstructorParameters.Length == 0 ||
                !ConstructorParameters.Any(x => x.ParameterType == paramType))
            {
                return null;
            }
            if (!_typeIndexMap.ContainsKey(paramType))
            {
                _typeIndexMap.Add(paramType, -1);
            }
            int previousIndex = _typeIndexMap[paramType];
            int currentIndex = previousIndex + 1;
            var overridenParams = ConstructorParameters.Where(x => x.ParameterType == paramType).ToList();
            if (overridenParams.Count < currentIndex + 1)
            {
                return overridenParams[previousIndex];
            }
            _typeIndexMap[paramType]++;
            return overridenParams[currentIndex];
        }

        #region IAutoMockBuilderStrategy Members

        public abstract object CreateMockByInterface(Type type);

        #endregion

        public override void PreBuildUp(IBuilderContext context)
        {
            BuilderContext = context;
            NamedTypeBuildKey buildKey = context.OriginalBuildKey;
            bool needToRegisterIfUnknown = mocknity.MockUnregisteredInterfaces && buildKey.Type.IsInterface;
            if (!mocknity.IsTypeMapped(buildKey.Type, Name) && !needToRegisterIfUnknown)
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

            if (IsDefault && mocknity.ContainsMapping(buildKey.Type, Name))
            {
                //if mapping of the type received is registered, - dont use default build strategy
                return;
            }

            var inputName = buildKey.Name ?? "";
            if (inputName != Name)
            {
                //it is the strategy for other registration name
                return;
            }

            if (!mocknity.getContainer().IsRegisteredPrivate(buildKey.Type))
            {
                Type typeToSearch = buildKey.Type.IsInterface ? buildKey.Type : _implType;
                if (OnlyOneMockCreation)
                {
                    if (mocknity.ContainsMock(typeToSearch, Name))
                    {
                        context.Existing = mocknity.GetMock(typeToSearch, Name);    
                    }
                    else
                    {
                        context.Existing = RegisterMock(buildKey);
                    }
                }
                else //multiple mocks case:registration is not required
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
            mocknity.AddMock(baseType , mock, Name);
            if (_implType != null && _implType != baseType)
            {
                mocknity.AddMock(_implType, mock, Name);    
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