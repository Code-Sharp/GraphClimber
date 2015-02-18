namespace GraphClimber
{
    internal delegate void ClimbDelegate<T>(object processor, T value);

    internal delegate void StructClimbDelegate<TStruct>(object processor, Box<TStruct> value);
}