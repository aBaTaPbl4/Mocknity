using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Mocknity.Strategies.Structure;
using Rhino.Mocks;

namespace Mocknity.Strategies.Rhino
{
    public abstract class AbstractRhinoMocksBuilderStrategy : AbstractAutoMockBuilderStrategy
    {
        private readonly IUnityContainer unityContainer;
        protected MockRepository repository;

        public AbstractRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType)
            : base(mocknity, baseType, implType)
        {
            repository = mocknity.getRepository();
            unityContainer = mocknity.getContainer();
        }

        protected object[] GetConstructorArguments(Type serviceType, IBuilderContext context)
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
            ParameterInfo[] constructorParameters = constructorInfo.GetParameters();
            var arguments = new object[constructorParameters.Length];
            int counter = 0;
            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                object paramValue = GetOverridenParameterAtResolve(parameterInfo.ParameterType, context);
                if (paramValue != null)
                {
                    arguments[counter++] = paramValue;
                    continue;
                }
                TypedInjectionValue overridenParam = base.GetOverridenParameterAtRegistration(parameterInfo.ParameterType);
                if (overridenParam == null)
                {
                    arguments[counter++] = unityContainer.Resolve(parameterInfo.ParameterType, string.Empty);    
                }
                else
                {
                    var resolvePolicy = overridenParam.GetResolverPolicy(parameterInfo.ParameterType);
                    arguments[counter++] = resolvePolicy.Resolve(context);
                }
                
            }
            return arguments;
        }

        object GetOverridenParameterAtResolve(Type paramType, IBuilderContext context)
        {
            IDependencyResolverPolicy resolver = context.GetOverriddenResolver(paramType);
            if (resolver == null)
            {
                return null;
            }
            return resolver.Resolve(context);
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
            ConstructorInfo result = constructorInfos[0];
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