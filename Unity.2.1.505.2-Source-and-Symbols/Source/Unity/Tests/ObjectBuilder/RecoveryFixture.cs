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
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.ObjectBuilder2.Tests
{
    // Testing that the IRequiresRecovery interface is
    // properly handled in the buildup process.
    [TestClass]
    public class RecoveryFixture
    {
        [TestMethod]
        public void RecoveryIsExecutedOnException()
        {
            var recovery = new RecoveryObject();
            MockBuilderContext context = GetContext();
            context.RecoveryStack.Add(recovery);

            try
            {
                context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);
            }
            catch(Exception)
            {
                // This is supposed to happen.
            }

            Assert.IsTrue(recovery.wasRecovered);
        }

        private static MockBuilderContext GetContext()
        {
            var context = new MockBuilderContext();
            context.Strategies.Add(new ThrowingStrategy());

            return context;
        }

        private class RecoveryObject : IRequiresRecovery
        {
            public bool wasRecovered;

            public void Recover()
            {
                wasRecovered = true;
            }
        }

        private class ThrowingStrategy : BuilderStrategy
        {
            public override void PreBuildUp(IBuilderContext context)
            {
                throw new Exception("Throwing from strategy");
            }
        }
    }
}
