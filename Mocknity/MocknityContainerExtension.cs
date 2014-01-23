using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Mocknity.Strategies.Rhino;
using Mocknity.Strategies.Structure;
using Rhino.Mocks;

namespace Mocknity
{
    public class MocknityContainerExtension : UnityContainerExtension, IMocknityExtensionConfiguration
    {
        //name, mappings coolection
        private readonly Dictionary<string, Dictionary<Type, object>> mocks;
        private readonly Dictionary<string, Dictionary<Type, Type>> strategiesMapping;
        private IAutoMockBuilderStrategy _defaultStrategy;
        private const UnityBuildStage StrategiesStage = UnityBuildStage.TypeMapping;
        protected MockRepository repository;

        public MocknityContainerExtension(MockRepository repository, bool mockUnregisteredInterfaces = false)
        {
            mocks = new Dictionary<string, Dictionary<Type, object>>();
            strategiesMapping = new Dictionary<string, Dictionary<Type, Type>>();
            this.repository = repository;
            MockUnregisteredInterfaces = mockUnregisteredInterfaces;
            AutoReplayPartialMocks = true;
            AutoReplayStubbedMocks = true;
        }

        #region IMocknityExtensionConfiguration Members

        public bool CheckStrategyMapping<TStrategy>(Type type, string name)
        {
            if (!strategiesMapping.ContainsKey(name))
            {
                return false;
            }
            var mappings = strategiesMapping[name];
            if (mappings.ContainsKey(type) && mappings[type] == typeof (TStrategy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetMock<TType>(string name)
        {
            Type key = typeof (TType);
            return GetMock(key, name);
        }

        public object GetMock(Type key, string name)
        {
            if (!mocks.ContainsKey(name))
            {
                return null;
            }
            var mcks = mocks[name];
            if (mcks.ContainsKey(key))
            {
                return mcks[key];
            }
            else
            {
                return null;
            }
        }

        public void RemoveMapping(Type key, string name)
        {
            strategiesMapping[name].Remove(key);
        }

        public bool ContainsMapping(Type key, string name)
        {
            if (!strategiesMapping.ContainsKey(name))
            {
                return false;
            }
            var mappings = strategiesMapping[name];
            return mappings.ContainsKey(key);
        }

        public void AddMapping<TMapping>(Type type, string name)
        {
            if (!strategiesMapping.ContainsKey(name))
            {
                strategiesMapping[name] = new Dictionary<Type, Type>();
            }
            strategiesMapping[name].Add(type, typeof(TMapping));
        }

        public bool ContainsMock(Type key, string name)
        {
            if (!mocks.ContainsKey(name))
            {
                return false;
            }
            var mcks = mocks[name];
            return mcks.ContainsKey(key);
        }

        public bool ContainsMock<TType>(string name)
        {
            Type key = typeof (TType);
            return ContainsMock(key, name);
        }

        public void AddMock(Type type, object mock, string name)
        {
            if (!mocks.ContainsKey(name))
            {
                mocks[name] = new Dictionary<Type, object>();
            }
            var mcks = mocks[name];
            mcks.Add(type, mock);
        }

        public bool IsTypeMapped(Type type, string name)
        {
            return ContainsMapping(type, name);
        }

        public MockRepository getRepository()
        {
            return repository;
        }

        public IUnityContainer getContainer()
        {
            return Container;
        }

        public bool MockUnregisteredInterfaces { get; set; }

        public bool AutoReplayPartialMocks { get; set; }
        public bool AutoReplayStubbedMocks { get; set; }

        #endregion

        protected override void Initialize()
        {
            if (repository == null)
            {
                throw new NullReferenceException("Mocknity needs to have a mock repository");
            }
            // register default builder strategy
            if (MockUnregisteredInterfaces)
            {
                _defaultStrategy = new DynamicRhinoMocksBuilderStrategy(this, null, null);
                _defaultStrategy.IsDefault = true;
                Context.Strategies.Add(_defaultStrategy, StrategiesStage);
            }
        }

        private IAutoMockBuilderStrategy CreateBuilderStrategy<TStrategy>(Type baseType, Type implType, string name, StubAction stubAction, params TypedInjectionValue[] resolveParams)
        {
            Type builderImpl = typeof (TStrategy).GetInterface(typeof (IBuilderStrategy).Name);
            if (builderImpl != null)
            {
                var strategy = 
                    (IAutoMockBuilderStrategy)
                    Activator.CreateInstance(typeof (TStrategy), new object[] {this, baseType, implType});
                strategy.Name = name;
                strategy.ConstructorParameters = resolveParams;
                strategy.StubAction = stubAction;
                return strategy;
            }
            throw new ArgumentException("Type must implement IAutoMockBuilderStrategy interface", "typeBase");
        }

        /// <summary>
        /// Set default strategy. Required using first, because clear all previous mappings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetDefaultStrategy<TStrategy>()
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<TStrategy>(null, null, "", null);
            strategy.IsDefault = true;
            _defaultStrategy = strategy;
            mocks.Clear();
            strategiesMapping.Clear();
            Context.Strategies.Clear();
            Context.Strategies.Add(_defaultStrategy, StrategiesStage);
        }

        public void SetStrategy<TStrategy>(Type type, bool onlyOneMockCreate = true, string name = "",  StubAction stubAction = null, params TypedInjectionValue[] resolveParams)
        {
            SetStrategy<TStrategy>(type, type, onlyOneMockCreate, name, stubAction, resolveParams);
        }

        private void SetStrategy<TStrategy>(Type typeBase, Type typeImpl, bool onlyOneMockCreate, string name, StubAction stubAction = null, params TypedInjectionValue[] resolveParams)
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<TStrategy>(typeBase, typeImpl, name, stubAction, resolveParams);
            strategy.OnlyOneMockCreation = onlyOneMockCreate;

            if (ContainsMapping(typeBase, name))
            {                
                RemoveMapping(typeBase, name);
            }
            if (ContainsMapping(typeImpl, name))
            {
                RemoveMapping(typeImpl, name);            
            }            
            AddMapping<TStrategy>(typeBase, name);
            if (typeBase != typeImpl)
            {
                AddMapping<TStrategy>(typeImpl, name);
            }
            Context.Strategies.Add(strategy, StrategiesStage);
            Container.RegisterKnownByExtensionNamedType(typeBase, name);
            Container.ClearCache();
        }

        public void RegisterStrictMockType<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name);
        }

