using System;
using System.IO.Pipelines;

namespace Ferrite.Transport
{
    public interface ISocketConnection
    {
        public IDuplexPipe Transport { get; }
        public IDuplexPipe Application { get; }
    }
}

