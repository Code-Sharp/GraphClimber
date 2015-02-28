using System;

namespace GraphClimber
{
    /// <summary>
    /// This attribute annotates a method inside a processor 
    /// as a processor method, Which means that the method can be called
    /// by the graph climber if it is found as the most suitable method to call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProcessorMethodAttribute : Attribute
    {
        public ProcessorMethodAttribute()
        {
            Precedence = 100;
        }

        /// <summary>
        /// Gets or sets the precedence of the method 
        /// annotated by this attribute. 
        /// 
        /// Defaults to 100.
        /// </summary>
        public int Precedence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this method will be called only via 
        /// <see cref="IValueDescriptor.Route(GraphClimber.IStateMember,System.Type,object,bool)"/> calls.
        /// </summary>
        public bool OnlyOnRoute { get; set; }
    }
}
