using System;
using System.Xml.Linq;

namespace GraphClimber.Examples
{
    internal class XmlReaderProcessor
    {
        private XElement _reader;

        public XmlReaderProcessor(XElement reader)
        {
            _reader = reader;
        }

        [ProcessorMethod]
        public void ProcessInt32(IWriteOnlyExactValueDescriptor<int> descriptor)
        {
            XElement element = 
                _reader.Element(descriptor.StateMember.Name);

            int result = 
                Convert.ToInt32(element.Value);

            descriptor.Set(result);
        }

        [ProcessorMethod]
        public void ProcessString(IWriteOnlyExactValueDescriptor<string> descriptor)
        {
            XElement element =
                _reader.Element(descriptor.StateMember.Name);

            string result = element.Value;

            descriptor.Set(result);
        }

        [ProcessorMethod(Precedence = 102)]
        public void ProcessGeneric<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            XElement temp = _reader;

            _reader = _reader.Element(descriptor.StateMember.Name);

            CreateObject(descriptor);

            descriptor.Climb();

            _reader = temp;
        }

        private void CreateObject<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            XAttribute attribute = _reader.Attribute("Type");

            if (attribute != null)
            {
                var type = attribute.Value;

                Type instanceType = Type.GetType(type);

                T field =
                    (T) Activator.CreateInstance(instanceType);

                descriptor.Set(field);
            }
        }

        [ProcessorMethod]
        public void ProcessObject(IWriteOnlyExactValueDescriptor<object> descriptor)
        {
            XElement element = _reader.Element(descriptor.StateMember.Name);
            
            XAttribute attribute = element.Attribute("Type");

            if (attribute != null)
            {
                var type = attribute.Value;

                Type instanceType = Type.GetType(type);

                // TODO: this will be the route method..
                descriptor.Route
                    (new MyCustomStateMember((IReflectionStateMember) descriptor.StateMember,
                        instanceType), descriptor.Owner);
            }
        }

        
    }
}