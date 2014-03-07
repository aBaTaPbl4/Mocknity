using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Rhino.Mocks;

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
        public StubAction StubAction { get; set; }

        public TypedInjectionValue[] ConstructorParameters { get; set; }
        protected IBuilderContext BuilderContext { get; private set; }

        protected void Stub(object mock)
        {
            if (StubAction != null)
            {
                this.StubAction.Execute(mock);
                if (mocknity.AutoReplayStubbedMocks)
                {
                    mock.Replay();
                } 
            }
        }

        protected TypedInjectionValue GetOverridenParameterAtRegistration(Type paramType)
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
            TypedInjectionValue result = null;
            if (overridenParams.Count < currentIndex + 1)
            {
                result = overridenParams[previousIndex];
            }
            else
            {
                _typeIndexMap[paramType]++;
                result = overridenParams[currentIndex];
            }
                        
            return result;
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

            bool isTypeRegisteredInMocknityContainer = mocknity.getContainer().IsRegisteredPrivate(buildKey.Type);
            bool isTypeRegisteredInResolveCalledFromContainer = context.ResolvedFromContainer.IsRegisteredPrivate(buildKey.Type);
            bool isTypeRegisteredBetweenMocknityAndResolveContainer = IsTypeRegisteredUnderMocknityContainer(context.ResolvedFromContainer, buildKey.Type);

            if (!isTypeRegisteredInMocknityContainer && !isTypeRegisteredInResolveCalledFromContainer && !isTypeRegisteredBetweenMocknityAndResolveContainer)
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
                context.BuildComplete = false;
            }
        }

        /// <summary>
        /// Is type registered in any unity container located under mocknity container but before bottomContainer. 
        /// </summary>
        /// <param name="bottomContainer">Container the type was resolved from</param>
        /// <returns></returns>
        private bool IsTypeRegisteredUnderMocknityContainer(IUnityContainer bottomContainer, Type typeToFind)
        {
            IUnityContainer mocknityContainer = mocknity.getContainer();
            if (mocknityContainer == bottomContainer)
            {
                return false;//no contianers between mocknityContainer and bottomContainer
            }
            IUnityContainer curContainer = bottomContainer.Parent;
            if (curContainer == null || curContainer == mocknityContainer)
            {
                return false; //no contianers between mocknityContainer and bottomContainer
            }

            while (curContainer != null && curContainer != mocknityContainer)
            {
                if (curContainer.IsRegisteredPrivate(typeToFind))
                {
                    return true;
                }
                curContainer = curContainer.Parent;
            }
            return false;

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
            Type mockedType = null;
            if (buildKey.Type.IsInterface)
            {
                bool arrivedInterfaceButWeHaveImplType = _implType != null && _implType != buildKey.Type;
                if (arrivedInterfaceButWeHaveImplType)
                {
                    mockedType = _implType;
                    mock = CreateMockByType(_implType);
                }
                if (mock == null)
                {
                    mockedType = buildKey.Type;
                    mock = CreateMockByInterface(buildKey.Type);
                }
            }
            else
            {
                mockedType = _implType;
                mock = CreateMockByType(_implType);

            }
            //we need to reset BuildKey with real type, to make unity to configure objects in later stage (init dependcy properties, injection method etc.
            UpdateBuildKey(mockedType);
            return mock;
        }

        public abstract object CreateMockByType(Type type);

        protected void UpdateBuildKey(Type type)
        {
            BuilderContext.BuildKey = new NamedTypeBuildKey(type, BuilderContext.BuildKey.Name);
        }
    }
}