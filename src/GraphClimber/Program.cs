using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace GraphClimber
{
    class Program
    {

        static void Main(string[] args)
        {
            SlowGraphClimber<XmlWriterProccessor> climber =
                new SlowGraphClimber<XmlWriterProccessor>
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

            XmlWriterProccessor processor = new XmlWriterProccessor();

            climber.Climb(person,
                processor);
        }

        class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
            
            public object Surprise { get; set; }
        }
    }

    

    public class XmlWriterProccessor : INullProcessor
    {
        private readonly XmlWriter _writer;
        private StringWriter _stringWriter;

        public XmlWriterProccessor()
        {
            _stringWriter = new StringWriter();
            _writer = new XmlTextWriter(_stringWriter);
        }

        [ProcessorMethod]
        public void Process(IReadOnlyValueDescriptor<string> descriptor)
        {
            WritePropertyName(descriptor);
            _writer.WriteValue(descriptor.Get());
            EndWritePropertyName<int>();
        }

        [ProcessorMethod]
        public void Process(IReadOnlyValueDescriptor<int> descriptor)
        {
            WritePropertyName(descriptor);
            _writer.WriteValue(descriptor.Get());
            EndWritePropertyName<int>();
        }

        // "Generic Processor"
        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            WritePropertyName(descriptor);

            descriptor.Climb();

            EndWritePropertyName<int>();
        }

        private void WritePropertyName(IValueDescriptor descriptor)
        {
            _writer.WriteStartElement(descriptor.StateMember.Name);
        }

        private void EndWritePropertyName<T>()
        {
            _writer.WriteEndElement();
        }

        public void ProcessNull<TField>(IWriteValueDescriptor<TField> descriptor)
        {
            WritePropertyName(descriptor);
            _writer.WriteValue("null");
            EndWritePropertyName<int>();
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
            public void ProcessForWrite(IWriteValueDescriptor<IAsyncResult> descriptor)
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