using System;
using System.IO;

namespace GraphClimber.Examples
{
    internal class PositionRestore : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _position;
        private bool _canceled;

        public PositionRestore(Stream stream)
        {
            _stream = stream;
            _position = stream.Position;
        }

        public void Dispose()
        {
            if (_canceled)
            {
                return;
            }

            _stream.Position = _position;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }
}