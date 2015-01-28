using System;
using System.Collections.Generic;

namespace GraphClimber
{
    /// <summary>
    /// Provides the Graph Climber with stateful members of wanted
    /// types.
    /// 
    /// State members for example can be fields, properties and even getter/setter methods.
    /// </summary>
    public interface IStateMemberProvider
    {

        /// <summary>
        /// Gets the list of the <see cref="IStateMember"/>s for the given <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<IStateMember> Provide(Type type);

    }
}