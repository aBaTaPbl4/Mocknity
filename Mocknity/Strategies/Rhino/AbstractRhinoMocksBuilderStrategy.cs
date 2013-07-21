using System;
using System.Reflection;
using Microsoft.Practices.Unity;
using Mocknity.Strategies.Structure;
using Rhino.Mocks;

namespace Mocknity.Strategies.Rhino
{
    public abstract class AbstractRhinoMocksBuilderStrategy<T> : AbstractAutoMockBuilderStrategy
    {
        private readonly IUnityContainer unityContainer;
        protected MockRepository repository;

        public AbstractRhinoMocksBuilderStrategy(IMocknityExtensionConfiguration mocknity, Type baseType, Type implType,
                                                 bool isDefault = false)
            : base(mocknity, baseType, implType, isDefault)
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
                arguments[counter++] = unityContainer.Resolve(parameterInfo.ParameterType, string.Empty);
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