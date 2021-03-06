﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemRegistration.cs" company="Wasiewcz">
//   pat.wasiewicz
// </copyright>
// <summary>
//   Defines the ItemRegistration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniAutFac.Resolvable
{
    using Exceptions;
    using Helpers;
    using Parameters;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ItemRegistration : ItemRegistration<object>
    {
        internal ItemRegistration(ContainerBuilder builder, params Type[] inTypes) : base(builder, inTypes) { }
    }

    /// <summary>
    /// The builder resolvable item.
    /// </summary>
    internal class ItemRegistration<TConcreteType> : ConcreteItemRegistrationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRegistration" /> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="inTypes"></param>
        internal ItemRegistration(ContainerBuilder builder, params Type[] inTypes)
            : base(builder)
        {
            this.Parameters = new List<Parameter>();
            foreach (var inType in inTypes)
            {
                if (Types.IsRegistrationForbiddenType(inType))
                {
                    throw new RegistrationNotAllowedException("Type " + inType.FullName + " cannot be registered.");
                }
            }

            this.InTypes = inTypes;

            if (this.InTypes.Length == 1)
            {
                this.AsType = this.InTypes.Single();
            }
        }

        /// <summary>
        /// Determines the output type of registered type with builder.
        /// </summary>
        public override ItemRegistrationBase As(Type type)
        {
            var typesToValidate = this.GetTypesWithoutFactory().ToList();
            if (typesToValidate.Any(inType => !type.IsAssignableFrom(inType)))
            {
                if ((type.IsGenericType && type.IsGenericTypeDefinition) &&
                    typesToValidate.Any(
                                        inType =>
                                        inType.IsGenericType && inType.IsGenericTypeDefinition &&
                                        (inType.GetInterfaces()
                                               .Where(i => i.IsGenericType)
                                               .Select(i => i.GetGenericTypeDefinition())
                                               .Contains(type) ||
                                         (inType.BaseType != null && inType.IsGenericType && inType.BaseType == type)))) { }
                else
                {
                    throw new NotAssignableException(type);
                }
            }

            if (Types.IsRegistrationForbiddenType(type))
            {
                throw new RegistrationNotAllowedException("Type " + type.FullName + " is forbidden to register.");
            }

            this.AsType = type;

            return this;
        }

        private IEnumerable<Type> GetTypesWithoutFactory()
        {
            return this.InTypes.Where(t => typeof(object) != t && this.OwnFactory == null);
        }
    }
}