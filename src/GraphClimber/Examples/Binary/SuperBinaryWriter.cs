using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphClimber.Examples
{
    
    class SuperBinaryWriter : BinaryWriter
    {
        private readonly IDictionary<string, int> _writtenStrings = new Dictionary<string, int>();

        protected SuperBinaryWriter()
        {
        }

        public SuperBinaryWriter(Stream output) : base(output)
        {
        }

        public SuperBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
        }

        public SuperBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public override void Write(string value)
        {
            int id;
            if (_writtenStrings.TryGetValue(value, out id))
            {
                base.Write((byte)1);
                base.Write(id);
            }
            else
            {
                _writtenStrings[value] = _writtenStrings.Count;
                base.Write((byte)0);
                base.Write(value);
            }
        }
    }


    class SuperBinaryReader : BinaryReader
    {
        private readonly IList<string> _readStrings = new List<string>();

        public SuperBinaryReader(Stream input) : base(input)
        {
        }

        public SuperBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public SuperBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override string ReadString()
        {
            var isRead = base.ReadByte();

            if (isRead == 1)
            {
                return _readStrings[base.ReadInt32()];
            }

            var newString = base.ReadString();
            _readStrings.Add(newString);

            return newString;
        }

        public override void Close()
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override int PeekChar()
        {
            return base.PeekChar();
        }

        public override int Read()
        {
            return base.Read();
        }

        public override bool ReadBoolean()
        {
            return base.ReadBoolean();
        }

        public override byte ReadByte()
        {
            return base.ReadByte();
        }

        public override sbyte ReadSByte()
        {
            return base.ReadSByte();
        }

        public override char ReadChar()
        {
            return base.ReadChar();
        }

        public override short ReadInt16()
        {
            return base.ReadInt16();
        }

        public override ushort ReadUInt16()
        {
            return base.ReadUInt16();
        }

        public override int ReadInt32()
        {
            return base.ReadInt32();
        }

        public override uint ReadUInt32()
        {
            return base.ReadUInt32();
        }

        public override long ReadInt64()
        {
            return base.ReadInt64();
        }

        public override ulong ReadUInt64()
        {
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            return base.ReadDouble();
        }

        public override decimal ReadDecimal()
        {
            return base.ReadDecimal();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override char[] ReadChars(int count)
        {
            return base.ReadChars(count);
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override byte[] ReadBytes(int count)
        {
            return base.ReadBytes(count);
        }

        public override Stream BaseStream
        {
            get { return base.BaseStream; }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
