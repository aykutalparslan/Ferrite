/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Text.Json;

namespace Ferrite.TL.Compiler;

public class TLSchema
{
    public List<TLConstructor> Constructors { get; set; }
    public List<TLMethod> Methods { get; set; }

    static public TLSchema Load(string file)
    {
        StreamReader sr = new StreamReader(file);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
        TLSchema schema = JsonSerializer.Deserialize<TLSchema>(sr.ReadToEnd(), options);
        
        return schema;
    }
}


