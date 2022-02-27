/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
namespace Ferrite.TL.Exceptions
{
    public class DeserializationException : Exception
    {
        public DeserializationException()
        {
        }

        public DeserializationException(string message)
            : base(message)
        {
        }

        public DeserializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

