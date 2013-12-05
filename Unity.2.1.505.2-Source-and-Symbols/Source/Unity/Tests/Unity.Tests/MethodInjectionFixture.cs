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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.Tests
{
    [TestClass]
    public class MethodInjectionFixture
    {
        [TestMethod]
        public void CanInjectMethodReturningVoid()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof (GuineaPig),
                    new InjectionMethod("Inject2", "Hello"));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual("Hello", pig.StringValue);
        }

        [TestMethod]
        public void CanInjectMethodReturningInt()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType(typeof (GuineaPig),
                        new InjectionMethod("Inject3", 17));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual(17, pig.IntValue);
        }

        [TestMethod]
        public void CanConfigureMultipleMethods()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType<GuineaPig>(
                    new InjectionMethod("Inject3", 37),
                    new InjectionMethod("Inject2", "Hi there"));

            GuineaPig pig = container.Resolve<GuineaPig>();

            Assert.AreEqual(37, pig.IntValue);
            Assert.AreEqual("Hi there", pig.StringValue);
        }

        [TestMethod]
        public void StaticMethodsShouldNotBeInjected()
        {
            IUnityContainer container = new UnityContainer();

            GuineaPig pig = container.Resolve<GuineaPig>();
            Assert.IsFalse(GuineaPig.StaticMethodWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContainerThrowsWhenConfiguringStaticMethodForInjection()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType<GuineaPig>(
                       new InjectionMethod("ShouldntBeCalled"));
            Assert.Fail("Should not get here");
        }

        public class GuineaPig
        {
            public int IntValue;
            public string StringValue;
            public static bool StaticMethodWasCalled = false;

            public void Inject1()
            {

            }

            public void Inject2(string stringValue)
            {
                this.StringValue = stringValue;
            }

            public int Inject3(int intValue)
            {
                IntValue = intValue;
                return intValue * 2;
            }

            [InjectionMethod]
            public static void ShouldntBeCalled()
            {
                StaticMethodWasCalled = true;
            }
        }
    }
}
