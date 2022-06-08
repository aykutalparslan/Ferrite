// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Text;

namespace Ferrite.TLParser;

public class TypeTermSyntax
{
    public bool IsBare { get; set; }
    public bool IsTypeOf { get; set; }
    public string? NamespaceIdentifier { get; set; }
    public string Identifier { get; set; }
    public TypeTermSyntax? OptionalType { get; set; }

    public string GetFullyQualifiedIdentifier()
    {
        var sb = new StringBuilder();
        if (NamespaceIdentifier != null)
        {
            sb.Append(NamespaceIdentifier);
            sb.Append(".");
        }

        if (IsTypeOf)
        {
            return "BoxedObject";
        }
        if (Identifier == "Vector" && OptionalType.Identifier == "int")
        {
            return "VectorOfInt";
        }
        if (Identifier == "Vector" && OptionalType.Identifier == "long")
        {
            return "VectorOfLong";
        }
        if (Identifier == "Vector" && OptionalType.Identifier == "double")
        {
            return "VectorOfDouble";
        }
        
        if (Identifier == "vector")
        {
            sb.Append("VectorBare");
        }
        else if (Identifier is "bytes" or "string")
        {
            sb.Append("TLString");
        }
        else if (Identifier is "#")
        {
            sb.Append("Flags");
        }
        else if (Identifier is "Object")
        {
            sb.Append("BoxedObject");
        }
        else if (Identifier is "int128" or "int258")
        {
            sb.Append("ReadOnlySpan<byte>");
        }
        else
        {
            sb.Append(Identifier);
        }
        if (OptionalType == null) return sb.ToString();
        sb.Append("<");
        sb.Append(OptionalType.GetFullyQualifiedIdentifier());
        sb.Append(">");
        return sb.ToString();
    }
}