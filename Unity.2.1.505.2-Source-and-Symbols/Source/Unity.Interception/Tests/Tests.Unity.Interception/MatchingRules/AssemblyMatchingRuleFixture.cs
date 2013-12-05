//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Unity.InterceptionExtension.Tests.MatchingRules
{
    [TestClass]
    public partial class AssemblyMatchingRuleFixture
    {
        private MethodBase objectToStringMethod;
        private string assemblyVersion;
        private string assemblyPublicKeyToken;

        [TestInitialize]
        public void TestInitialize()
        {
            string assemblyName = typeof(object).Assembly.FullName;
            assemblyVersion = assemblyName.Substring(assemblyName.IndexOf("Version=") + "Version=".Length);
            assemblyVersion = assemblyVersion.Substring(0, assemblyVersion.IndexOf(","));
            assemblyPublicKeyToken = assemblyName.Substring(assemblyName.IndexOf("PublicKeyToken=") + "PublicKeyToken=".Length);
            if (assemblyPublicKeyToken.IndexOf(",") > 0)
            {
                assemblyPublicKeyToken = assemblyPublicKeyToken.Substring(0, assemblyPublicKeyToken.IndexOf(","));
            }
            objectToStringMethod = typeof(object).GetMethod("ToString");
        }

        [TestMethod]
        public void CanMatchAssemblyNameByNameOnly()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule("mscorlib");
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanExplicitlyDenyMatchOnVersion()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule("mscorlib, Version=1.2.3.4, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.IsFalse(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanMatchAssemblyNameByNameAndVersion()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}", assemblyVersion));
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanMatchAssemblyNameByNameVersionAndKey()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, PublicKeyToken={1}", assemblyVersion, assemblyPublicKeyToken));
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanMatchAssemblyNameByNameVersionAndCulture()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, Culture=neutral", assemblyVersion));
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanMatchAssemblyByFullyQualifiedName()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, Culture=neutral, PublicKeyToken={1}", assemblyVersion, assemblyPublicKeyToken));
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanExplicitlyDenyMatchOnNoKey()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, Culture=neutral, PublicKeyToken=null", assemblyVersion));
            Assert.IsFalse(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanExplicitlyDenyMatchOnSpecificKey()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, Culture=neutral, PublicKeyToken=bbbbbbbbbbbbbbbb", assemblyVersion));
            Assert.IsFalse(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanExplicitlyDenyMatchOnSpecificCulture()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0}, Culture=nl-NL, PublicKeyToken={1}", assemblyVersion, assemblyPublicKeyToken));
            Assert.IsFalse(matchingRule.Matches(objectToStringMethod));
        }

        [TestMethod]
        public void CanMatchAssemblyNameUsingArbitraryAmountOfSpaces()
        {
            AssemblyMatchingRule matchingRule = new AssemblyMatchingRule(string.Format("mscorlib, Version={0},     Culture=neutral,   PublicKeyToken={1}", assemblyVersion, assemblyPublicKeyToken));
            Assert.IsTrue(matchingRule.Matches(objectToStringMethod));
        }
    }
}
