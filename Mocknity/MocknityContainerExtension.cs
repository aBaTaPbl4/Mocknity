using System;
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
        private readonly bool _mockUnregisteredInterfaces;
        private readonly Dictionary<Type, object> mocks = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> strategiesMapping = new Dictionary<Type, Type>();
        private IAutoMockBuilderStrategy _defaultStrategy;
        protected MockRepository repository;

        public MocknityContainerExtension(MockRepository repository, bool mockUnregisteredInterfaces = false)
        {
            this.repository = repository;
            _mockUnregisteredInterfaces = mockUnregisteredInterfaces;
            AutoReplayPartialMocks = true;
        }

        #region IMocknityExtensionConfiguration Members

        public bool CheckStrategyMapping<T>(Type type)
        {
            if (strategiesMapping.ContainsKey(type) && strategiesMapping[type] == typeof (T))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object Get<T>()
        {
            Type key = typeof (T);
            return Get(key);
        }

        public object Get(Type key)
        {
            if (mocks.ContainsKey(key))
            {
                return mocks[key];
            }
            else
            {
                return null;
            }
        }

        public bool ContainsMapping(Type key)
        {
            return strategiesMapping.ContainsKey(key);
        }

        public bool ContainsMock(Type key)
        {
            return mocks.ContainsKey(key);
        }

        public bool ContainsMock<T>()
        {
            Type key = typeof (T);
            return ContainsMock(key);
        }

        public void AddMock(Type type, object mock)
        {
            mocks.Add(type, mock);
        }

        public bool IsTypeMapped(Type type)
        {
            return strategiesMapping.ContainsKey(type);
        }

        public MockRepository getRepository()
        {
            return repository;
        }

        public IUnityContainer getContainer()
        {
            return Container;
        }

        public bool MockUnregisteredInterfaces
        {
            get { return _mockUnregisteredInterfaces; }
        }

        public bool AutoReplayPartialMocks { get; set; }


        #endregion

        protected override void Initialize()
        {
            if (repository == null)
            {
                throw new NullReferenceException("Mocknity needs to have a mock repository");
            }
            // register default builder strategy
            if (_mockUnregisteredInterfaces)
            {
                _defaultStrategy = new DynamicRhinoMocksBuilderStrategy(this, null, null);
                _defaultStrategy.IsDefault = true;
                Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
            }
        }

        private IAutoMockBuilderStrategy CreateBuilderStrategy<T>(Type baseType, Type implType)
        {
            Type builderImpl = typeof (T).GetInterface(typeof (IBuilderStrategy).Name);
            if (builderImpl != null)
            {
                return
                    (IAutoMockBuilderStrategy)
                    Activator.CreateInstance(typeof (T), new object[] {this, baseType, implType});
            }
            throw new ArgumentException("Type must implement IAutoMockBuilderStrategy interface", "typeBase");
        }

        /// <summary>
        /// Set default strategy. Required using first, because clear all previous mappings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetDefaultStrategy<T>()
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<T>(null, null);
            strategy.IsDefault = true;
            _defaultStrategy = strategy;
            mocks.Clear();
            strategiesMapping.Clear();
            Context.Strategies.Clear();
            Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
        }

        public void SetStrategy<T>(Type type, bool onlyOneMockCreate = true)
        {
            SetStrategy<T>(type, type, onlyOneMockCreate);
        }

        private void SetStrategy<T>(Type typeBase, Type typeImpl, bool onlyOneMockCreate = true)
        {
            IAutoMockBuilderStrategy strategy = CreateBuilderStrategy<T>(typeBase, typeImpl);
            strategy.OnlyOneMockCreation = onlyOneMockCreate;

            if (strategiesMapping.ContainsKey(typeBase))
            {
                strategiesMapping.Remove(typeBase);
            }
            if (strategiesMapping.ContainsKey(typeImpl))
            {
                strategiesMapping.Remove(typeImpl);
            }
            strategiesMapping.Add(typeBase, typeof (T));
            if (typeBase != typeImpl)
            {
                strategiesMapping.Add(typeImpl, typeof (T));
            }
            Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
        }

        public void RegisterStrictMockType<TBaseType, TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false);
        }

        public void RegisterStrictMockType<TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof(TType), false);
        }

        public void RegisterStrictMock<TBaseType, TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }

        public void RegisterStrictMock<TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TType));
        }

        public void RegisterDynamicMockType<TBaseType, TType>()
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false);
        }

        public void RegisterDynamicMockType<TType>()
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof(TType), false);
        }

        public void RegisterDynamicMock<TBaseType, TType>()
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }

        public void RegisterDynamicMock<TType>()
        {
            SetStrategy<DynamicRhinoMocksBuilderStrategy>(typeof (TType));
        }

        public void RegisterPartialMock<TBaseType, TType>()
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }

        public void RegisterPartialMock<TType>()
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof (TType));
        }

        public void RegisterPartialMockType<TBaseType, TType>()
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof(TType), false);
        }

        public void RegisterPartialMockType<TType>()
        {
            SetStrategy<PartialRhinoMocksBuilderStrategy>(typeof(TType), false);
        }

        public void RegisterStub<TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (TType));
        }

        public void RegisterStub<TBaseType, TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }

        public void RegisterStubType<TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TType), false);
        }

        public void RegisterStubType<TBaseType, TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof(TBaseType), typeof(TType), false);
        }
    }
}