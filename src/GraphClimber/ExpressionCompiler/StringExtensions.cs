using System;

namespace GraphClimber.ExpressionCompiler
{
    public static class StringExtensions
    {

        /// <summary>
        /// Gets the cursor position (Column, Line) of the given index in the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <returns></returns>
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

            return new Position(newLines, column);
        }

        public static int IndexOfNot(this string str, char chr)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != chr)
                {
                    return i;
                }
            }

            return -1;
        }

        public static string FirstLowerCase(this string str)
        {
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
        }

    }

    /// <summary>
    /// Represents a position inside a string
    /// </summary>
    public struct Position
    {
        private readonly int _line;
        private readonly int _column;

        public Position(int line, int column)
        {
            _line = line;
            _column = column;
        }

        /// <summary>
        /// Gets the line of the position
        /// </summary>
        public int Line
        {
            get
            {
                return _line;
            }
        }

        /// <summary>
        /// Gets the column of the position
        /// </summary>
        public int Column
        {
            get
            {
                return _column;
            }
        }

    }
}