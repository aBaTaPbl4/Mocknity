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
        protected MockRepository repository;

        public MocknityContainerExtension(MockRepository repository, bool mockUnregisteredInterfaces = false)
        {
            mocks = new Dictionary<string, Dictionary<Type, object>>();
            strategiesMapping = new Dictionary<string, Dictionary<Type, Type>>();
            this.repository = repository;
            MockUnregisteredInterfaces = mockUnregisteredInterfaces;
            AutoReplayPartialMocks = true;
        }

        #region IMocknityExtensionConfiguration Members

        public bool CheckStrategyMapping<T>(Type type, string name)
        {
            if (!strategiesMapping.ContainsKey(name))
            {
                return false;
            }
            var mappings = strategiesMapping[name];
            if (mappings.ContainsKey(type) && mappings[type] == typeof (T))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetMock<T>(string name)
        {
            Type key = typeof (T);
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

        public bool ContainsMock<T>(string name)
        {
            Type key = typeof (T);
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
                Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
            }
        }

        private IAutoMockBuilderStrategy CreateBuilderStrategy<T>(Type baseType, Type implType, string name, params TypedInjectionValue[] resolveParams)
        {
            Type builderImpl = typeof (T).GetInterface(typeof (IBuilderStrategy).Name);
            if (builderImpl != null)
            {
                var strategy = 
                    (IAutoMockBuilderStrategy)
                    Activator.CreateInstance(typeof (T), new object[] {this, baseType, implType});
                strategy.Name = name;
                strategy.ConstructorParameters = resolveParams;
                return strategy;
            }
            throw new ArgumentException("Type must implement IAutoMockBuilderStrategy interface", "typeBase");
        }

        /// <summary>
        /// Set default strategy. Required using first, because clear all previous mappings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetDefaultStrategy<T>()
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<T>(null, null, "");
            strategy.IsDefault = true;
            _defaultStrategy = strategy;
            mocks.Clear();
            strategiesMapping.Clear();
            Context.Strategies.Clear();
            Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
        }

        public void SetStrategy<T>(Type type, bool onlyOneMockCreate = true, string name = "", params TypedInjectionValue[] resolveParams)
        {
            SetStrategy<T>(type, type, onlyOneMockCreate, name, resolveParams);
        }

        private void SetStrategy<T>(Type typeBase, Type typeImpl, bool onlyOneMockCreate, string name, params TypedInjectionValue[] resolveParams)
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<T>(typeBase, typeImpl, name, resolveParams);
            strategy.OnlyOneMockCreation = onlyOneMockCreate;

            if (ContainsMapping(typeBase, name))
            {                
                RemoveMapping(typeBase, name);
            }
            if (ContainsMapping(typeImpl, name))
            {
                RemoveMapping(typeImpl, name);            
            }            
            AddMapping<T>(typeBase, name);
            if (typeBase != typeImpl)
            {
                AddMapping<T>(typeImpl, name);
            }
            Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
        }

        public void RegisterStrictMockType<TBaseType, TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name);
        }

        public void RegisterStrictMockType<TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterStrictMock<TBaseType, TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, name);
        }

        public void RegisterStrictMock<TType>(string name = "")
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TType), true, name);
        }

        public void RegisterDynamicMockType<TBaseType, TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name);
        }

        public void RegisterDynamicMockType<TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterDynamicMock<TBaseType, TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, name);
        }

        public void RegisterDynamicMock<TType>(string name = "")
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), true, name);
        }



        public void RegisterPartialMock<TBaseType, TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, name);
        }

        public void RegisterPartialMock<TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof (TType), true, name);
        }

        public void RegisterPartialMock<TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), true, "", resolveParamOverrides);
        }

        public void RegisterPartialMock<TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), true, name, resolveParamOverrides);
        }


        public void RegisterPartialMock<TBaseType, TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, "", resolveParamOverrides);
        }

        public void RegisterPartialMock<TBaseType, TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, name, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TBaseType, TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name);
        }

        public void RegisterPartialMockType<TType>(string name = "")
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterPartialMockType<TBaseType, TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, "", resolveParamOverrides);
        }

        public void RegisterPartialMockType<TType>(params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, "", resolveParamOverrides);
        }

        public void RegisterPartialMockType<TBaseType, TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name, resolveParamOverrides);
        }

        public void RegisterPartialMockType<TType>(string name, params TypedInjectionValue[] resolveParamOverrides)
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false, name, resolveParamOverrides);
        }

        public void RegisterStub<TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), true, name);
        }

        public void RegisterStub<TBaseType, TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), true, name);
        }

        public void RegisterStubType<TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), false, name);
        }

        public void RegisterStubType<TBaseType, TType>(string name = "")
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false, name);
        }
    }
}