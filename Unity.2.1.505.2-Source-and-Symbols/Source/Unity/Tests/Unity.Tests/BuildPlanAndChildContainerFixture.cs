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

using Microsoft.Practices.Unity.Tests.TestObjects;
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.Tests
{
    /// <summary>
    /// Summary description for BuildPlanAndChildContainerFixture
    /// </summary>
    [TestClass]
    public class BuildPlanAndChildContainerFixture
    {
        private IUnityContainer parentContainer;
        private IUnityContainer childContainer;

        private const int ValueInjectedFromParent = 3;
        private const int ValueInjectedFromChild = 5;

        [TestInitialize]
        public void Setup()
        {
            parentContainer = new UnityContainer()
                .RegisterType<TestObject>(new InjectionConstructor(ValueInjectedFromParent))
                .RegisterType<ILogger, MockLogger>();

            childContainer = parentContainer.CreateChildContainer()
                .RegisterType<TestObject>(new InjectionConstructor(ValueInjectedFromChild));
        }

        public class TestObject
        {
            public int Value { get; private set; }

            public TestObject(int value)
            {
                Value = value;
            }
        }

        [TestMethod]
        public void ValuesInjectedAreCorrectWhenResolvingFromParentFirst()
        {
            // Be aware ordering is important here - resolve through parent first
            // to elicit the buggy behavior
            var fromParent = parentContainer.Resolve<TestObject>();
            var fromChild = childContainer.Resolve<TestObject>();

            Assert.AreEqual(ValueInjectedFromParent, fromParent.Value);
            Assert.AreEqual(ValueInjectedFromChild, fromChild.Value);
        }

        [TestMethod]
        public void ChildContainersForUnconfiguredTypesPutConstructorParamResolversInParent()
        {
            childContainer.Resolve<ObjectWithOneDependency>();
            parentContainer.CreateChildContainer().Resolve<ObjectWithOneDependency>();

            // No exception means we're good.
        }

        [TestMethod]
        public void ChildContainersForUnconfiguredTypesPutPropertyResolversInParent()
        {
            childContainer.Resolve<ObjectWithLotsOfDependencies>();
            parentContainer.CreateChildContainer().Resolve<ObjectWithLotsOfDependencies>();
        }
    }
}
