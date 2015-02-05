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
    }
}
