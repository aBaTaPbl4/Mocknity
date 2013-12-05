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

using Microsoft.Practices.ObjectBuilder2.Tests.TestDoubles;

namespace Microsoft.Practices.ObjectBuilder2.Tests.TestObjects
{
    class ObjectWithMarkedConstructor
    {
        public ObjectWithMarkedConstructor(int notTheInjectionConstructor)
        {
            
        }

        [InjectionConstructor]
        public ObjectWithMarkedConstructor(string theInjectionConstructor)
        {
            
        }
    }
}
