using System;

namespace GraphClimber.ExpressionCompiler
{
    public static class StringExtensions
    {

        public static Position GetPosition(this string str, int index)
        {
            var column = index;
            var newLineIndex = str.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            var newLines = 0;

            while (newLineIndex != -1 && newLineIndex < index)
            {
                newLines++;
                column = index - newLineIndex - Environment.NewLine.Length;
                newLineIndex = str.IndexOf(Environment.NewLine, newLineIndex + 1, StringComparison.Ordinal);
            }


            return new Position { Line = newLines, Column = column };
        }

        public struct Position
        {
            public Position(int line, int column)
                : this()
            {
                Line = line;
                Column = column;
            }

            public int Line { get; set; }

            public int Column { get; set; }

        }
    }
}