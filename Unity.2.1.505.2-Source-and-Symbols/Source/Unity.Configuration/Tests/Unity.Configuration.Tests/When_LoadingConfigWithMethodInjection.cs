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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity.Configuration.Tests.ConfigFiles;
using Microsoft.Practices.Unity.TestSupport.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.Configuration.Tests
{
    /// <summary>
    /// Summary description for When_LoadingConfigWithMethodInjection
    /// </summary>
    [TestClass]
    public class When_LoadingConfigWithMethodInjection : SectionLoadingFixture<ConfigFileLocator>
    {
        public When_LoadingConfigWithMethodInjection() : base("MethodInjection")
        {
        }

        [TestMethod]
        public void Then_FirstRegistrationHasOneMethodInjection()
        {
            var registration = (from reg in Section.Containers.Default.Registrations
                where reg.TypeName == "ObjectWithInjectionMethod" && reg.Name == "singleMethod"
                select reg).First();

            Assert.AreEqual(1, registration.InjectionMembers.Count);
            var methodRegistration = (MethodElement) registration.InjectionMembers[0];

            Assert.AreEqual("Initialize", methodRegistration.Name);
            CollectionAssert.AreEqual(new string[] {"connectionString", "logger"},
                methodRegistration.Parameters.Select(p => p.Name).ToList());
        }


    }
}
