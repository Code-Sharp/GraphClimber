using System;
using System.Xml.Linq;

namespace GraphClimber.Examples
{
    internal class XElementReaderProcessor
    {
        private XElement _reader;

        public XElementReaderProcessor(XElement reader)
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

            if ((element != null) && !element.IsEmpty)
            {
                string result = element.Value;

                descriptor.Set(result);
            }
            else
            {
                descriptor.Set(null);
            }
        }

        [ProcessorMethod(Precedence = 102)]
        public void ProcessGeneric<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            XElement temp = _reader;

            if (!(descriptor.Owner is Box))
            {
                _reader = _reader.Element(descriptor.StateMember.Name);
            }

            if (_reader != null)
            {
                if (CreateObject(descriptor))
                {
                    descriptor.Climb();
                }
            }

            _reader = temp;
        }

        private bool CreateObject<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            XAttribute attribute = _reader.Attribute("Type");

            if (attribute != null)
            {
                var type = attribute.Value;

                Type instanceType = Type.GetType(type);

                T field =
                    (T) Activator.CreateInstance(instanceType);

                descriptor.Set(field);

                return true;
            }

            return false;
        }

        [ProcessorMethod]
        public void ProcessObject(IWriteOnlyExactValueDescriptor<object> descriptor)
        {
            XElement element = _reader;

            if (!(descriptor.Owner is Box))
            {
                element = _reader.Element(descriptor.StateMember.Name);
            }
            
            XAttribute attribute = element.Attribute("Type");

            if (attribute != null)
            {
                var type = attribute.Value;

                Type instanceType = Type.GetType(type);

                // TODO: this will be the route method..
                descriptor.Route
                    (new MyCustomStateMember(descriptor.StateMember,
                        instanceType), descriptor.Owner, true);
            }
        }
    }
}