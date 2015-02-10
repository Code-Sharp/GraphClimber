using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using GraphClimber.Examples;
using GraphClimber.ValueDescriptor;

namespace GraphClimber
{
    public class Program
    {
        public static void DoSomething<T>(StrongBox<T> hello)
        {
            Console.WriteLine(hello);
        }


        static void Main(string[] args)
        {
            //IStore store = new TrivialStore();

            //store.Set("A", 5);
            //store.GetInner("A").Set("A", 10);

            //SerializeDeserializeXML();

            // SerializeDeserializeStore();

            SerializeDeserializeBinary();
        }

        private static void SerializeDeserializeBinary()
        {
            var person = GetPerson2();

            var stateMemberProvider = new BinaryStateMemberProvider(new ReflectionPropertyStateMemberProvider());

            var writeClimber = new SlowGraphClimber<BinaryWriterProcessor>(stateMemberProvider);
            var readClimber = new SlowGraphClimber<BinaryReaderProcessor>(stateMemberProvider);

            var stream = new MemoryStream();
            var binaryWriterProcessor = new BinaryWriterProcessor(new SuperBinaryWriter(stream));

            // Not that good :( : 
            // I need the field to be "object" and not the struct "Person2"
            //writeClimber.Route(person, binaryWriterProcessor);
            StrongBox<object> strongBox2 =
                new StrongBox<object>()
                {
                    Value = person
                };

            var stateMember = new StaticStateMember(person);
            
            writeClimber.Route(person, binaryWriterProcessor, false);
            //new ReflectionValueDescriptor<Person2, Person2>(binaryWriterProcessor, stateMemberProvider, stateMember, null).Route(stateMember, person.GetType(), null);

            stream.Position = 0;
            var binaryReaderProcessor = new BinaryReaderProcessor(new SuperBinaryReader(stream));

            var strongBox = new StrongBox<object>();
            readClimber.Climb(strongBox, binaryReaderProcessor);
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

            public Person Father { get; set; }
        }

        struct Person2
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public object Surprise { get; set; }

            public object Father { get; set; }
        }


        public static void SerializeDeserializeStore()
        {
            var store = new TrivialStore();

            SlowGraphClimber<StoreWriterProcessor> climber = new SlowGraphClimber<StoreWriterProcessor>(new ReflectionPropertyStateMemberProvider());

            var processor = new StoreWriterProcessor(store);
            var box = new StrongBox<Person>(GetPerson());

            climber.Climb(box, processor);


            SlowGraphClimber<StoreReaderProcessor> readerClimber = new SlowGraphClimber<StoreReaderProcessor>(new ReflectionPropertyStateMemberProvider());

            var readerProcessor = new StoreReaderProcessor(store);
            var readBox = new StrongBox<object>(null);
            readerClimber.Climb(readBox, readerProcessor);

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
                Surprise = new Person()
                {
                    Age = 21,
                    Name = "Yosi Attias",
                    Surprise = ilan
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
                Surprise = new Person2()
                {
                    Age = 21,
                    Name = "Yosi Attias",
                    Surprise =
                    new object[]
                    {
                        ilan, ilan, 342, "Hello",
                        new int[]{1,1,2,3,5,8},
                        new int[,]{{1,1},{2,3},{5,8}}
                    }
                }
            };
        }   
    }
}