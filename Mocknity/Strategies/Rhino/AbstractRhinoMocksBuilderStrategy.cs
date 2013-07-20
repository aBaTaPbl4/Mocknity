using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.Unity;
using Rhino.Mocks;
using Mocknity.Strategies.Structure;
using Microsoft.Practices.ObjectBuilder2;

namespace Mocknity.Strategies.Rhino
{
  public abstract class AbstractRhinoMocksBuilderStrategy<T> : AbstractAutoMockBuilderStrategy
  {
    protected MockRepository repository;
      private IUnityContainer unityContainer;
    public AbstractRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType, bool isDefault = false)
      : base(mocknity, baseType, implType, isDefault)
    {
      this.repository = mocknity.getRepository();
      this.unityContainer = mocknity.getContainer();
    }


    protected object[] GetConstructorArguments(Type serviceType)
    {
        ConstructorInfo constructorInfo;
        try
        {
            constructorInfo = GetGreediestConstructor(serviceType.GetConstructors());
        }
        catch (Exception ex)
        {
            throw new ApplicationException(string.Format("Auto mocking failed for type:{0}", serviceType), ex);
        }
        var constructorParameters = constructorInfo.GetParameters();
        var arguments = new object[constructorParameters.Length];
        var counter = 0;
        foreach (var parameterInfo in constructorParameters)
        {
            arguments[counter++] = this.unityContainer.Resolve(parameterInfo.ParameterType, string.Empty);
        }
        return arguments;
    }

    /// Gets the greediest constructor.
    /// The constructor infos.
    /// Greediest constructor.
    private static ConstructorInfo GetGreediestConstructor(ConstructorInfo[] constructorInfos)
    {
        if (constructorInfos.Length == 0)
        {
            throw new InvalidOperationException("No available constructors.");
        }

        if (constructorInfos.Length == 1)
        {
            return constructorInfos[0];
        }
        var result = constructorInfos[0];
        for (int i = 1; i < constructorInfos.Length; i++)
        {
            if (constructorInfos[i].GetParameters().Length > result.GetParameters().Length)
            {
                result = constructorInfos[i];
            }
        }
        return result;
    }
  }
}
