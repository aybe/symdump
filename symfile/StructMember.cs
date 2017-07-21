﻿using System;
using System.Diagnostics;
using System.IO;
using core;
using symfile.util;

namespace symfile
{
    public class StructMember : IEquatable<StructMember>
    {
        public readonly string name;
        public readonly TypedValue typedValue;
        public readonly TypeInfo typeInfo;
        public readonly IMemoryLayout memoryLayout;

        public StructMember(TypedValue tv, BinaryReader reader, bool extended, IDebugSource debugSource)
        {
            typeInfo = reader.readTypeInfo(extended, debugSource);
            name = reader.readPascalString();
            typedValue = tv;

            if (typeInfo.classType != ClassType.Bitfield && typeInfo.classType != ClassType.StructMember &&
                typeInfo.classType != ClassType.UnionMember && typeInfo.classType != ClassType.EndOfStruct)
                throw new Exception($"Unexpected class {typeInfo.classType}");

            memoryLayout = typeInfo.typeDef.memoryLayout; // symFile.findTypeDefinition(typeInfo.tag);
        }

        public bool Equals(StructMember other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(name, other.name) && typeInfo.Equals(other.typeInfo) &&
                   typedValue.Equals(other.typedValue);
        }

        public override string ToString()
        {
            switch (typeInfo.classType)
            {
                case ClassType.Bitfield:
                    return typeInfo.asDeclaration(name) +
                           $" : {typeInfo.size}; // offset={typedValue.value / 8}.{typedValue.value % 8}";
                case ClassType.StructMember:
                case ClassType.UnionMember:
                    return typeInfo.asDeclaration(name) +
                           $"; // size={typeInfo.size}, offset={typedValue.value}";
                default:
                    throw new Exception($"Unexpected class {typeInfo.classType}");
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StructMember) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = name != null ? name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (typeInfo != null ? typeInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (typedValue != null ? typedValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}