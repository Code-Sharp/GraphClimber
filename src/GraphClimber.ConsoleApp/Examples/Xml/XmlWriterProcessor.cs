using System;
using System.IO;
using System.Xml;

namespace GraphClimber.Examples
{
    public class XmlWriterProcessor : INullProcessor
    {
        private readonly XmlWriter _writer;

        public XmlWriterProcessor(XmlWriter writer)
        {
            _writer = writer;
        }

        [ProcessorMethod]
        public void ProcessString(IReadOnlyValueDescriptor<string> descriptor)
        {
            WritePropertyName(descriptor);
            _writer.WriteValue(descriptor.Get());
            EndWritePropertyName();
        }

        [ProcessorMethod]
        public void ProcessInt32(IReadOnlyValueDescriptor<int> descriptor)
        {
            WritePropertyName(descriptor);
            _writer.WriteValue(descriptor.Get());
            EndWritePropertyName();
        }

        // "Generic Processor"
        [ProcessorMethod(Precedence = 101)]
        public void ProcessGeneric<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            WritePropertyName(descriptor);

            descriptor.Climb();

            EndWritePropertyName();
        }

        private void WritePropertyName<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            Type type = typeof (T);

            WritePropertyName((IValueDescriptor)descriptor);
            _writer.WriteAttributeString("Type", type.AssemblyQualifiedName);
        }

        private void WritePropertyName(IValueDescriptor descriptor)
        {
            IStateMember stateMember = descriptor.StateMember;
            _writer.WriteStartElement(stateMember.Name);
        }

        private void EndWritePropertyName()
        {
            _writer.WriteEndElement();
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            //WritePropertyName(descriptor);
            //_writer.WriteValue("null");
            //EndWritePropertyName();
        }
    }
}