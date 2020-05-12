using System;
using System.Collections.Generic;
using System.Reflection;

namespace GraphClimber.Examples
{
    class OldGames
    {
        public static void Main2(string[] args)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder(new FallbackToFirstCandidateMethodSelector(new BinderMethodSelector(Type.DefaultBinder)));
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