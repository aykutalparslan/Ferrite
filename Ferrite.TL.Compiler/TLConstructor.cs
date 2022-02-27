/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

namespace Ferrite.TL.Compiler;

public class TLConstructor
{
    public string Id { get; set; }
    public string Predicate { get; set; }
    public List<TLParam> Params { get; set; }
    public string Type { get; set; }
}