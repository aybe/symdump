using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using symdump.symfile.util;
using symdump.util;

namespace symdump.symfile
{
    public class Block
    {
        public class VarInfo
        {
            public readonly string name;
            public readonly TypeInfo typeInfo;
            public readonly TypedValue typedValue;

            public VarInfo(string name, TypeInfo typeInfo, TypedValue typedValue)
            {
                this.name = name;
                this.typeInfo = typeInfo;
                this.typedValue = typedValue;
            }

            public override string ToString()
            {
                switch (typeInfo.classType)
                {
                    case ClassType.AutoVar:
                        return $"{typeInfo.asCode(name)}; /* sp {typedValue.value} */";
                    case ClassType.Register:
                        return $"{typeInfo.asCode(name)}; /* ${(Register) typedValue.value} */";
                    case ClassType.Static:
                        return $"static {typeInfo.asCode(name)}; // offset 0x{typedValue.value:x}";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public readonly uint endLine;
        public readonly uint endOffset;

        public readonly Function function;
        public readonly List<Label> labels = new List<Label>();
        public readonly uint startLine;

        public readonly uint startOffset;
        public readonly List<Block> subBlocks = new List<Block>();
        public readonly Dictionary<string, TypeInfo> typedefs = new Dictionary<string, TypeInfo>();
        public readonly Dictionary<string, VarInfo> vars = new Dictionary<string, VarInfo>();

        public Block(uint ofs, uint ln, Function f)
            : this(null, ofs, ln, f)
        {
        }

        public Block(BinaryReader reader, uint ofs, uint ln, Function f)
        {
            startOffset = ofs;
            startLine = ln;
            function = f;

            if (reader == null)
                return;

            while (true)
            {
                var typedValue = new TypedValue(reader);

                if (reader.skipSld(typedValue))
                    continue;

                switch (typedValue.type & 0x7f)
                {
                    case 16:
                        subBlocks.Add(new Block(reader, (uint) typedValue.value, reader.ReadUInt32(), function));
                        break;
                    case 18:
                        endOffset = (uint) typedValue.value;
                        endLine = reader.ReadUInt32();
                        return;
                    case 20:
                    {
                        var ti = reader.readTypeInfo(false);
                        var memberName = reader.readPascalString();
                        Debug.Assert(!string.IsNullOrEmpty(memberName));
                        switch (ti.classType)
                        {
                            case ClassType.AutoVar:
                            case ClassType.Register:
                            case ClassType.Static:
                                vars.Add(memberName, new VarInfo(memberName, ti, typedValue));
                                break;
                            case ClassType.Typedef:
                                typedefs.Add(memberName, ti);
                                break;
                            case ClassType.Label:
                                labels.Add(new Label(typedValue, memberName));
                                break;
                            default:
                                throw new Exception($"Unexpected class type {ti.classType}");
                        }
                        break;
                    }
                    case 22:
                    {
                        var ti = reader.readTypeInfo(true);
                        var memberName = reader.readPascalString();
                        Debug.Assert(!string.IsNullOrEmpty(memberName));
                        switch (ti.classType)
                        {
                            case ClassType.AutoVar:
                            case ClassType.Register:
                            case ClassType.Static:
                                vars.Add(memberName, new VarInfo(memberName, ti, typedValue));
                                break;
                            case ClassType.Typedef:
                                typedefs.Add(memberName, ti);
                                break;
                            case ClassType.Label:
                                labels.Add(new Label(typedValue, memberName));
                                break;
                            default:
                                throw new Exception($"Unexpected class type {ti.classType}");
                        }
                        break;
                    }
                }
            }
        }

        public void dump(IndentedTextWriter writer)
        {
            writer.WriteLine($"{{ // line {startLine}, offset 0x{startOffset:x}");
            ++writer.indent;
            foreach (var t in typedefs)
                writer.WriteLine($"typedef {t.Value.asCode(t.Key)};");
            foreach (var varInfo in vars)
                writer.WriteLine(varInfo.Value);
            foreach (var l in labels)
                writer.WriteLine(l);
            subBlocks.ForEach(b => b.dump(writer));
            --writer.indent;
            writer.WriteLine($"}} // line {endLine}, offset 0x{endOffset:x}");
        }
    }
}
