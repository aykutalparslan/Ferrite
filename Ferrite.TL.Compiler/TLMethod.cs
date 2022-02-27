/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
namespace Ferrite.TL.Compiler;

public class TLMethod
{
    public string Id { get; set; }
    public string Method { get; set; }
    public List<TLParam> Params { get; set; }
    public string Type { get; set; }
}


