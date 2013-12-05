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

using System.Linq;
using Microsoft.Practices.Unity.Configuration.Tests.ConfigFiles;
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.Practices.Unity.TestSupport.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.Configuration.Tests
{
    /// <summary>
    /// Summary description for When_ConfiguringContainerWithBasicTypeMappings
    /// </summary>
    [TestClass]
    public class When_ConfiguringContainerWithBasicTypeMappings : SectionLoadingFixture<ConfigFileLocator>
    {
        public When_ConfiguringContainerWithBasicTypeMappings() : base("BasicTypeMapping")
        {
        }

        private IUnityContainer container;

        protected override void Arrange()
        {
            base.Arrange();
            container = new UnityContainer();
        }

        protected override void Act()
        {
            Section.Configure(container);
        }
    
        [TestMethod]
        public void Then_ContainerHasTwoMappingsForILogger()
        {
            Assert.AreEqual(2,
                container.Registrations.Where(r => r.RegisteredType == typeof(ILogger)).Count());
        }

        [TestMethod]
        public void Then_DefaultILoggerIsMappedToMockLogger()
        {
            Assert.AreEqual(typeof (MockLogger),
                container.Registrations
                    .Where(r => r.RegisteredType == typeof (ILogger) && r.Name == null)
                    .Select(r => r.MappedToType)
                    .First());
        }

        [TestMethod]
        public void Then_SpecialILoggerIsMappedToSpecialLogger()
        {
            Assert.AreEqual(typeof(SpecialLogger),
                container.Registrations
                    .Where(r => r.RegisteredType == typeof(ILogger) && r.Name == "special")
                    .Select(r => r.MappedToType)
                    .First());
        }

        [TestMethod]
        public void Then_AllRegistrationsHaveTransientLifetime()
        {
            Assert.IsTrue(container.Registrations
                .Where(r => r.RegisteredType == typeof(ILogger))
                .All(r => r.LifetimeManagerType == typeof (TransientLifetimeManager)));
        }
    }
}
