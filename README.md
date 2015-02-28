# GraphClimber
Climbs on graph of objects (.net)

Master | Provider
------ | --------
[![Build Status][MonoImgMaster]][MonoLinkMaster] | Mono CI Provided by [travis-ci][]
[![Build Status][TeamCityImgMaster]][TeamCityLinkMaster] | TeamCity CI Provided by [CodeBetter][]    
[![Build status](https://img.shields.io/appveyor/ci/shanielh/graphclimber.svg)](https://ci.appveyor.com/project/shanielh/graphclimber/branch/master) | Windows CI Provided by [AppVeyor][]

[MonoImgMaster]:https://img.shields.io/travis/Code-Sharp/GraphClimber/master.svg
[MonoLinkMaster]:https://travis-ci.org/Code-Sharp/GraphClimber

[TeamCityImgMaster]:https://img.shields.io/teamcity/codebetter/GraphClimber_Dev_Build_Github.svg
[TeamCityLinkMaster]:http://teamcity.codebetter.com/project.html?projectId=GraphClimber_Dev&tab=projectOverview&guest=1


[travis-ci]:https://travis-ci.org/
[JetBrains]:http://www.jetbrains.com/
[CodeBetter]:http://codebetter.com/
[AppVeyor]:http://www.appveyor.com/

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

There are many `Descriptors` to choose from, and you can _really_ go crazy with generic arguments. You can even implement `IGenericParameterFilter` in an Attribute and decorate your generic parameter with it, Like this method here which accepts only primitive values :

    [ProcessorMethod(Precedence = 99)]
    public void ProcessPrimitives<[Primitive]T>(IReadOnlyValueDescriptor<T> descriptor)

Then you need to choose between the simple state member providers (or to create one of your own). The objective of the `IStateMemberProvider` is to identify all the "State Members" that lies inside a given type. The graph climber climbs only on state members, Those can be fields, properties and even pair of get/set methods. 

__Note__ : A state member can be "read"/"write" only, but that's good as long as you want only read/write only access, When you'll try to read/write to those state members, no exception will be thrown from the default implementations of the `StateMemberProviders`. You may throw it from the `StateMember` (if you created it).

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

Actually, It's not going to do steps 2 and 3 all the times, only in the first time it encounters a new type of object, Because it generates an implementation of a method that climbs on that type runtime and than it caches it. Basicly that means "One time reflection", It's _fast_!

## FAQ

__Q__ : How can I help?
__A__ : You can help us by starting issues, writing code, documenting code, writing tests, and so. [Pull Requests are awesome](https://help.github.com/articles/creating-a-pull-request/).


__Q__ : Is this going to be better documented?
__A__ : Yeah, I Promise.


__Q__ : How can I climb recursively?
__A__ : Call climb() on the given descriptors, they will continue to climb on the current object that found on the state member, And remember, don't climb on nulls!


__Q__ : Can I climb on arrays?
__A__ : Sure you can, In that case the name of the statemember is going to be formatted with the array indices


__Q__ : Can I visit Enums?
__A__ : Hell yeah, Get `IReadOnlyEnumExactValueDescriptor` as an argument in your processor method

## Development stages

1. Writing full API
2. Implementing SlowGraphClimber with Examples (_Current State_)
3. Implementing GraphClimber with full tests

