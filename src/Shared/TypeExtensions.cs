﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;

namespace System
{
    internal static partial class TypeExtensions
    {
        private const BindingFlags QualifiedMethodBindingFlags = BindingFlags.NonPublic |
                                                                 BindingFlags.Static |
                                                                 BindingFlags.Instance |
                                                                 BindingFlags.IgnoreCase |
                                                                 BindingFlags.DeclaredOnly;


        /// <summary>
        /// Find a base type or implemented interface which has a generic definition
        /// represented by the parameter, <c>definition</c>.
        /// </summary>
        /// <param name="type">
        /// The subject type.
        /// </param>
        /// <param name="definition">
        /// The generic definiton to check with.
        /// </param>
        /// <returns>
        /// The base type or the interface found; otherwise, <c>null</c>.
        /// </returns>
        public static Type FindGenericType(this Type type, Type definition)
        {
            // If the type conforms the given generic definition, no further check required.
            if (type.HasGenericDefinition(definition))
            {
                return type;
            }

            // If the definition is interface, we only need to check the interfaces 
            // implemented by the current type
            if (definition.IsInterface)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.HasGenericDefinition(definition))
                    {
                        return interfaceType;
                    }
                }
            }

            // If the definition is not an interface, then the current type cannot be an interface too.
            // Otherwise, we should only check the parent class types of the current type.
            else if (!type.IsInterface)
            {
                // no null check for the type required, as we are sure it is not an interface type
                while (type != typeof (object))
                {
                    if (type.HasGenericDefinition(definition))
                    {
                        return type;
                    }

                    type = type.BaseType;
                }

            }

            return null;
        }

        /// <summary>
        /// Check if this type has a generic definition 
        /// represented by the parameter, <c>definition</c>.
        /// </summary>
        /// <param name="type">
        /// The subject type.
        /// </param>
        /// <param name="definition">
        /// The generic definiton to check with.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type is genereic and has a generic definition 
        /// represented by the parameter, <c>definition</c>; 
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool HasGenericDefinition(this Type type, Type definition)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == definition;
        }

        public static MethodInfo GetQualifiedMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, QualifiedMethodBindingFlags);
        }
    }
}
