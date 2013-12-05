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
using System.Configuration;
using System.Xml;
using Microsoft.Practices.Unity.Configuration.ConfigurationHelpers;
using Microsoft.Practices.Unity.Utility;

namespace Microsoft.Practices.Unity.Configuration
{
    /// <summary>
    /// A configuration element storing information about a single type alias.
    /// </summary>
    public class AliasElement : DeserializableConfigurationElement
    {
        private const string AliasPropertyName = "alias";
        private const string TypeNamePropertyName = "type";

        /// <summary>
        /// Construct a new, uninitialized <see cref="AliasElement"/>.
        /// </summary>
        public AliasElement()
        {
            
        }

        /// <summary>
        /// Construct a new <see cref="AliasElement"/> that is initialized
        /// to alias <paramref name="alias"/> to the target <paramref name="targetType"/>.
        /// </summary>
        /// <param name="alias">Alias to use.</param>
        /// <param name="targetType">Type that is aliased.</param>
        public AliasElement(string alias, Type targetType)
        {
            Guard.ArgumentNotNull(targetType, "targetType");
            Alias = alias;
            TypeName = targetType.AssemblyQualifiedName;
        }

        /// <summary>
        /// The alias used for this type.
        /// </summary>
        [ConfigurationProperty(AliasPropertyName, IsRequired = true, IsKey = true)]
        public string Alias
        {
            get { return (string) base[AliasPropertyName]; }
            set { base[AliasPropertyName] = value; }
        }

        /// <summary>
        /// The fully qualified name this alias refers to.
        /// </summary>
        [ConfigurationProperty(TypeNamePropertyName, IsRequired = true)]
        public string TypeName
        {
            get { return (string) base[TypeNamePropertyName]; }
            set { base[TypeNamePropertyName] = value; }
        }

        /// <summary>
        /// Write the contents of this element to the given <see cref="XmlWriter"/>.
        /// </summary>
        /// <remarks>The caller of this method has already written the start element tag before
        /// calling this method, so deriving classes only need to write the element content, not
        /// the start or end tags.</remarks>
        /// <param name="writer">Writer to send XML content to.</param>
        public override void SerializeContent(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteAttributeString(AliasPropertyName, Alias);
            writer.WriteAttributeString(TypeNamePropertyName, TypeName);
        }
    }
}
