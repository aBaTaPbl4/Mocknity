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
using System.ComponentModel;
using System.Reflection;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Microsoft.Practices.Unity.TestSupport
{
    public class NaiveINotifyPropertyChangedInterceptionBehavior : IInterceptionBehavior
    {
        private PropertyChangedEventHandler handler;
        private readonly object handlerLock = new object();

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var methodReturn = InvokeINotifyPropertyChangedMethod(input);

            if (methodReturn != null)
            {
                return methodReturn;
            }

            methodReturn = getNext()(input, getNext);

            if (methodReturn.Exception == null && input.MethodBase.IsSpecialName)
            {
                var property = GetPropertyInfoForSetMethod(input);

                if (property != null)
                {
                    var currentHandler = this.handler;
                    if (currentHandler != null)
                    {
                        try
                        {
                            currentHandler(input.Target, new PropertyChangedEventArgs(property.Name));
                        }
                        catch (Exception e)
                        {
                            return input.CreateExceptionMethodReturn(e);
                        }
                    }
                }
            }

            return methodReturn;
        }

        private IMethodReturn InvokeINotifyPropertyChangedMethod(IMethodInvocation input)
        {
            if (input.MethodBase.DeclaringType == typeof(INotifyPropertyChanged))
            {
                switch (input.MethodBase.Name)
                {
                    case "add_PropertyChanged":
                        lock (handlerLock)
                        {
                            handler = (PropertyChangedEventHandler)Delegate.Combine(handler, (Delegate)input.Arguments[0]);
                        }
                        break;

                    case "remove_PropertyChanged":
                        lock (handlerLock)
                        {
                            handler = (PropertyChangedEventHandler)Delegate.Remove(handler, (Delegate)input.Arguments[0]);
                        }
                        break;

                    default:
                        return input.CreateExceptionMethodReturn(new InvalidOperationException());
                }

                return input.CreateMethodReturn(null);
            }

            return null;
        }

        private static PropertyInfo GetPropertyInfoForSetMethod(IMethodInvocation input)
        {
            foreach (var property in input.MethodBase.DeclaringType.GetProperties())
            {
                if (input.MethodBase == property.GetSetMethod())
                {
                    return property;
                }
            }

            return null;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return new[] { typeof(INotifyPropertyChanged) };
        }

        public bool WillExecute { get { return true; } }
    }
}
