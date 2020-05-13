using System;
using System.Xml;
using System.Xml.Linq;

namespace GraphClimber.Examples
{
    public class XmlTextReaderProcessor
    {
        private readonly XmlReader _reader;

        public XmlTextReaderProcessor(XmlReader reader)
        {
            _reader = reader;
        }

        [ProcessorMethod(Precedence = 104)]
        public void ProcessGeneric<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            XmlReader reader = _reader;

            IStateMember member = descriptor.StateMember;

            object owner = CreateObject(descriptor);

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (descriptor.Children.TryGetValue(reader.Name, out member))
                    {
                        descriptor.Route(member, owner, false);
                    }
                }
            }
        }

        [ProcessorMethod]
        public void ProcessObject(IWriteOnlyExactValueDescriptor<object> descriptor)
        {
            IStateMember member = descriptor.StateMember;

            if (descriptor.Owner is Box)
            {
                _reader.ReadToFollowing(member.Name);
            }

            string type = _reader.GetAttribute("Type");

            Type instanceType = Type.GetType(type);

            // TODO: this will be the route method..
            descriptor.Route
                (new MyCustomStateMember(descriptor.StateMember,
                                         instanceType), descriptor.Owner, true);
        }

        private T CreateObject<T>
            (IWriteOnlyValueDescriptor<T> descriptor)
        {
            string type = _reader.GetAttribute("Type");

            if (type != null)
            {
                Type instanceType = Type.GetType(type);

                T field =
                    (T) Activator.CreateInstance(instanceType);

                descriptor.Set(field);

                return field;
            }

            return default(T);
        }

        [ProcessorMethod]
        public void ProcessInt32(IWriteOnlyExactValueDescriptor<int> descriptor)
        {
            int value = _reader.ReadElementContentAsInt();
            descriptor.Set(value);
        }


        [ProcessorMethod]
        public void ProcessString(IWriteOnlyExactValueDescriptor<string> descriptor)
        {
            string value = _reader.ReadElementContentAsString();
            descriptor.Set(value);
        }
    }
}