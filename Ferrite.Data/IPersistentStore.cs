/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
namespace Ferrite.Data;
public interface IPersistentStore
{
    public void SaveAuthKey(byte[] authKeyId, byte[] authKey);
    public byte[] GetAuthKey(byte[] authKeyId);
}

