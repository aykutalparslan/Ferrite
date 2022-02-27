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
    public class TLExecutionException : Exception
    {
        public TLExecutionException()
        {
        }

        public TLExecutionException(string message)
            : base(message)
        {
        }

        public TLExecutionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

