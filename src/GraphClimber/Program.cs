using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GraphClimber
{

    class Program
    {
        static void Main(string[] args)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder();
            MethodInfo[] methods;
            MethodInfo method;
            binder.TryBind(typeof (MyClass).GetMethod("MyMethod3"),
                new Type[] {typeof (MyClass2[])},
                out method);

            int[] array = {};
            MyStaticClass.NewTest<object>(array);
        } 
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

        public void MyMethod2<T, S>(T enumerable)
             where T : IEnumerable<S>
             where S : class 
         {

         }
        public void MyMethod3<T>(IEnumerable<T> enumerable)
        {

        }
    
    }

    class MyClass2 : IComparable<string>, IComparable<int>
    {
        public int CompareTo(int other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(string other)
        {
            throw new NotImplementedException();
        }
    }

    public interface IGraphClimber<TProcessor>
    {

        void Climb(object o, TProcessor processor);

    }

    public class MyInheritedProcessor : IProcessor<int[]>, IRevisitedFilter
//	, IInheritedProcessor<IAsyncResult>
    {

		// This thing doesn't compile on mono for some reason...
//        public void Process<TReal>(TReal value, IValueDescriptor<IAsyncResult> descriptor) where TReal : IAsyncResult
//        {
//            descriptor.Set((IAsyncResult)null);
//        }

        public void Process(int[] value, IValueDescriptor<int[]> descriptor)
        {
            value[0] = 4;
            descriptor.Set(value);
        }

        public bool Visited(object obj)
        {
            return false;
        }
    }
}
