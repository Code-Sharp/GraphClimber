namespace GraphClimber
{
    internal class Box<T>
    {
        public Box(T value)
        {
            Value = value;
        }

        public T Value;
    }
}