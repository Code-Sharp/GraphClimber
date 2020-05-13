using System.Xml;
using System.Xml.Linq;

namespace GraphClimber.Examples
{
    public class XmlSerializer
    {
        private readonly IGraphClimber<XmlWriterProcessor> _writerClimber;
        private readonly IGraphClimber<XElementReaderProcessor> _readerClimber;
        private readonly IGraphClimber<XmlTextReaderProcessor> _newReaderClimber;

        public XmlSerializer()
        {
            IStateMemberProvider memberProvider = new CachingStateMemberProvider(new PropertyStateMemberProvider());
            
            _newReaderClimber = DefaultGraphClimber<XmlTextReaderProcessor>.Create(memberProvider);
            _readerClimber = DefaultGraphClimber<XElementReaderProcessor>.Create(memberProvider);
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
            XElementReaderProcessor processor = new XElementReaderProcessor(element);
            Box<T> box = new Box<T>();
            _readerClimber.Climb(box, processor);
            return box.Value;
        }

        public object Deserialize(XmlReader reader)
        {
            return Deserialize<object>(reader);
        }

        public T Deserialize<T>(XmlReader reader)
        {
            XmlTextReaderProcessor processor = new XmlTextReaderProcessor(reader);
            Box<T> box = new Box<T>();
            _newReaderClimber.Climb(box, processor);
            return box.Value;
        }
    }
}