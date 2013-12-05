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
using System.ComponentModel;
using System.Configuration;
using System.Xml;
using Microsoft.Practices.Unity.Configuration.ConfigurationHelpers;

namespace Microsoft.Practices.Unity.Configuration
{
    /// <summary>
    /// A configuration element that describes an instance to add to the container.
    /// </summary>
    public class InstanceElement : ContainerConfiguringElement
    {
        private const string NamePropertyName = "name";
        private const string TypeConverterTypeNamePropertyName = "typeConverter";
        private const string TypeNamePropertyName = "type";
        private const string ValuePropertyName = "value";

        /// <summary>
        /// Name to register instance under
        /// </summary>
        [ConfigurationProperty(NamePropertyName, IsRequired = false, DefaultValue = "")]
        public string Name
        {
            get { return (string) base[NamePropertyName]; }
            set { base[NamePropertyName] = value; }
        }

        /// <summary>
        /// Value for this instance
        /// </summary>
        [ConfigurationProperty(ValuePropertyName, IsRequired = false)]
        public string Value
        {
            get { return (string) base[ValuePropertyName]; }
            set { base[ValuePropertyName] = value; }
        }

        /// <summary>
        /// Type of the instance. If not given, defaults to string
        /// </summary>
        [ConfigurationProperty(TypeNamePropertyName, IsRequired = false, DefaultValue = "")]
        public string TypeName
        {
            get { return (string) base[TypeNamePropertyName]; }
            set { base[TypeNamePropertyName] = value; }
        }

        /// <summary>
        /// Type name for the type converter to use to create the instance. If not
        /// given, defaults to the default type converter for this instance type.
        /// </summary>
        [ConfigurationProperty(TypeConverterTypeNamePropertyName, IsRequired = false, DefaultValue = "")]
        public string TypeConverterTypeName
        {
            get { return (string) base[TypeConverterTypeNamePropertyName]; }
            set { base[TypeConverterTypeNamePropertyName] = value; }
        }

        /// <summary>
        /// Key used to keep these instances unique in the config collection.
        /// </summary>
        public override string Key
        {
            get { return "instance:" + Name + ":" + Value; }
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
            writer.WriteAttributeIfNotEmpty(NamePropertyName, Name)
                .WriteAttributeIfNotEmpty(ValuePropertyName, Value)
                .WriteAttributeIfNotEmpty(TypeNamePropertyName, TypeName)
                .WriteAttributeIfNotEmpty(TypeConverterTypeNamePropertyName, TypeConverterTypeName);
        }

        /// <summary>
        /// Add the instance defined by this element to the given container.
        /// </summary>
        /// <param name="container">Container to configure.</param>
        protected override void ConfigureContainer(IUnityContainer container)
        {
            Type instanceType = GetInstanceType();
            object instanceValue = GetInstanceValue();

            container.RegisterInstance(instanceType, Name, instanceValue);
        }

        private Type GetInstanceType()
        {
            return TypeResolver.ResolveTypeWithDefault(TypeName, typeof (string));
        }

        private object GetInstanceValue()
        {
            if (string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(TypeConverterTypeName))
            {
                return null;
            }

            TypeConverter converter = GetTypeConverter();
            return converter.ConvertFromInvariantString(Value);
        }

        private TypeConverter GetTypeConverter()
        {
            if (!string.IsNullOrEmpty(TypeConverterTypeName))
            {
                Type converterType = TypeResolver.ResolveType(TypeConverterTypeName);
                return (TypeConverter) Activator.CreateInstance(converterType);
            }
            return TypeDescriptor.GetConverter(GetInstanceType());
        }
    }
}
