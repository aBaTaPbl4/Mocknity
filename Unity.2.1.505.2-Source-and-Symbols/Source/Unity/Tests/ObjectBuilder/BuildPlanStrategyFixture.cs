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

using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.ObjectBuilder2.Tests
{
    [TestClass]
    public class BuildPlanStrategyFixture
    {
        [TestMethod]
        public void StrategyGetsBuildPlanFromPolicySet()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new BuildPlanStrategy());
            object instance = new object();
            ReturnInstanceBuildPlan plan = new ReturnInstanceBuildPlan(instance);

            context.Policies.Set<IBuildPlanPolicy>(plan, new NamedTypeBuildKey<object>());

            object result = context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);

            Assert.IsTrue(plan.BuildUpCalled);
            Assert.AreSame(instance, result);
        }

        [TestMethod]
        public void StrategyCreatesBuildPlanWhenItDoesntExist()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new BuildPlanStrategy());
            MockBuildPlanCreatorPolicy policy = new MockBuildPlanCreatorPolicy();
            context.Policies.SetDefault<IBuildPlanCreatorPolicy>(policy);

            object result = context.ExecuteBuildUp(new NamedTypeBuildKey<object>(), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(policy.PolicyWasCreated);

            IBuildPlanPolicy plan = context.Policies.Get<IBuildPlanPolicy>(new NamedTypeBuildKey(typeof(object)));
            Assert.IsNotNull(plan);
        }

    }

    internal class MockBuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
    {
        private bool policyWasCreated = false;

        public IBuildPlanPolicy CreatePlan(IBuilderContext context, NamedTypeBuildKey buildKey)
        {
            policyWasCreated = true;
            return new ReturnInstanceBuildPlan(new object());
        }

        public bool PolicyWasCreated
        {
            get { return policyWasCreated; }
            set { policyWasCreated = value; }
        }
    }

    class ReturnInstanceBuildPlan : IBuildPlanPolicy
    {
        private object instance;
        private bool buildUpCalled;

        public ReturnInstanceBuildPlan(object instance)
        {
            this.instance = instance;
            this.buildUpCalled = false;
        }

        public void BuildUp(IBuilderContext context)
        {
            buildUpCalled = true;
            context.Existing = instance;
        }

        public bool BuildUpCalled
        {
            get { return buildUpCalled; }
        }

        public object Instance
        {
            get { return instance; }
        }
    }
}
