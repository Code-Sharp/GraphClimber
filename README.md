# GraphClimber
Climbs on graph of objects (.net)

Master | Provider
------ | --------
[![Build Status][MonoImgMaster]][MonoLinkMaster] | Mono CI Provided by [travis-ci][] 
[![Build status](https://ci.appveyor.com/api/projects/status/rka7yi9j5j7dm9hr/branch/master?svg=true)](https://ci.appveyor.com/project/shanielh/graphclimber/branch/master) | Windows CI Provided by [AppVeyor][]

[MonoImgMaster]:https://travis-ci.org/Code-Sharp/GraphClimber.png?branch=master
[MonoLinkMaster]:https://travis-ci.org/Code-Sharp/GraphClimber

[travis-ci]:https://travis-ci.org/
[AppVeyor]:http://www.appveyor.com/
[JetBrains]:http://www.jetbrains.com/
[CodeBetter]:http://codebetter.com/

## Whatttt?

For example, If you want to check the size of a object and his descendents in your .net heap - You can do it easily with graph climber.

Other examples :
- Writing serializers / deserializers without having to use Expressions or runtime code generation
- Doing something with all the objects that implements some interface within a graph of an object

## How does it work?

You write a `Processor`, a `Processor` is an object that has a lot of `ProcessMethod`s, they have a signature like this one :

    [ProcessorMethod(Precedence = 102)]
    public void ProcessReferenceType<T>(IWriteOnlyExactValueDescriptor<T> descriptor)
        where T : class

And then you need to choose between the simple state member providers, This class identifies all the "State Members" that lies inside a given type. The graph climber climbs only on state members, Those can be fields, properties and even pair of get/set methods. 

__Note__ : A state member can be "read"/"write" only, but that's good as long as you want only read/write only access, When you'll try to read/write to those state members, no exception will be thrown from the GraphClimber. You may throw it from the `StateMember` (if you created it).

    var stateMemberProvider = new CachingStateMemberProvider(new PropertiesStateMemberProvider());
    var gc = new GraphClimber<MyProcessor>(stateMemberProvider);

After you've done those, you ready to climb on objects : 

    var myProcessorInstance = new MyProcessor();
    var myObject = GetComplexObject();
    
    gc.Climb(myObject, myProcessorInstance);
    
What's the graph climber is going to do now? 
 
1. Look for the climbed object type
2. Tell the state member provider to give all the state members that the given type has
3. Look for the most appropriate method (If exists) inside "MyProcessor" to call for every state member and call it.

## FAQ

__Q__ : How can I help?

__A__ : You can help us by starting issues, writing code, documenting code, writing tests, and so. [Pull Requests are awesome](https://help.github.com/articles/creating-a-pull-request/).


__Q__ : Is this going to be better documented?

__A__ : Yeah, I Promise.


__Q__ : How can I climb recursively?

__A__ : Call climb() on the given descriptors, they will continue to climb on the current object that found on the state member, And remember, don't climb on nulls!


__Q__ : Can I climb on arrays?

__A__ : Sure you can, In that case the name of the statemember is going to be formatted with the array indices


__Q__ : Can I climb on enums?

__A__ : Hell yeah, Get `IReadOnlyEnumExactValueDescriptor` as an argument in your processor method

## Development stages

1. Writing full API
2. Implementing SlowGraphClimber with Examples (_Current State_)
3. Implementing GraphClimber with full tests
