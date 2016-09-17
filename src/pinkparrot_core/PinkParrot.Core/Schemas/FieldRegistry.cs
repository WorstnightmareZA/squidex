﻿// ==========================================================================
//  FieldRegistry.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schemas
{
    public delegate Field FactoryFunction(long id, string name, FieldProperties properties);

    public sealed class FieldRegistry
    {
        private readonly Dictionary<string, IRegisteredField> fieldsByTypeName = new Dictionary<string, IRegisteredField>();
        private readonly Dictionary<Type, IRegisteredField> fieldsByPropertyType = new Dictionary<Type, IRegisteredField>();

        private sealed class Registered : IRegisteredField
        {
            private readonly FactoryFunction fieldFactory;
            private readonly Type propertiesType;
            private readonly string typeName;

            public Type PropertiesType
            {
                get { return propertiesType; }
            }

            public string TypeName
            {
                get { return typeName; }
            }

            public Registered(FactoryFunction fieldFactory, Type propertiesType)
            {
                typeName = TypeNameRegistry.GetName(propertiesType);

                this.fieldFactory = fieldFactory;
                this.propertiesType = propertiesType;
            }

            Field IRegisteredField.CreateField(long id, string name, FieldProperties properties)
            {
                return fieldFactory(id, name, properties);
            }
        }

        public FieldRegistry()
        {
            Add<NumberFieldProperties>((id, name, properties) => new NumberField(id, name, (NumberFieldProperties)properties));
        }

        public void Add<TFieldProperties>(FactoryFunction fieldFactory)
        {
            Guard.NotNull(fieldFactory, nameof(fieldFactory));
           
            var registered = new Registered(fieldFactory, typeof(TFieldProperties));

            fieldsByTypeName[registered.TypeName] = registered;
            fieldsByPropertyType[registered.PropertiesType] = registered;
        }

        public Field CreateField(long id, string name, FieldProperties properties)
        {
            var registered = fieldsByPropertyType[properties.GetType()];

            return registered.CreateField(id, name, properties);
        }

        public IRegisteredField FindByPropertiesType(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var registered = fieldsByPropertyType.GetOrDefault(type);

            if (registered == null)
            {
                throw new InvalidOperationException($"The field property '{type}' is not supported.");
            }

            return registered;
        }

        public IRegisteredField FindByTypeName(string typeName)
        {
            Guard.NotNullOrEmpty(typeName, nameof(typeName));

            var registered = fieldsByTypeName.GetOrDefault(typeName);

            if (registered == null)
            {
                throw new DomainException($"A field with type '{typeName} is not known.");
            }

            return registered;
        }
    }
}
