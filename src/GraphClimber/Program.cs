using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using GraphClimber.Examples;
using GraphClimber.Examples.Binary;
using GraphClimber.ExpressionCompiler;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class EnumConvert<TEnum, TUnderlying>
    {
        public static readonly Func<TEnum, TUnderlying> ToUnderlying = GetToUnderlying();
        public static readonly Func<TUnderlying, TEnum> ToEnum = GetToEnum();

        private static Func<TEnum, TUnderlying> GetToUnderlying()
        {
            return GetConvert<TEnum, TUnderlying>();
        }

        private static Func<TUnderlying, TEnum> GetToEnum()
        {
            return GetConvert<TUnderlying, TEnum>();
        }


        private static Func<TSource, TTarget> GetConvert<TSource, TTarget>()
        {
            var parameter = Expression.Parameter(typeof(TSource), "value");

            Expression<Func<TSource, TTarget>> lambda =
                Expression.Lambda<Func<TSource, TTarget>>
                    (Expression.Convert(parameter, typeof (TTarget)),
                        parameter);

            return lambda.Compile();
        } 
    
    }

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

    public class ExpressionDebugGames
    {
        private static int PrivateField = 2;

        private static int PrivateProperty
        {
            get { return 5; }
        }

        private static int PrivateMethod<T>()
        {
            return 3;
        }

        private static int GenericMethod<TEnum, TUnderlying>(TEnum value) where TEnum : IConvertible
        {
            return 2;
        }

        public static void Play()
        {

            Expression<Func<Type>> hi = () => typeof(int);

            MemberExpression propertyOrField = Expression.Field(null, typeof(ExpressionDebugGames).GetField("PrivateField", BindingFlags.Static | BindingFlags.NonPublic));
            MemberExpression property = Expression.Property(null, typeof(ExpressionDebugGames), "PrivateProperty");
            MethodCallExpression method = Expression.Call(null,
                typeof(ExpressionDebugGames).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(typeof(object)));

            var parameter = Expression.Parameter(typeof(int), "someNumberArgument");

            var variable = Expression.Variable(typeof(int), "myInt");
            var @break = Expression.Label();
            Expression exp = Expression.Assign(variable, Expression.Divide(propertyOrField, Expression.Subtract(property, method)));
            exp = Expression.Block(new[] { variable }, exp, Expression.Add(2.Constant(), 5.Constant()), Expression.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }), Expression.Constant("Hello Worlda")));


            var iVariable = Expression.Variable(typeof(int), "i");
            // exp = CustomExpression.For(iVariable, 0.Constant(), Expression.LessThan(iVariable, 5.Constant()), Expression.PostIncrementAssign(iVariable), exp);

            exp = Expression.Block(exp,
                Expression.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }),
                    Expression.Constant("Goodbye")));

            var lambda = Expression.Lambda<Action<int>>(exp, "Hello_World",
                new[] { parameter });

            var expression = new DebugExpressionCompiler(DebugViewExpressionDescriber.Empty).Compile(lambda);

            expression(56);
        }
        
    }

    class SimpleType
    {


        public ValueType SomeValueType { get; set; }

        public string String { get; set; }

        public IConvertible Convertible { get; set; }


    }

    public class Program
    {
        private static readonly IStateMemberProvider _stateMemberProvider = new CachingStateMemberProvider(new ReflectionPropertyStateMemberProvider());

        public static void DoSomething<T>(StrongBox<T> hello)
        {
            Console.WriteLine(hello);
        }


        static void Main(string[] args)
        {
            //ExpressionDebugGames.Play();
            //IStore store = new TrivialStore();

            //store.Set("A", 5);
            //store.GetInner("A").Set("A", 10);

            //SerializeDeserializeXML();

            //SerializeDeserializeStore();

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
            object person = GetSimpleTypeInstance();

            IPerson structure = new Person2() { Age = 25, Surprise = Days.Monday };
            person = new PersonHolder() { A = structure, B = structure, C = structure };

            structure.IncreaseAge();
            
            var stateMemberProvider = new BinaryStateMemberProvider(_stateMemberProvider);

            //var writeClimber = new SlowGraphClimber<BinaryWriterProcessor>(stateMemberProvider);
            //var readClimber = new SlowGraphClimber<BinaryReaderProcessor>(stateMemberProvider);

            var stream = new MemoryStream();
            var binaryWriterProcessor = new BinaryWriterProcessor(new LoggingWriter(new CompressingWriter(new BinaryWriterAdapter(new BinaryWriter(stream))), Console.Out));

            ClimbStore store2 = new ClimbStore(binaryWriterProcessor.GetType(),
                new BinaryStateMemberProvider(new PropertyStateMemberProvider()),
                new MethodMapper(),
                new TrivialExpressionCompiler());

            ClimbDelegate<StrongBox<object>> climb2 = 
                store2.GetClimb<StrongBox<object>>(typeof(StrongBox<object>));

            climb2(binaryWriterProcessor,
                new StrongBox<object>(person));

            //writeClimber.Route(person, binaryWriterProcessor, false);
            
            stream.Position = 0;
            var binaryReaderProcessor = new BinaryReaderProcessor(new LoggingReader(new DecompressingReader(new BinaryReaderAdapter(new BinaryReader(stream))), Console.Out));

            var strongBox = new StrongBox<object>();

            ClimbStore store = new ClimbStore(binaryReaderProcessor.GetType(),
                new BinaryStateMemberProvider(new PropertyStateMemberProvider()), 
                new MethodMapper(),
                new TrivialExpressionCompiler());

            ClimbDelegate<StrongBox<object>> climb = store.GetClimb<StrongBox<object>>(typeof (StrongBox<object>));

            climb(binaryReaderProcessor, strongBox);
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
                    (_stateMemberProvider);

            XElement reader = XElement.Parse(text);

            XmlReaderProcessor processor2 =
                new XmlReaderProcessor(reader);

            Person person2 = new Person();

            climber2.Climb(person2,
                processor2);


            // Writer code:
            SlowGraphClimber<XmlWriterProcessor> climber =
                new SlowGraphClimber<XmlWriterProcessor>
                    (_stateMemberProvider);

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
            var store = new TrivialStore();

            SlowGraphClimber<StoreWriterProcessor> climber = new SlowGraphClimber<StoreWriterProcessor>(_stateMemberProvider);

            var processor = new StoreWriterProcessor(store);
            var box = new StrongBox<Person>(GetPerson());

            climber.Climb(box, processor);


            SlowGraphClimber<StoreReaderProcessor> readerClimber = new SlowGraphClimber<StoreReaderProcessor>(_stateMemberProvider);

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