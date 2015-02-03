using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace GraphClimber
{
    public class Program
    {
        public static void DoSomething<[My] T>(StrongBox<T> hello)
        {
            Console.WriteLine(hello);
        }


        static void Main(string[] args)
        {
            //IStore store = new TrivialStore();

            //store.Set("A", 5);
            //store.GetInner("A").Set("A", 10);

            //SerializeDeserializeXML();

            SerializeDeserializeStore();
        }

        private static void SerializeDeserializeXML()
        {
            var text =
                @"<Person Type=""GraphClimber.Program+Person, GraphClimber, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
  <Name Type=""System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">Shani Elharrar</Name>
  <Age Type=""System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">24</Age>
  <Surprise Type=""GraphClimber.Program+Person, GraphClimber, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
    <Name>null</Name>
    <Age Type=""System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">23</Age>
    <Surprise Type=""System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">4</Surprise>
  </Surprise>
</Person>";


            // Reader code:
            SlowGraphClimber<XmlReaderProcessor> climber2 =
                new SlowGraphClimber<XmlReaderProcessor>
                    (new ReflectionPropertyStateMemberProvider());

            XElement reader = XElement.Parse(text);

            XmlReaderProcessor processor2 =
                new XmlReaderProcessor(reader);

            Person person2 = new Person();

            climber2.Climb(person2,
                processor2);


            // Writer code:
            SlowGraphClimber<XmlWriterProcessor> climber =
                new SlowGraphClimber<XmlWriterProcessor>
                    (new ReflectionPropertyStateMemberProvider());

            Person person = new Person()
            {
                Age = 24,
                Name = "Shani Elharrar",
                Surprise = new Person()
                {
                    Age = 23,
                    Name = null,
                    Surprise = 4
                }
            };

            XmlWriterProcessor processor = new XmlWriterProcessor();

            climber.Climb(person,
                processor);
        }

        class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
            
            public object Surprise { get; set; }
        }

        public static void SerializeDeserializeStore()
        {
            var store = new TrivialStore();

            SlowGraphClimber<StoreWriterProcessor> climber = new SlowGraphClimber<StoreWriterProcessor>(new ReflectionPropertyStateMemberProvider());

            var processor = new StoreWriterProcessor(store);
            var box = new StrongBox<Person>(new Person()
            {
                Age = 26,
                Name = "Elad Zelinger",
                Surprise = new Person()
                {
                    Age = 21,
                    Name = "Yosi Attias",
                    Surprise = 1
                }
            });

            climber.Climb(box, processor);
            

            SlowGraphClimber<StoreReaderProcessor> readerClimber = new SlowGraphClimber<StoreReaderProcessor>(new ReflectionPropertyStateMemberProvider());

            var readerProcessor = new StoreReaderProcessor(store);
            var readBox = new StrongBox<object>(null);
            readerClimber.Climb(readBox, readerProcessor);

        }
    }

    public class StoreReaderProcessor
    {
        private IStore _store;

        public StoreReaderProcessor(IStore store)
        {
            _store = store;
        }

        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            string type;
            if (_store.TryGet("Type", out type))
            {

                var value = (T)Activator.CreateInstance(Type.GetType(type));

                descriptor.Set(value);

                var temp = _store;
                _store = _store.GetInner(descriptor.StateMember.Name);

                descriptor.Climb();

                _store = temp;
            }
        }

        [ProcessorMethod(Precedence = 99)]
        public void ProcessPrimitives<[Primitive]T>(IWriteOnlyExactValueDescriptor<T> descriptor)
        {
            T value;
            if (_store.TryGet<T>(descriptor.StateMember.Name, out value))
            {
                descriptor.Set(value);
            }
            
        }

        [ProcessorMethod]
        public void Process(IWriteOnlyValueDescriptor<object> descriptor)
        {
            string type;

            if (_store.TryGet("Type", out type))
            { 
                descriptor.Route(new MyCustomStateMember((IReflectionStateMember)descriptor.StateMember, Type.GetType(type)), descriptor.Owner);
            }
        }
    }

    public class StoreWriterProcessor 
    {
        private IStore _store;

        public StoreWriterProcessor(IStore store)
        {
            _store = store;
        }

        [ProcessorMethod]
        public void Process<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            var value = descriptor.Get();
            _store.Set("Type", value.GetType().AssemblyQualifiedName);

            var temp = _store;

            _store = _store.GetInner(descriptor.StateMember.Name);

            descriptor.Climb();

            _store = temp;
        }

        [ProcessorMethod(Precedence = 99)]
        public void ProcessPrimitives<[Primitive]T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            var value = descriptor.Get();
            // _store.Set("Type", value.GetType());
            _store.Set(descriptor.StateMember.Name, value);
        }

    }

    public interface IGenericParameterFilter
    {

        bool PassesFilter(Type type);

    }


    internal class MyAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.GenericParameter)]
    public class PrimitiveAttribute : Attribute, IGenericParameterFilter
    {
        public bool PassesFilter(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }
    }

    public class TrivialStore : IStore
    {
        private readonly IDictionary<string, object> _store = new Dictionary<string, object>();

        public IStore GetInner(string path)
        {
            return new InnerStore(this, path);
        }

        public void Set<T>(string path, T value)
        {
            _store[path] = value;
        }

        public bool TryGet<T>(string path, out T value)
        {
            object boxedValue;
            if (_store.TryGetValue(path, out boxedValue))
            {
                value = (T) boxedValue;
                return true;
            }
            value = default(T);
            return false;
        }

        private class InnerStore : IStore
        {
            private readonly IStore _store;
            private readonly string _path;

            public InnerStore(IStore store, string path)
            {
                _store = store;
                _path = path;
            }

            public IStore GetInner(string path)
            {
                return new InnerStore(_store, GetPath(path));
            }

            public void Set<T>(string path, T value)
            {
                _store.Set(GetPath(path), value);
            }

            public bool TryGet<T>(string path, out T value)
            {
                return _store.TryGet(GetPath(path), out value);
            }

            private string GetPath(string path)
            {
                return _path + "." + path;
            }
        }

    }



    public interface IStore
    {

        IStore GetInner(string path);

        void Set<T>(string path, T value);

        bool TryGet<T>(string path, out T value);

    }


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

    public class MyCustomStateMember : IReflectionStateMember
    {
        private readonly IReflectionStateMember _underlying;
        private readonly Type _memberType;

        public MyCustomStateMember(IReflectionStateMember underlying, Type memberType)
        {
            _underlying = underlying;
            _memberType = memberType;
        }

        public string Name
        {
            get { return _underlying.Name; }
        }

        public Type OwnerType
        {
            get { return _underlying.OwnerType; }
        }

        public Type MemberType
        {
            get
            {
                return _memberType;
            }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return _underlying.GetGetExpression(obj);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _underlying.GetSetExpression(obj, value);
        }

        public object GetValue(object owner)
        {
            return _underlying.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _underlying.SetValue(owner, value);
        }
    }



    public class XmlWriterProcessor : INullProcessor
    {
        private readonly XmlWriter _writer;
        private StringWriter _stringWriter;

        public XmlWriterProcessor()
        {
            _stringWriter = new StringWriter();
            _writer = new XmlTextWriter(_stringWriter);
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
            WritePropertyName(descriptor);
            _writer.WriteValue("null");
            EndWritePropertyName();
        }
    }

    class OldGames
    {
        public static void Main2(string[] args)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder();
            MethodInfo[] methods;
            MethodInfo method;
            binder.TryBind(typeof(MyClass).GetMethod("MyMethod4"),
                new Type[] { typeof(MyClass3) },
                out methods);

            int[] array = { };
            MyStaticClass.NewTest<object>(array);
        }

        public static class MyStaticClass
        {
            public static void NewTest<T>(T a)
                where T : new()
            {

            }
        }
        public class MyClass
        {


            public void MyMethod<T, S>(T enumerable)
                where T : IEnumerable<S>
            {

            }

            public void MyMethod2<T, S, U>(T enumerable)
                where T : IEnumerable<S>
                where S : IComparable<U>
                where U : new()
            {
            }

            public void MyMethod3<T>(IEnumerable<T> enumerable)
            {

            }

            public void MyMethod4<T>(T enumerable)
                where T : IComparable<T>
            {

            }
        }

        class MyClass3 : IComparable<MyClass3>
        {
            public int CompareTo(MyClass3 other)
            {
                throw new NotImplementedException();
            }
        }

        class MyClass2 : IComparable<string>, IComparable<int>, IComparable<int[]>, ICloneable
        {
            public int CompareTo(int other)
            {
                throw new NotImplementedException();
            }

            public int CompareTo(string other)
            {
                throw new NotImplementedException();
            }

            public object Clone()
            {
                throw new NotImplementedException();
            }

            public int CompareTo(int[] other)
            {
                throw new NotImplementedException();
            }
        }

        public interface IGraphClimber<TProcessor>
        {

            void Climb(object parent, TProcessor processor);

        }

        public class MyInheritedProcessor : IWriteProcessor<IAsyncResult>, IProcessor<int[]>, IRevisitedFilter
        {
            public void ProcessForWrite(IWriteOnlyValueDescriptor<IAsyncResult> descriptor)
            {
                // Sets all fields that are assignable from IAsyncResult to null.
                // (i.e: all fields that have static type object or IAsyncResult)
                descriptor.Set((IAsyncResult)null);
            }

            public void ProcessForReadWrite(IReadWriteValueDescriptor<int[]> descriptor)
            {
                int[] value = descriptor.Get();
                value[0] = 4;
                descriptor.Set(value);
            }

            public bool Visited(object obj)
            {
                return false;
            }
        }

        public class MyInheritedProcessor2 : IReadWriteProcessor<IAsyncResult>
        {
            public void ProcessForReadWrite(IReadWriteValueDescriptor<IAsyncResult> descriptor)
            {
                // Sets all field that are assignable from IAsyncResult and are
                // actually currently from type IAsyncResult, to null.
                descriptor.Set((IAsyncResult)null);
            }
        }
    }
}