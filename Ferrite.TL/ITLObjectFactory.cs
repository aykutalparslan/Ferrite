using System;
using DotNext.IO;

namespace Ferrite.TL
{
    public interface ITLObjectFactory
    {
        public ITLObject Read(int constructor, ref SequenceReader buff);
        public T Read<T>(ref SequenceReader buff) where T : ITLObject;
        public T Resolve<T>() where T : ITLObject;
    }
}

