using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class PropertyStateMember : IReflectionStateMember
    {
        private readonly PropertyInfo _property;
        private static readonly int[] EmptyIndex = new int[0];

        public PropertyStateMember(PropertyInfo property)
        {
            _property = property;
        }


        public string Name
        {
            get { return _property.Name; }
        }

        public Type OwnerType
        {
            get { return _property.ReflectedType; }
        }

        public virtual Type MemberType
        {
            get { return _property.PropertyType; }
        }

        public bool CanRead
        {
            get { return _property.CanRead; }
        }

        public bool CanWrite
        {
            get { return _property.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return
                Expression.Property
                    (obj.Convert(_property.DeclaringType),
                        _property);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return
                Expression.Assign(Expression.Property
                    (obj.Convert(_property.DeclaringType),
                        _property),
                    value.Convert(_property.PropertyType));
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            // See http://stackoverflow.com/questions/18937935/how-to-mutate-a-boxed-struct-using-il
            var dynamicMethod =
                new DynamicMethod("BoxSetter_" + this.Name, typeof (void),
                    new[] {typeof (object), typeof (T)},
                    typeof (AccessorFactory).Module, true);

            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);                       // object
            generator.Emit(OpCodes.Unbox, this.OwnerType);         // Struct&
            generator.Emit(OpCodes.Ldarg_1);                       // Struct& T

            if (typeof(T).IsValueType && !_property.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, typeof(T));
            }

            generator.Emit(OpCodes.Call, _property.SetMethod);     // --empty--
            generator.Emit(OpCodes.Ret);                           // --empty--

            Action<object, T> result = (Action<object, T>)dynamicMethod.CreateDelegate(typeof(Action<object, T>));

            return result;
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get
            {
                return EmptyIndex;
            }
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get
            {
                return _property;
            }
        }

        public object GetValue(object owner)
        {
            return _property.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _property.SetValue(owner, value);
        }

        protected bool Equals(PropertyStateMember other)
        {
            return Equals(_property, other._property);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyStateMember) obj);
        }

        public override int GetHashCode()
        {
            return (_property != null ? _property.GetHashCode() : 0);
        }
    }
}