        public void RegisterStrictMockType<TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

                    
        public void RegisterStrictMockType<TBaseType, TImplType>(Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, "", actionWrap);
        }

        public void RegisterStrictMockType<TType>(Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TType), false, "", actionWrap);
        }

        public void RegisterStrictMockType<TBaseType, TImplType>(string name, Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name, actionWrap);
        }

        public void RegisterStrictMockType<TType>(string name, Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TType), false, name, actionWrap);
        }

        public void RegisterStrictMock<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, name);
        }

        public void RegisterStrictMock<TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TType), true, name);
        }

        public void RegisterDynamicMockType<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name);
        }

        public void RegisterDynamicMockType<TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterDynamicMockType<TBaseType, TImplType>(Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, "", actionWrap);
        }

        public void RegisterDynamicMockType<TType>(Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), false, "", actionWrap);
        }

        public void RegisterDynamicMockType<TBaseType, TImplType>(string name, Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name, actionWrap);
        }

        public void RegisterDynamicMockType<TType>(string name, Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), false, name, actionWrap);
        }

        public void RegisterDynamicMock<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, name);
        }

        public void RegisterDynamicMock<TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), true, name);
        }



        public void RegisterPartialMock<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, name);
        }

        public void RegisterPartialMock<TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof (TType), true, name);
        }

        public void RegisterPartialMock<TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), true, "", null, resolveParamOverrides);
        }

        public void RegisterPartialMock<TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), true, name, null, resolveParamOverrides);
        }


        public void RegisterPartialMock<TBaseType, TImplType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, "", null, resolveParamOverrides);
        }

        public void RegisterPartialMock<TBaseType, TImplType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, name, null, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name);
        }

        public void RegisterPartialMockType<TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterPartialMockType<TType>(Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, "", actionWrap);
        }

        public void RegisterPartialMockType<TType>(Action<TType> stubAction, params TypedInjectionValue[] resolveParamOverrides)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, "", actionWrap, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TBaseType, TImplType>(Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, "", actionWrap );
        }

        public void RegisterPartialMockType<TType>(string name, Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, name, actionWrap);
        }
        
        public void RegisterPartialMockType<TBaseType, TImplType>(string name, Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name, actionWrap);
        }

        public void RegisterPartialMockType<TBaseType, TImplType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, "", null, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, "", null, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TBaseType, TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name, null, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, name, null, resolveParamOverrides);
        }

        public void RegisterStub<TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), true, name);
        }


        public void RegisterStub<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), true, name);
        }

        public void RegisterStubType<TType>(string name, Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), false, name, actionWrap);
        }

        public void RegisterStubType<TType>(Action<TType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TType>(stubAction);
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), false, "", actionWrap);
        }

        public void RegisterStubType<TBaseType, TImplType>(Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, "", actionWrap);
        }

        public void RegisterStubType<TBaseType, TImplType>(string name, Action<TImplType> stubAction)
        {
            var actionWrap = new StubActionGeneric<TImplType>(stubAction);
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name, actionWrap);
        }

        public void RegisterStubType<TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterStubType<TBaseType, TImplType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TImplType), false, name);
        }
    }
}