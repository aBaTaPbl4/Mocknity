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

namespace Microsoft.Practices.Unity.InterceptionExtension
{
    internal struct GeneratedTypeKey
    {
        private readonly Type baseType;
        private readonly Type[] additionalInterfaces;

        public GeneratedTypeKey(Type baseType, Type[] additionalInterfaces)
        {
            this.baseType = baseType;
            this.additionalInterfaces = additionalInterfaces;
        }

        internal class GeneratedTypeKeyComparer : IEqualityComparer<GeneratedTypeKey>
        {
            public bool Equals(GeneratedTypeKey x, GeneratedTypeKey y)
            {
                if (!(x.baseType.Equals(y.baseType) && x.additionalInterfaces.Length == y.additionalInterfaces.Length))
                {
                    return false;
                }
                for (int i = 0; i < x.additionalInterfaces.Length; i++)
                {
                    if (!x.additionalInterfaces[i].Equals(y.additionalInterfaces[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(GeneratedTypeKey obj)
            {
                return obj.baseType.GetHashCode();
            }
        }
    }
}
