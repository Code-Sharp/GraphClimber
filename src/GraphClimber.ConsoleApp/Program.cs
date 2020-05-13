using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using GraphClimber.ConsoleApp.Examples.Binary;
using GraphClimber.Examples;
using GraphClimber.Examples.Binary;

namespace GraphClimber
{
    enum Days
    {
        Sunday,
        Monday,
        Tuesday,
        Wendensday,
        Thursday,
        Friday,
        Saturday
    }

    class SimpleType
    {


        public ValueType SomeValueType { get; set; }

        public string String { get; set; }

        public IConvertible Convertible { get; set; }


    }

    public class Program
    {
        private static readonly IStateMemberProvider _stateMemberProvider = new CachingStateMemberProvider(new PropertyStateMemberProvider());

        public static void DoSomething<T>(StrongBox<T> hello)
        {
            Console.WriteLine(hello);
        }


        static void Main(string[] args)
        {
            //ExpressionDebugGames.Play();
            IStore store = new TrivialStore();

            store.Set("A", 5);
            store.GetInner("A").Set("A", 10);

            SerializeDeserializeXML();

            SerializeDeserializeStore();

            SerializeDeserializeBinary();
        }

        public static object GetSimpleTypeInstance()
        {
            return new SimpleType()
            {
                Convertible = "sadasd",
                SomeValueType = Days.Monday,
                String = "Hello W0rld"
            };
        }

        public class PersonHolder
        {
            public IPerson A { get; set; }
            public IPerson B { get; set; }

            public IPerson C { get; set; }
        }

        private static void SerializeDeserializeBinary()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            //object person = GetPerson();

            IPerson structure = new Person2() { Age = 25, Surprise = Days.Monday };
            object person = new PersonHolder() { A = structure, B = structure, C = structure };

            structure.IncreaseAge();

            var stream = new MemoryStream();
            IWriter loggingWriter = new LoggingWriter(new CompressingWriter(new BinaryWriterAdapter(new BinaryWriter(stream))), Console.Out);

            formatter.Serialize(loggingWriter, person);

            stream.Position = 0;
            IReader loggingReader = new LoggingReader(new DecompressingReader(new BinaryReaderAdapter(new BinaryReader(stream))), Console.Out);

            object deserialized = formatter.Deserialize(loggingReader);
        }

        private static void SerializeDeserializeXML()
        {
            XmlSerializer serializer = new XmlSerializer();

            var text =
                @"<Value Type=""GraphClimber.Program+Person, GraphClimber.ConsoleApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
  <Name Type=""System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"">Shani Elharrar</Name>
  <Age Type=""System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"">24</Age>
  <Surprise Type=""GraphClimber.Program+Person, GraphClimber.ConsoleApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
    <Age Type=""System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"">23</Age>
    <Surprise Type=""System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"">4</Surprise>
  </Surprise>
</Value>";

            // Writer code:
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

            StringWriter stringWriter = new StringWriter();
            
            serializer.Serialize(person, new XmlTextWriter(stringWriter));

            string xml = stringWriter.ToString();

            object result = serializer.Deserialize(XElement.Parse(xml));
            object result2 = serializer.Deserialize(XmlReader.Create(new StringReader(xml)));
        }

        public struct MyClass
        {
            public string Name { get; set; }
        }

        class Person
        {
            public Person()
            {
                //Children = new List<Person>();
            }

            public string Name { get; set; }

            public int Age { get; set; }

            public object Surprise { get; set; }

            public Person Father { get; set; }

            //public IList<Person> Children { get; set; }
        }

        internal struct Person2 : IPerson
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public object Surprise { get; set; }

            public object Father { get; set; }

            public Days Day { get; set; }
            public void IncreaseAge()
            {
                Age++;
            }
        }


        public static void SerializeDeserializeStore()
        {
            IStore store = new TrivialStore();
            StoreSerializer serializer = new StoreSerializer();

            Person person = GetPerson();
            serializer.Write(person, store);

            object copy = serializer.Read(store);
        }

        private static Person GetPerson()
        {
            var ilan = new Person { Age = 26 + 25, Name = "Ilan Zelingher", Surprise = 1 };
            ilan.Surprise = ilan;
            return new Person()
            {
                Age = 26,
                Name = "Elad Zelinger",
                Father = ilan,
                //Children = { new Person() { Name = "Jason"}, new Person() { Name = "Tomerh" }},
                Surprise = new Person()
                {
                    Age = 21,
                    Name = "Yosi Attias",
                    Surprise = new MyClass()
                    {
                        Name = "YO!"
                    }
                }
            };
        }

        private static Person2 GetPerson2()
        {
            Person2 ilan = new Person2 { Age = 26 + 25, Name = "Ilan Zelingher" };

            ilan.Surprise = ilan;
            return new Person2()
            {
                Age = 26,
                Name = "Elad Zelinger",
                Father = ilan,
                Day = Days.Tuesday,
                Surprise = new Person2()
                {
                    Age = 21,
                    Name = "Yosi Attias",
                    Surprise =ilan
                        //new object[]
                        //{
                        //    ilan, ilan, 342, "Hello",
                        //    new int[,]{{1,1},{2,3},{5,8}},
                        //    new int[]{1,1,2,3,5,8},
                        //    Days.Saturday
                        //}
                }
            };

        }   
    }

    public interface IPerson
    {

        void IncreaseAge();

    }
}