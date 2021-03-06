﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphClimber
{
    internal class PropertyStateMemberProvider : IStateMemberProvider
    {
        public IEnumerable<IStateMember> Provide(Type type)
        {
            return type.GetRuntimeProperties()
                .Where(x => x.GetIndexParameters().Length == 0)
                .Select(property => new PropertyStateMember(property));
        }

        public IStateMember ProvideArrayMember(Type arrayType, int[] indices)
        {
            return new ArrayStateMember(arrayType, arrayType.GetElementType(), indices);
        }
    }
}