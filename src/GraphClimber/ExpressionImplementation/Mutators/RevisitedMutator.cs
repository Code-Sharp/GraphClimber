using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class RevisitedMutator : IMethodMutator
    {
        private static readonly MethodInfo _visitedMethod =
            typeof (IRevisitedFilter).GetMethod("Visited");

        private static readonly MethodInfo _processRevisitedMethod =
            typeof (IRevisitedProcessor).GetMethod("ProcessRevisited");

        private readonly bool _isRevisitedFilter;

        private readonly bool _isRevisitedProcessor;

        public RevisitedMutator(Type processorType)
        {
            _isRevisitedFilter = typeof(IRevisitedFilter).IsAssignableFrom(processorType);
            _isRevisitedProcessor = typeof(IRevisitedProcessor).IsAssignableFrom(processorType);
        }

        public Expression Mutate(Expression oldExpression, Expression processor, Expression owner, IStateMember member, Expression descriptor)
        {
            if (!_isRevisitedFilter && !_isRevisitedProcessor)
            {
                return oldExpression;
            }

            Type memberType = member.MemberType;

            if (!member.CanRead ||
                (memberType.IsValueType && !memberType.IsNullable()))
            {
                return oldExpression;
            }

            Expression processRevisited = GetProcessRevisitedExpression(processor, descriptor, memberType);

            // Generated code should look like this : 

            // if (processor.Visited(value)
            // {
            //      processor.ProcessRevisited(descriptor); || Empty Expression
            // }
            // else
            // {
            //      oldValue();
            // }

            var value = member.GetGetExpression(owner);

            Expression body =
                Expression.Condition(Expression.Call(processor, _visitedMethod, value),
                    processRevisited,
                    oldExpression);

            return body;
        }

        /// <summary>
        /// Returns an expression to be called
        /// when the value that should be processed
        /// is already visited.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="descriptor"></param>
        /// <param name="memberType"></param>
        /// <returns>
        /// Depending on the processor type, If <see cref="IRevisitedProcessor"/> is implemented, 
        /// Returns a call to <see cref="IRevisitedProcessor.ProcessRevisited{TField}"/>. 
        /// Else, returns an empty expression.</returns>
        private Expression GetProcessRevisitedExpression(Expression processor, Expression descriptor, Type memberType)
        {
            if (!_isRevisitedProcessor)
            {
                // non-call if it's not implemented.
                return ExpressionExtensions.Empty;
            }

            // call to the method if the interface is implemented
            MethodInfo processRevisitedMethod =
                _processRevisitedMethod.MakeGenericMethod(memberType);

            return Expression.Call(processor, processRevisitedMethod, descriptor);
        }
    }
}