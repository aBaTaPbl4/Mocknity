using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder2;
using Mocknity.Strategies.Structure;
using Mocknity.Strategies.Rhino;
using Rhino.Mocks;

namespace Mocknity
{
  public class MocknityContainerExtension : UnityContainerExtension, IMocknityExtensionConfiguration
  {
    Dictionary<Type, Type> strategiesMapping = new Dictionary<Type, Type>();
    protected MockRepository repository;

    public MocknityContainerExtension(MockRepository repository)
    {
      this.repository = repository;
    }

    protected override void Initialize()
    {
      if (this.repository == null)
      {
        throw new NullReferenceException("Mocknity needs to have a mock repository");
      }
      // register default builder strategy
      Context.Strategies.Add(new DynamicRhinoMocksBuilderStrategy(this), UnityBuildStage.PreCreation);
    }

    #region IMocknityExtensionConfiguration Members

    public void SetStrategy<T>(Type type)
    {
      var builderImpl = typeof(T).GetInterface(typeof(IBuilderStrategy).Name);
      if (builderImpl != null)
      {
        if (this.strategiesMapping.ContainsKey(type))
        {
          this.strategiesMapping.Remove(type);
        }
        this.strategiesMapping.Add(type, typeof(T));
        Context.Strategies.Add((IBuilderStrategy)Activator.CreateInstance(typeof(T), new object[] { this }), UnityBuildStage.PreCreation);
      }
      else
      {
        throw new ArgumentException("Type must implement IBuilderStrategy interface", "type");
      }
    }

    public bool CheckStrategyMapping<T>(Type type)
    {
      if (strategiesMapping.ContainsKey(type) && strategiesMapping[type] == typeof(T))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool IsTypeMapped(Type type)
    {
      return strategiesMapping.ContainsKey(type);
    }

    public MockRepository getRepository()
    {
      return this.repository;
    }

    public IUnityContainer getContainer()
    {
      return Container;
    }

    #endregion
  }
}
