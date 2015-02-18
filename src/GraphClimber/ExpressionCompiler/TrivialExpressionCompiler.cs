using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public class TrivialExpressionCompiler : IExpressionCompiler
    {
        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        {
            return expression.Compile();
        }
    }

    public class FallbackExpressionCompiler : IExpressionCompiler
    {
        private readonly IExpressionCompiler[] _compilers;

        public FallbackExpressionCompiler(params IExpressionCompiler[] compilers)
        {
            _compilers = compilers;
        }

        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        {
            IList<Exception> exceptions = null;

            foreach (var compiler in _compilers)
            {
                try
                {
                    return compiler.Compile(expression);
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }
            
            throw new AggregateException("Could not compile expression with any of the compilers given. Exceptions are collected.", exceptions);
        }
    }
}