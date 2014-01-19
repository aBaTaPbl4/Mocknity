using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            ParameterInfo[] constructorParameters = constructorInfo.GetParameters();
            var arguments = new object[constructorParameters.Length];
            int counter = 0;
            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                TypedInjectionValue overridenParam = base.GetOverridenParameter(parameterInfo.ParameterType);
                if (overridenParam == null)
                {
                    arguments[counter++] = unityContainer.Resolve(parameterInfo.ParameterType, string.Empty);    
                }
                else
                {
                    var resolvePolicy = overridenParam.GetResolverPolicy(parameterInfo.ParameterType);
                    arguments[counter++] = resolvePolicy.Resolve(BuilderContext);
                }
                
            }
            return arguments;
        }

        protected void InitDependencyProperties(object mock, Type type)
        {
            List<PropertyInfo> props = type.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(DependencyAttribute))).ToList();
            foreach (var propertyInfo in props)
            {
                DependencyAttribute attrib = Attribute.GetCustomAttribute(
                                                            propertyInfo, 
                                                            typeof(DependencyAttribute)) as DependencyAttribute;
                string typeRegistrationName = attrib == null ? string.Empty : attrib.Name ?? string.Empty;
                propertyInfo.SetValue(mock, unityContainer.Resolve(propertyInfo.PropertyType, typeRegistrationName), null);
            }
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