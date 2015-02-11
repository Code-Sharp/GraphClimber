using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace GraphClimber.Tests
{
    public class GenericArgumentBinderTests
    {

        [Test]
        [TestCaseSource("TestData")]
        public void Should_Return_Method_Info_When_Can(MethodInfo methodInfo, Type[] realTypes, MethodInfo expectedMethodInfo)
        {
            MethodInfo actualMethodInfo;

            Assert.That(GetTarget().TryBind(methodInfo, realTypes, out actualMethodInfo), Is.EqualTo(expectedMethodInfo != null));
            Assert.That(expectedMethodInfo, Is.EqualTo(actualMethodInfo));

        }

        private static GenericArgumentBinder GetTarget()
        {
            return new GenericArgumentBinder(new FallbackToFirstCandidateMethodSelector(new BinderMethodSelector(Type.DefaultBinder)));
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                IEnumerable<object[]> testData = GetTestData();
                IEnumerable<object[]> genericTestData = GetGenericTestData();

                return testData.Concat(genericTestData);
            }
        }

        private static IEnumerable<object[]> GetTestData()
        {
            foreach (var methodInfo in typeof (ITestData).GetMethods())
            {
                foreach (var data in methodInfo.GetCustomAttributes<TestDataAttribute>())
                {
                    yield return new object[]
                    {
                        methodInfo,
                        data.RealType,
                        data.ShouldReturnMethod ? methodInfo.MakeGenericMethod(data.GenericParameterTypes) : null
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetGenericTestData()
        {
            foreach (MethodInfo method in typeof (IGenericTestData<>).GetMethods())
            {
                foreach (GenericTestDataAttribute attribute in method.GetCustomAttributes<GenericTestDataAttribute>())
                {
                    Type genericType =
                        typeof (IGenericTestData<>).MakeGenericType(attribute.GenericArgument);

                    MethodInfo resolvedMethod = genericType.GetMethods().
                        FirstOrDefault(x => x.Name == method.Name);

                    yield return new object[]
                        {
                            resolvedMethod,
                            attribute.RealType,
                            attribute.ShouldReturnMethod ?  resolvedMethod.MakeGenericMethod(attribute.GenericParameterTypes) : null
                        };

                }
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private class TestDataAttribute : Attribute
        {
            private readonly Type[] _realType;
            private readonly bool _shouldReturnMethod;
            private readonly Type[] _genericParameterTypes;

            public TestDataAttribute(Type realType, bool shouldReturnMethod, params Type[] genericParameterTypes)
            {
                _realType = new[] { realType };
                _shouldReturnMethod = shouldReturnMethod;
                _genericParameterTypes = genericParameterTypes;
            }

            public TestDataAttribute(Type[] realType, bool shouldReturnMethod, params Type[] genericParameterTypes)
            {
                _realType = realType;
                _shouldReturnMethod = shouldReturnMethod;
                _genericParameterTypes = genericParameterTypes;
            }

            public Type[] RealType
            {
                get { return _realType; }
            }

            public bool ShouldReturnMethod
            {
                get { return _shouldReturnMethod; }
            }

            public Type[] GenericParameterTypes
            {
                get { return _genericParameterTypes; }
            }
        }

        private class GenericTestDataAttribute : TestDataAttribute
        {
            private readonly Type _genericArgument;

            public GenericTestDataAttribute(Type genericArgument, Type realType, bool shouldReturnMethod, params Type[] genericParameterTypes) : this(genericArgument, new Type[] {realType}, shouldReturnMethod, genericParameterTypes)
            {
            }

            public GenericTestDataAttribute(Type genericArgument, Type[] realType, bool shouldReturnMethod, params Type[] genericParameterTypes) : base(realType, shouldReturnMethod, genericParameterTypes)
            {
                _genericArgument = genericArgument;
            }

            public Type GenericArgument
            {
                get { return _genericArgument; }
            }
        }


        private interface IGenericTestData<T>
        {
            // NOTE: Methods here are resolved by name (in unit tests only)
            // So please ensure method name is unique. Thanks!

            [GenericTestData(typeof(int), typeof(List<string>), false)]
            [GenericTestData(typeof(int), typeof(int[]), true, typeof(int[]))]
            [GenericTestData(typeof(IEnumerable<char>), typeof(List<string>), true, typeof(List<string>))]
            [GenericTestData(typeof(string), typeof(List<string>), true, typeof(List<string>))]
            [GenericTestData(typeof(IComparable), typeof(List<int>), false)] // Since List<int> : IEnumerable<IComparable> isn't true.
            void EasyTest<TEnumerable>(TEnumerable a)
                where TEnumerable : IEnumerable<T>;

            [GenericTestData(typeof(ICloneable), typeof(List<int>), false)]
            [GenericTestData(typeof(IComparable), typeof(List<int>), true, typeof(List<int>), typeof(int))]
            void ModerateTest<TEnumerable, S>(TEnumerable a)
                where TEnumerable : IEnumerable<S>
                where S : T;
        }

        private interface ITestData
        {

            [TestData(typeof(object), true, typeof(object))]
            void Simplest<T>(T a);

            [TestData(typeof(List<List<int>>), true, typeof(List<int>), typeof(int))]
            [TestData(typeof(string[][]), true, typeof(string[]), typeof(string))]
            void HardTest<TEnumerable, TKaki>(IEnumerable<TEnumerable> a)
                where TEnumerable : IEnumerable<TKaki>;


            [TestData(typeof(int[][]), false)]
            [TestData(typeof(int[,][]), true, typeof(int))]
            void ArrayTest<TArray>(TArray[,][] array);

            [TestData(typeof(int[][]), true, typeof(int[]))]
            [TestData(typeof(string[][]), false)]
            void OtherArrayTest<TEnumerable>(TEnumerable[] array)
                where TEnumerable : IEnumerable<int>;


            [TestData(typeof(int[]), true, typeof(object))]
            [TestData(typeof(Stopwatch), true, typeof(Stopwatch))]
            void NewTest<T>(T a)
                where T : new();

            [TestData(typeof(TestClass), true, typeof(TestClass))]
            [TestData(typeof(OtherTestClass), true, typeof(OtherTestClass))]
            [TestData(typeof(OtherStuff), false)]
            void TestAllInterfaceImplementatinos<T>(T obj)
                where T : IEquatable<int>;

            [TestData(typeof(Shit<object>), true, typeof(Shit<object>))]
            [TestData(typeof(Shit<ulong>), true, typeof(Shit<ulong>))]
            [TestData(typeof(object), false)]
            void RecursiveArgTest<T>(T obj)
                where T : IEquatable<T>;

            [TestData(typeof(object), false)]
            [TestData(typeof(int), false)]
            void MissingArgTest<TMissingArg>();

            [TestData(new[] { typeof(OtherTestClass), typeof(TestClass) }, true, typeof(OtherTestClass))]
            void HalfGenericMethod<TGeneric>(TGeneric param1, IEquatable<DateTime> equatable)
                where TGeneric : IEquatable<int>;

            [TestData(new[] { typeof(OtherStuff), typeof(TestClass) }, true, typeof(OtherStuff), typeof(DateTime))]
            [TestData(new[] { typeof(Shit<int>), typeof(TestClass) }, true, typeof(Shit<int>), typeof(Shit<int>))]
            void HalfGenericMethodHard<THardGeneric, TGeneric>(THardGeneric param1, IEquatable<DateTime> equatable)
                where THardGeneric : IEquatable<TGeneric>;

        }

        class OtherTestClass : IEquatable<DateTime>, IEquatable<int>
        {
            public bool Equals(int other)
            {
                throw new NotSupportedException();
            }

            public bool Equals(DateTime other)
            {
                throw new NotSupportedException();
            }
        }

        class TestClass : IEquatable<int>, IEquatable<DateTime>
        {
            public bool Equals(int other)
            {
                throw new NotSupportedException();
            }

            public bool Equals(DateTime other)
            {
                throw new NotSupportedException();
            }
        }

        class OtherStuff : IEquatable<DateTime>
        {
            public bool Equals(DateTime other)
            {
                throw new NotSupportedException();
            }
        }

        class Shit<T> : IEquatable<Shit<T>>
        {
            public bool Equals(Shit<T> other)
            {
                throw new NotSupportedException();
            }
        }

    }
}