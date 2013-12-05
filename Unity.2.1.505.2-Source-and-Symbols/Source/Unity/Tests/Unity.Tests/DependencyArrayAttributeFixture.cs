﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.Tests
{
    [TestClass]
    public class DependencyArrayAttributeFixture
    {
        [TestMethod]
        public void CanResolveArrayForConstructorParameter()
        {
            ILogger o1 = new MockLogger();
            ILogger o2 = new SpecialLogger();

            IUnityContainer container
                = new UnityContainer()
                .RegisterInstance<ILogger>("o1", o1)
                .RegisterInstance<ILogger>("o2", o2);

            TypeWithArrayConstructorParameter resolved = container.Resolve<TypeWithArrayConstructorParameter>();

            Assert.IsNotNull(resolved.loggers);
            Assert.AreEqual(2, resolved.loggers.Length);
            Assert.AreSame(o1, resolved.loggers[0]);
            Assert.AreSame(o2, resolved.loggers[1]);
        }

        [TestMethod]
        public void CanResolveArrayForProperty()
        {
            ILogger o1 = new MockLogger();
            ILogger o2 = new SpecialLogger();

            IUnityContainer container
                = new UnityContainer()
                .RegisterInstance<ILogger>("o1", o1)
                .RegisterInstance<ILogger>("o2", o2);

            TypeWithArrayProperty resolved = container.Resolve<TypeWithArrayProperty>();

            Assert.IsNotNull(resolved.Loggers);
            Assert.AreEqual(2, resolved.Loggers.Length);
            Assert.AreSame(o1, resolved.Loggers[0]);
            Assert.AreSame(o2, resolved.Loggers[1]);
        }

        [TestMethod]
        public void CanResolveArrayForConstructorParameterOnClosedGenericType()
        {
            ILogger o1 = new MockLogger();
            ILogger o2 = new SpecialLogger();

            IUnityContainer container
                = new UnityContainer()
                .RegisterInstance<ILogger>("o1", o1)
                .RegisterInstance<ILogger>("o2", o2);

            GenericTypeWithArrayConstructorParameter<ILogger> resolved
                = container.Resolve<GenericTypeWithArrayConstructorParameter<ILogger>>();

            Assert.IsNotNull(resolved.values);
            Assert.AreEqual(2, resolved.values.Length);
            Assert.AreSame(o1, resolved.values[0]);
            Assert.AreSame(o2, resolved.values[1]);
        }

        [TestMethod]
        public void BindingDependencyArrayToArrayParameterWithRankOverOneThrows()
        {
            IUnityContainer container = new UnityContainer();

            try
            {
                container.Resolve<TypeWithArrayConstructorParameterOfRankTwo>();
                Assert.Fail("Call to Resolve<>() should have failed");
            }
            catch (ResolutionFailedException)
            {
            }
        }

        public class TypeWithArrayConstructorParameter
        {
            public readonly ILogger[] loggers;

            public TypeWithArrayConstructorParameter(ILogger[] loggers)
            {
                this.loggers = loggers;
            }
        }

        public class GenericTypeWithArrayConstructorParameter<T>
        {
            public readonly T[] values;

            public GenericTypeWithArrayConstructorParameter(T[] values)
            {
                this.values = values;
            }
        }

        public class TypeWithArrayProperty
        {
            private ILogger[] loggers;

            [Dependency]
            public ILogger[] Loggers
            {
                get { return loggers; }
                set { this.loggers = value; }
            }
        }

        public class TypeWithArrayConstructorParameterOfRankTwo
        {
            public TypeWithArrayConstructorParameterOfRankTwo(ILogger[,] loggers)
            {
            }
        }
    }
}
