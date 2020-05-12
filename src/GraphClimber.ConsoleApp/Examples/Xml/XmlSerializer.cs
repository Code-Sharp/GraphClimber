using System.Xml;
using System.Xml.Linq;

namespace GraphClimber.Examples
{
    public class XmlSerializer
    {
        private readonly IGraphClimber<XmlWriterProcessor> _writerClimber;
        private readonly IGraphClimber<XmlReaderProcessor> _readerClimber;

        public XmlSerializer()
        {
            IStateMemberProvider memberProvider = new CachingStateMemberProvider(new PropertyStateMemberProvider());
            
            _readerClimber = DefaultGraphClimber<XmlReaderProcessor>.Create(memberProvider);
            _writerClimber = DefaultGraphClimber<XmlWriterProcessor>.Create(memberProvider);
        }

        public void Serialize<T>(T value, XmlWriter writer)
        {
            XmlWriterProcessor processor = new XmlWriterProcessor(writer);
            _writerClimber.Climb(new Box<T> {Value = value}, processor);
        }

        public object Deserialize(XElement element)
        {
            return Deserialize<object>(element);
        }

        public T Deserialize<T>(XElement element)
        {
            XmlReaderProcessor processor = new XmlReaderProcessor(element);
            Box<T> box = new Box<T>();
            _readerClimber.Climb(box, processor);
            return box.Value;
        }
    }
}