using System;
using System.IO;

namespace CnCNetLauncher
{
    public class PositionedStream : Stream
    {
        private long _position;
        private Stream _baseStream;

        public PositionedStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        #region implemented abstract members of Stream

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int numRead = _baseStream.Read(buffer, offset, count);
            _position += numRead;
            return numRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return _baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}

