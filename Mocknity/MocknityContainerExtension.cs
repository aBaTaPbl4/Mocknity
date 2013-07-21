using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Mocknity.Strategies.Rhino;
using Rhino.Mocks;

namespace Mocknity
{
    public class MocknityContainerExtension : UnityContainerExtension, IMocknityExtensionConfiguration
    {
        private readonly bool _mockUnregisteredInterfaces;
        private readonly Dictionary<Type, object> mocks = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> strategiesMapping = new Dictionary<Type, Type>();
        private IBuilderStrategy _defaultStrategy;
        protected MockRepository repository;

        public MocknityContainerExtension(MockRepository repository, bool mockUnregisteredInterfaces = false)
        {
            this.repository = repository;
            _mockUnregisteredInterfaces = mockUnregisteredInterfaces;
        }

        #region IMocknityExtensionConfiguration Members

        public void SetStrategy<T>(Type type)
        {
            SetStrategy<T>(type, type);
        }

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
                _defaultStrategy = new DynamicRhinoMocksBuilderStrategy(this, null, null, true);
                Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
            }
        }

        private IBuilderStrategy CreateBuilderStrategy<T>(Type baseType, Type implType, bool isDefault)
        {
            Type builderImpl = typeof (T).GetInterface(typeof (IBuilderStrategy).Name);
            if (builderImpl != null)
            {
                return
                    (IBuilderStrategy)
                    Activator.CreateInstance(typeof (T), new object[] {this, baseType, implType, isDefault});
            }
            throw new ArgumentException("Type must implement IBuilderStrategy interface", "typeBase");
        }

        /// <summary>
        /// Set default strategy. Required using first, because clear all previous mappings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetDefaultStrategy<T>()
        {
            IBuilderStrategy strategy = CreateBuilderStrategy<T>(null, null, true);
            _defaultStrategy = strategy;
            mocks.Clear();
            strategiesMapping.Clear();
            Context.Strategies.Clear();
            Context.Strategies.Add(_defaultStrategy, UnityBuildStage.PreCreation);
        }

        private void SetStrategy<T>(Type typeBase, Type typeImpl)
        {
            IBuilderStrategy strategy = CreateBuilderStrategy<T>(typeBase, typeImpl, false);
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

        public void RegisterStrictMock<TBaseType, TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }

        public void RegisterStrictMock<TType>()
        {
            SetStrategy<StrictRhinoMocksBuilderStrategy>(typeof (TType));
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

        public void RegisterStub<TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (TType));
        }

        public void RegisterStub<TBaseType, TType>()
        {
            SetStrategy<StubRhinoMocksBuilderStrategy>(typeof (TBaseType), typeof (TType));
        }
    }
}