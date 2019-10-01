﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace symdump.exefile.util
{
    public class EndianBinaryReader : IDisposable
    {
        private BinaryReader _stream;

        public EndianBinaryReader([NotNull] Stream s)
            : this(new BinaryReader(s))
        {
        }

        private EndianBinaryReader([NotNull] BinaryReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.BaseStream.CanRead)
                throw new ArgumentException("Stream isn't readable", nameof(stream));
            _stream = stream;
        }

        [NotNull] public Stream BaseStream => _stream.BaseStream;

        public void Dispose()
        {
            _stream.Dispose();
            _stream = null;
        }

        public byte[] ReadBytes(int n)
        {
            return _stream.ReadBytes(n);
        }

        public byte ReadByte()
        {
            return _stream.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return _stream.ReadSByte();
        }

        public short ReadInt16()
        {
            var tmp = _stream.ReadBytes(2);
            return (short) ((tmp[1] << 8) | tmp[0]);
        }

        public int ReadInt32()
        {
            var tmp = _stream.ReadBytes(4);
            return (tmp[3] << 24) | (tmp[2] << 16) | (tmp[1] << 8) | tmp[0];
        }

        public ushort ReadUInt16()
        {
            return (ushort) ReadInt16();
        }

        public uint ReadUInt32()
        {
            return (uint) ReadInt32();
        }
    }
}
