using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GraphClimber.Examples.Binary
{
    public interface IWriter : IDisposable
    {
        void Flush();
        void Write(bool value);
        void Write(byte value);
        void Write(sbyte value);
        void Write(byte[] buffer);
        void Write(byte[] buffer, int index, int count);
        void Write(char ch);
        void Write(char[] chars);
        void Write(char[] chars, int index, int count);
        void Write(double value);
        void Write(decimal value);
        void Write(short value);
        void Write(ushort value);
        void Write(int value);
        void Write(uint value);
        void Write(long value);
        void Write(ulong value);
        void Write(float value);
        void Write(string value);

       
    }

    public interface IReader
    {
        string ReadString();
        int PeekChar();
        int Read();
        bool ReadBoolean();
        byte ReadByte();
        sbyte ReadSByte();
        char ReadChar();
        short ReadInt16();
        ushort ReadUInt16();
        int ReadInt32();
        uint ReadUInt32();
        long ReadInt64();
        ulong ReadUInt64();
        float ReadSingle();
        double ReadDouble();
        decimal ReadDecimal();
        int Read(char[] buffer, int index, int count);
        char[] ReadChars(int count);
        int Read(byte[] buffer, int index, int count);
        byte[] ReadBytes(int count);
        void Dispose();
    }

    public class BinaryWriterAdapter : IWriter
    {
        private readonly BinaryWriter _underlying;

        public BinaryWriterAdapter(BinaryWriter underlying)
        {
            _underlying = underlying;
        }

        public void Flush()
        {
            _underlying.Flush();
        }

        public void Write(bool value)
        {
            _underlying.Write(value);
        }

        public void Write(byte value)
        {
            _underlying.Write(value);
        }

        public void Write(sbyte value)
        {
            _underlying.Write(value);
        }

        public void Write(byte[] buffer)
        {
            _underlying.Write(buffer);
        }

        public void Write(byte[] buffer, int index, int count)
        {
            _underlying.Write(buffer, index, count);
        }

        public void Write(char ch)
        {
            _underlying.Write(ch);
        }

        public void Write(char[] chars)
        {
            _underlying.Write(chars);
        }

        public void Write(char[] chars, int index, int count)
        {
            _underlying.Write(chars, index, count);
        }

        public void Write(double value)
        {
            _underlying.Write(value);
        }

        public void Write(decimal value)
        {
            _underlying.Write(value);
        }

        public void Write(short value)
        {
            _underlying.Write(value);
        }

        public void Write(ushort value)
        {
            _underlying.Write(value);
        }

        public void Write(int value)
        {
            _underlying.Write(value);
        }

        public void Write(uint value)
        {
            _underlying.Write(value);
        }

        public void Write(long value)
        {
            _underlying.Write(value);
        }

        public void Write(ulong value)
        {
            _underlying.Write(value);
        }

        public void Write(float value)
        {
            _underlying.Write(value);
        }

        public void Write(string value)
        {
            _underlying.Write(value);
        }

        public void Dispose()
        {
            _underlying.Dispose();
        }
    }

    public class BinaryReaderAdapter : IReader
    {
        private readonly BinaryReader _underlying;

        public BinaryReaderAdapter(BinaryReader underlying)
        {
            _underlying = underlying;
        }

        public int PeekChar()
        {
            return _underlying.PeekChar();
        }

        public int Read()
        {
            return _underlying.Read();
        }

        public bool ReadBoolean()
        {
            return _underlying.ReadBoolean();
        }

        public byte ReadByte()
        {
            return _underlying.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return _underlying.ReadSByte();
        }

        public char ReadChar()
        {
            return _underlying.ReadChar();
        }

        public short ReadInt16()
        {
            return _underlying.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return _underlying.ReadUInt16();
        }

        public int ReadInt32()
        {
            return _underlying.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return _underlying.ReadUInt32();
        }

        public long ReadInt64()
        {
            return _underlying.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            return _underlying.ReadUInt64();
        }

        public float ReadSingle()
        {
            return _underlying.ReadSingle();
        }

        public double ReadDouble()
        {
            return _underlying.ReadDouble();
        }

        public decimal ReadDecimal()
        {
            return _underlying.ReadDecimal();
        }

        public string ReadString()
        {
            return _underlying.ReadString();
        }

        public int Read(char[] buffer, int index, int count)
        {
            return _underlying.Read(buffer, index, count);
        }

        public char[] ReadChars(int count)
        {
            return _underlying.ReadChars(count);
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return _underlying.Read(buffer, index, count);
        }

        public byte[] ReadBytes(int count)
        {
            return _underlying.ReadBytes(count);
        }

        public void Dispose()
        {
            _underlying.Dispose();
        }
    }

    public abstract class BaseWriterDecorator : IWriter
    {
        private readonly IWriter _underlying;

        public BaseWriterDecorator(IWriter underlying)
        {
            _underlying = underlying;
        }

        public virtual void Dispose()
        {
            _underlying.Dispose();
        }

        public virtual void Flush()
        {
            _underlying.Flush();
        }

        public virtual void Write(bool value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(byte value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(sbyte value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(byte[] buffer)
        {
            _underlying.Write(buffer);
        }

        public virtual void Write(byte[] buffer, int index, int count)
        {
            _underlying.Write(buffer, index, count);
        }

        public virtual void Write(char ch)
        {
            _underlying.Write(ch);
        }

        public virtual void Write(char[] chars)
        {
            _underlying.Write(chars);
        }

        public virtual void Write(char[] chars, int index, int count)
        {
            _underlying.Write(chars, index, count);
        }

        public virtual void Write(double value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(decimal value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(short value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(ushort value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(int value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(uint value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(long value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(ulong value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(float value)
        {
            _underlying.Write(value);
        }

        public virtual void Write(string value)
        {
            _underlying.Write(value);
        }
    }

    public abstract class BaseReaderDecorator : IReader
    {
        private readonly IReader _underlying;

        protected BaseReaderDecorator(IReader underlying)
        {
            _underlying = underlying;
        }

        public virtual string ReadString()
        {
            return _underlying.ReadString();
        }

        public virtual int PeekChar()
        {
            return _underlying.PeekChar();
        }

        public virtual int Read()
        {
            return _underlying.Read();
        }

        public virtual bool ReadBoolean()
        {
            return _underlying.ReadBoolean();
        }

        public virtual byte ReadByte()
        {
            return _underlying.ReadByte();
        }

        public virtual sbyte ReadSByte()
        {
            return _underlying.ReadSByte();
        }

        public virtual char ReadChar()
        {
            return _underlying.ReadChar();
        }

        public virtual short ReadInt16()
        {
            return _underlying.ReadInt16();
        }

        public virtual ushort ReadUInt16()
        {
            return _underlying.ReadUInt16();
        }

        public virtual int ReadInt32()
        {
            return _underlying.ReadInt32();
        }

        public virtual uint ReadUInt32()
        {
            return _underlying.ReadUInt32();
        }

        public virtual long ReadInt64()
        {
            return _underlying.ReadInt64();
        }

        public virtual ulong ReadUInt64()
        {
            return _underlying.ReadUInt64();
        }

        public virtual float ReadSingle()
        {
            return _underlying.ReadSingle();
        }

        public virtual double ReadDouble()
        {
            return _underlying.ReadDouble();
        }

        public virtual decimal ReadDecimal()
        {
            return _underlying.ReadDecimal();
        }

        public virtual int Read(char[] buffer, int index, int count)
        {
            return _underlying.Read(buffer, index, count);
        }

        public virtual char[] ReadChars(int count)
        {
            return _underlying.ReadChars(count);
        }

        public virtual int Read(byte[] buffer, int index, int count)
        {
            return _underlying.Read(buffer, index, count);
        }

        public virtual byte[] ReadBytes(int count)
        {
            return _underlying.ReadBytes(count);
        }

        public virtual void Dispose()
        {
            _underlying.Dispose();
        }
    }

    public class CompressingWriter : BaseWriterDecorator
    {
        
        private readonly IDictionary<string, int> _writtenStrings = new Dictionary<string, int>(); 
        
        public CompressingWriter(IWriter underlying) : base(underlying)
        {
        }

        public override void Write(string value)
        {
            int id;
            if (_writtenStrings.TryGetValue(value, out id))
            {
                Write((byte)1);
                Write(id);
            }
            else
            {
                _writtenStrings[value] = _writtenStrings.Count;
                Write((byte)0);
                base.Write(value);
            }
        }
    }

    public class DecompressingReader : BaseReaderDecorator
    {
        private readonly IList<string> _readStrings = new List<string>();

        public DecompressingReader(IReader underlying) : base(underlying)
        {
        }

        public override string ReadString()
        {
            var isRead = ReadByte();

            if (isRead == 1)
            {
                return _readStrings[ReadInt32()];
            }

            var newString = base.ReadString();
            _readStrings.Add(newString);

            return newString;
        }
    }

    public class LoggingWriter : BaseWriterDecorator
    {
        private readonly TextWriter _output;

        public LoggingWriter(IWriter underlying, TextWriter output) : base(underlying)
        {
            _output = output;
        }

        public override void Dispose()
        {
            _output.WriteLine("Dispose()");
            base.Dispose();
        }

        public override void Flush()
        {
            _output.WriteLine("Flush()");
            base.Flush();
        }

        public override void Write(bool value)
        {
            _output.WriteLine("Write(bool {0})", value);
            base.Write(value);
        }

        public override void Write(byte value)
        {
            _output.WriteLine("Write(byte {0})", value);
            base.Write(value);
        }

        public override void Write(sbyte value)
        {
            _output.WriteLine("Write(sbyte {0})", value);
            base.Write(value);
        }

        public override void Write(byte[] buffer)
        {
            _output.WriteLine("Write(buffer {0})", buffer);
            base.Write(buffer);
        }

        public override void Write(byte[] buffer, int index, int count)
        {
            _output.WriteLine("Write(buffer {0}, index {1}, count {2})", buffer, index, count);
            base.Write(buffer, index, count);
        }

        public override void Write(char ch)
        {
            _output.WriteLine("Write(char {0})", ch);
            base.Write(ch);
        }

        public override void Write(char[] chars)
        {
            _output.WriteLine("Write(chars {0})", chars);
            base.Write(chars);
        }

        public override void Write(char[] chars, int index, int count)
        {
            _output.WriteLine("Write(chars {0}, index {1}, count {2})", chars, index, count);
            base.Write(chars, index, count);
        }

        public override void Write(double value)
        {
            _output.WriteLine("Write(double {0})", value);
            base.Write(value);
        }

        public override void Write(decimal value)
        {
            _output.WriteLine("Write(decimal {0})", value);
            base.Write(value);
        }

        public override void Write(short value)
        {
            _output.WriteLine("Write(short {0})", value);
            base.Write(value);
        }

        public override void Write(ushort value)
        {
            _output.WriteLine("Write(ushort {0})", value);
            base.Write(value);
        }

        public override void Write(int value)
        {
            _output.WriteLine("Write(int {0})", value);
            base.Write(value);
        }

        public override void Write(uint value)
        {
            _output.WriteLine("Write(uint {0})", value);
            base.Write(value);
        }

        public override void Write(long value)
        {
            _output.WriteLine("Write(long {0})", value);
            base.Write(value);
        }

        public override void Write(ulong value)
        {
            _output.WriteLine("Write(ulong {0})", value);
            base.Write(value);
        }

        public override void Write(float value)
        {
            _output.WriteLine("Write(float {0})", value);
            base.Write(value);
        }

        public override void Write(string value)
        {
            _output.WriteLine("Write(string {0})", value);
            base.Write(value);
        }

    }

    public class LoggingReader : BaseReaderDecorator
    {
        private readonly TextWriter _output;

        public LoggingReader(IReader underlying, TextWriter output) : base(underlying)
        {
            _output = output;
        }

        private void LogTrivialCall([CallerMemberName] string callerName = null)
        {
            _output.WriteLine("{0}()", callerName);
        }

        public override string ReadString()
        {
            LogTrivialCall();
            return base.ReadString();
        }

        public override int PeekChar()
        {
            LogTrivialCall();
            return base.PeekChar();
        }

        public override int Read()
        {
            LogTrivialCall();
            return base.Read();
        }

        public override bool ReadBoolean()
        {
            LogTrivialCall();
            return base.ReadBoolean();
        }

        public override byte ReadByte()
        {
            LogTrivialCall();
            return base.ReadByte();
        }

        public override sbyte ReadSByte()
        {
            LogTrivialCall();
            return base.ReadSByte();
        }

        public override char ReadChar()
        {
            LogTrivialCall();
            return base.ReadChar();
        }

        public override short ReadInt16()
        {
            LogTrivialCall();
            return base.ReadInt16();
        }

        public override ushort ReadUInt16()
        {
            LogTrivialCall();
            return base.ReadUInt16();
        }

        public override int ReadInt32()
        {
            LogTrivialCall();
            return base.ReadInt32();
        }

        public override uint ReadUInt32()
        {
            LogTrivialCall();
            return base.ReadUInt32();
        }

        public override long ReadInt64()
        {
            LogTrivialCall();
            return base.ReadInt64();
        }

        public override ulong ReadUInt64()
        {
            LogTrivialCall();
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            LogTrivialCall();
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            LogTrivialCall();
            return base.ReadDouble();
        }

        public override decimal ReadDecimal()
        {
            LogTrivialCall();
            return base.ReadDecimal();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            _output.WriteLine("Read(buffer {0}, index {1}, count {2})", buffer, index, count);
            return base.Read(buffer, index, count);
        }

        public override char[] ReadChars(int count)
        {
            _output.WriteLine("ReadChars(count {0})", count);
            return base.ReadChars(count);
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            _output.WriteLine("Read(buffer {0}, index {1}, count {2})", buffer, index, count);
            return base.Read(buffer, index, count);
        }

        public override byte[] ReadBytes(int count)
        {
            _output.WriteLine("ReadBytes(count {0})", count);
            return base.ReadBytes(count);
        }

        public override void Dispose()
        {
            LogTrivialCall();
            base.Dispose();
        }
    }
}
