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
using Microsoft.CodeAnalysis;

namespace Ferrite.TLParser;

public class TLSourceGenerator
{
    public static readonly GeneratedSource DefaultSource = new GeneratedSource("default", "");
    readonly SortedSet<string> _namespaces = new();
    readonly SortedSet<string> _bareNamespaces = new();
    readonly SortedList<string, CombinatorDeclarationSyntax> _combinators = new();
    private readonly Dictionary<string, int> _typeCount = new();

    public IEnumerable<GeneratedSource> Generate(string nameSpace, string source)
    {
        Dictionary<string, List<CombinatorDeclarationSyntax>> types = new();
        _namespaces.Add(nameSpace);
        List<Token> tokens = new List<Token>();
        var lexer = new Lexer(source);
        foreach (GeneratedSource generatedSource in GenerateSources(nameSpace, lexer, types))
        {
            yield return generatedSource;
        }

        yield return DefaultSource;
    }
    private IEnumerable<GeneratedSource> GenerateSources(string nameSpace, Lexer lexer, Dictionary<string, List<CombinatorDeclarationSyntax>> types)
    {
        List<CombinatorDeclarationSyntax> combinators = new();
        var parser = new Parser(lexer);
        var c = parser.ParseCombinator();
        while (c != null)
        {
            var ns = nameSpace;
            if (c.CombinatorType == CombinatorType.Constructor &&
                c.Type.NamespaceIdentifier != null)
            {
                ns += (ns.Length > 0 ? "." : "") + c.Type.NamespaceIdentifier;
                _namespaces.Add(ns);
                if (c.Type.NamespaceIdentifier != null)
                {
                    _bareNamespaces.Add(c.Type.NamespaceIdentifier);
                }
            }
            else if (c.CombinatorType == CombinatorType.Function && 
                     c.Namespace != null)
            {
                ns += (ns.Length > 0 ? "." : "") + c.Namespace;
                _namespaces.Add(ns);
                if (c.Namespace != null)
                {
                    _bareNamespaces.Add(c.Namespace);
                }
            }
            if (c.Name != null && !_combinators.ContainsKey(c.Name))
            {
                if (_typeCount.ContainsKey(c.Identifier))
                {
                    _typeCount[c.Identifier] = _typeCount[c.Identifier] + 1;
                }
                else
                {
                    _typeCount.Add(c.Identifier, 1);
                }
                combinators.Add(c);
                _combinators.Add(c.Name, c);
            }
            c = parser.ParseCombinator();
        }

        foreach (var combinator in combinators)
        {
            if (combinator?.Identifier == "file")
            {
                combinator.Identifier += "_";
            }
            if (combinator != null && _bareNamespaces.Contains(combinator.Identifier))
            {
                combinator.Identifier += "_";
            }
            if (combinator != null && _typeCount[combinator.Identifier.TrimEnd('_')] > 1)
            {
                combinator.Identifier = combinator.Namespace != null ? combinator.Namespace + "_" + combinator.Identifier : combinator.Identifier;
            }
            var ns = nameSpace;
            if (combinator?.CombinatorType == CombinatorType.Constructor &&
                combinator.Type.NamespaceIdentifier != null)
            {
                ns += (ns.Length > 0 ? "." : "") + combinator.Type.NamespaceIdentifier;
            }
            else if (combinator?.CombinatorType == CombinatorType.Function && 
                     combinator.Namespace != null)
            {
                ns += (ns.Length > 0 ? "." : "") + combinator.Namespace;
            }
            
            var id = ns + "." + combinator?.Type.Identifier;
            if (combinator?.CombinatorType == CombinatorType.Constructor &&
                !types.ContainsKey(id))
            {
                types.Add(id, new List<CombinatorDeclarationSyntax>() { combinator });
                yield return GenerateSourceFile(combinator, ns);
            }
            else if (combinator?.CombinatorType == CombinatorType.Constructor)
            {
                types[id].Add(combinator);
                yield return GenerateSourceFile(combinator, ns);
            }
            else if (combinator?.CombinatorType == CombinatorType.Function)
            {
                yield return GenerateFunctionSource(combinator, ns);
            }
        }
    }
    public GeneratedSource GenerateObjectReader()
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Runtime.InteropServices;");
        foreach (var ns in _namespaces)
        {
            sb.Append(@"
using Ferrite.TL.slim"+(ns.Length>0?"."+ns:"")+@";
");
        }
        
sb.Append(@"
namespace Ferrite.TL.slim;

public static class ObjectReader
{
    private static readonly Dictionary<int, ObjectReaderDelegate> _objectReaders = new();
    private static readonly Dictionary<int, ObjectSizeReaderDelegate> _sizeReaders = new();
    static ObjectReader()
    {");
         foreach (var combinator in _combinators.Values)
        {
            sb.Append(@"
        _objectReaders.Add(unchecked((int)0x"+combinator.Name+@"), "+combinator.Identifier+@".Read);
        _sizeReaders.Add(unchecked((int)0x"+combinator.Name+@"), "+combinator.Identifier+@".ReadSize);");
        }
        sb.Append(@"
    }
");
        sb.Append(@"
    public static Span<byte> Read(Span<byte> buff)
    {
        if (buff.Length < 4)
        {
            return Span<byte>.Empty;
        }
        int constructor = MemoryMarshal.Read<int>(buff);
        if (_objectReaders.ContainsKey(constructor))
        {
            var reader = _objectReaders[constructor];
            return reader(buff, 0);
        }
        return Span<byte>.Empty;
    }
    public static Span<byte> Read(Span<byte> buff, int constructor)
    {
        if (buff.Length < 4)
        {
            return Span<byte>.Empty;
        }
        if (_objectReaders.ContainsKey(constructor))
        {
            var reader = _objectReaders[constructor];
            return reader(buff, 0);
        }
        return Span<byte>.Empty;
    }
    public static int ReadSize(Span<byte> buff)
    {
        if (buff.Length < 4)
        {
            return 0;
        }
        int constructor = MemoryMarshal.Read<int>(buff);
        if (_sizeReaders.ContainsKey(constructor))
        {
            var reader = _sizeReaders[constructor];
            return reader(buff, 0);
        }
        return 0;
    }
    public static int ReadSize(Span<byte> buff, int constructor)
    {
        if (buff.Length < 4)
        {
            return 0;
        }
        if (_sizeReaders.ContainsKey(constructor))
        {
            var reader = _sizeReaders[constructor];
            return reader.Invoke(buff, 0);
        }
        return 0;
    }
    public static ObjectReaderDelegate? GetObjectReader(int constructor)
    {
        if (_objectReaders.ContainsKey(constructor))
        {
            return _objectReaders[constructor];
        }

        return null;
    }
    public static ObjectSizeReaderDelegate? GetObjectSizeReader(int constructor)
    {
        if (_sizeReaders.ContainsKey(constructor))
        {
            return _sizeReaders[constructor];
        }

        return null;
    }
}
");
        return new GeneratedSource("ObjectReader.g.cs",
            sb.ToString());
    }
    public GeneratedSource GenerateConstructors()
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

namespace Ferrite.TL.slim;

public static class Constructors
{
    ");
        foreach (var combinator in _combinators.Values)
        {
            sb.Append(@"
    public const int " + combinator.Identifier + " = unchecked((int)0x" + combinator.Name + @");");
        }
        sb.Append(@"
}
");
        return new GeneratedSource("Constructors.g.cs",
            sb.ToString());
    }
    private GeneratedSource GenerateFunctionSource(CombinatorDeclarationSyntax? combinator, string nameSpace)
    {
        var typeName = combinator.Identifier;
        StringBuilder sourceBuilder = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.InteropServices;
using Ferrite.Utils;
using DotNext.Buffers;

namespace Ferrite.TL.slim"+(nameSpace.Length > 0? "."+nameSpace:"")+@";

public readonly ref struct " + typeName + @"
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;");
        GenerateCreate(sourceBuilder, combinator);
        sourceBuilder.Append(
            @"
    public " + typeName + @"(Span<byte> buff)
    {
        _buff = buff;
    }
    "+
    (combinator.Name != null ?                                                    
        @"
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }": "")+
    
    @"
    public int Length => _buff.Length;
    public ReadOnlySpan<byte> ToReadOnlySpan() => _buff;
    public TLBytes? TLBytes => _memory != null ? new TLBytes(_memory, 0, _buff.Length) : null;
    public static Span<byte> Read(Span<byte> data, int offset)
    {
        var bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
    @", data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }
");
        GenerateGetRequiredBufferSize(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", data[offset..]);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        GenerateBuilder(sourceBuilder, combinator);
        var str = @"
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
";
        sourceBuilder.Append(str);
        return new GeneratedSource((nameSpace.Length>0?nameSpace+"_":"") + 
                          combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }
    private GeneratedSource GenerateSourceFile(CombinatorDeclarationSyntax? combinator, string nameSpace)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        StringBuilder sourceBuilder = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.InteropServices;
using Ferrite.Utils;
using DotNext.Buffers;

namespace Ferrite.TL.slim" + (nameSpace.Length > 0 ? "." + nameSpace : "") + @";

public readonly ref struct " + typeName + @"
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;");
        GenerateCreate(sourceBuilder, combinator);
    sourceBuilder.Append(
    @"
    public " + typeName + @"(Span<byte> buff)
    {
        _buff = buff;
    }
    "+
                         (combinator.Name != null ?                                                    
                             @"
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }": "")+
    
                         @"
    public int Length => _buff.Length;
    public ReadOnlySpan<byte> ToReadOnlySpan() => _buff;
    public TLBytes? TLBytes => _memory != null ? new TLBytes(_memory, 0, _buff.Length) : null;
    public static Span<byte> Read(Span<byte> data, int offset)
    {
        var bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
                         @", data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }
");
        GenerateGetRequiredBufferSize(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", data[offset..]);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        GenerateBuilder(sourceBuilder, combinator);
        var str = @"
    public static TLObjectBuilder Builder()
    {
        return new TLObjectBuilder();
    }
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
";
        sourceBuilder.Append(str);
        return new GeneratedSource((nameSpace.Length>0?nameSpace+"_":"") + 
                          combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }
    private void GenerateGetRequiredBufferSize(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    public static int GetRequiredBufferSize(");
        bool first = true;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "true" &&
                arg.TypeTerm.IsBare)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                if (arg.TypeTerm.Identifier is "int" or "long" or "double" or "int128" or "int258" or "Bool")
                {
                    sb.Append("bool has_" + arg.Identifier);
                }
                else
                {
                    sb.Append("bool has_" + arg.Identifier + ", int len_" + arg.Identifier);
                }
            }
            else if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier == "Bool")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append("bool has_" + arg.Identifier);
            }
            else if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "true")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append("int len_" + arg.Identifier);
            }
            else if (arg.TypeTerm.Identifier != "#" && arg.TypeTerm.Identifier != "int" &&
                     arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                     arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256" &&
                     arg.TypeTerm.Identifier != "true" && arg.TypeTerm.Identifier != "Bool")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append("int len_" + arg.Identifier);
            }
        }

        sb.Append(@")
    {
        return ");
        bool appended = false;
        if (combinator.Name != null)
        {
            sb.Append("4");
            appended = true;
        }

        for (int i = 0; i < combinator.Arguments.Count; i++)
        {
            var arg = combinator.Arguments[i];
            if (arg.TypeTerm.Identifier == "#")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append("4");
            }
            else if (arg.TypeTerm.Identifier == "true")
            {
                
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?4:0)" : "4");
            }
            else if (arg.TypeTerm.Identifier == "Bool")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?4:0)" : "4");
            }
            else if (arg.TypeTerm.Identifier is "long" or "double")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?8:0)" : "8");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?16:0)" : "16");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?32:0)" : "32");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                if(arg.ConditionalDefinition != null)
                {
                    sb.Append("(has_"+arg.Identifier+"?BufferUtils.CalculateTLBytesLength(len_" + arg.Identifier+"):0)");
                }
                else
                {
                    sb.Append("BufferUtils.CalculateTLBytesLength(len_" + arg.Identifier+")");
                }
            }
            else
            {
                if (appended)
                {
                    sb.Append(" + ");
                }
                else
                {
                    appended = true;
                }
                sb.Append("len_" + arg.Identifier);
            }
        }

        if (!appended)
        {
            sb.Append("0");
        }
        sb.Append(@";
    }");
    }
    private void GenerateCreate(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        sb.Append(@"
    public " + typeName +
                  @"(");
        int count = combinator.Arguments.Count;
        foreach (var arg in combinator.Arguments)
        {
            bool comma = --count != 0;
            if (arg.Identifier == "long")
            {
                arg.Identifier = "longitude";
            }
            if (arg.Identifier == combinator.Identifier || arg.Identifier == "out" || 
                arg.Identifier == "static" || arg.Identifier == "params" ||
                arg.Identifier == "default" || arg.Identifier == "public" ||
                arg.Identifier == "readonly")
            {
                arg.Identifier += "_";
            }
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append("Flags " + arg.Identifier + (comma ? ", ": ""));
            }
            else if (arg.TypeTerm.Identifier is "true" or "Bool")
            {
                sb.Append("bool " + arg.Identifier + (comma ? ", ": ""));
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string" or "int128" or "int256")
            {
                sb.Append("ReadOnlySpan<byte> " + arg.Identifier + (comma ? ", ": ""));
            }
            else if (arg.TypeTerm.Identifier is "int" or "double" or "long")
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(typeIdent + " " + arg.Identifier +(comma ? ", ": ""));
            }
            else if (arg.TypeTerm.Identifier is "Vector" or "VectorBare" or "vector")
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(typeIdent + " " + arg.Identifier +(comma ? ", ": ""));
            }
            else if (arg.TypeTerm.Identifier != "true")
            {
                sb.Append("ReadOnlySpan<byte> " + arg.Identifier+ (comma ? ", ": ""));
            }
            /*else if (arg.TypeTerm.Identifier != "true")
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(typeIdent + (arg.ConditionalDefinition!=null?"?":"") + " " + arg.Identifier +(comma ? ", ": ""));
            }*/
        }

        sb.Append(@")
    {
        var bufferLength = GetRequiredBufferSize(");
        bool first = true;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "true" &&
                arg.TypeTerm.IsBare)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                if (arg.TypeTerm.Identifier != "int" && arg.TypeTerm.Identifier != "long" && 
                    arg.TypeTerm.Identifier != "double" && arg.TypeTerm.Identifier != "int128" && 
                    arg.TypeTerm.Identifier != "int256")
                {
                    sb.Append(arg.ConditionalDefinition.Identifier + "["+arg.ConditionalDefinition.ConditionalArgumentBit+"], "+ arg.Identifier +".Length");
                }
                else
                {
                    sb.Append(arg.ConditionalDefinition.Identifier + "["+arg.ConditionalDefinition.ConditionalArgumentBit+"]");
                }
            }
            else if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier == "Bool")
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(arg.ConditionalDefinition.Identifier + "["+arg.ConditionalDefinition.ConditionalArgumentBit+"]");
            }
            else if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "true")
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append("("+arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]?"+arg.Identifier + ".Length:0)");
            }
            else if (arg.TypeTerm.Identifier != "#" && arg.TypeTerm.Identifier != "int" &&
                     arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                     arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256" &&
                     arg.TypeTerm.Identifier != "true" && arg.TypeTerm.Identifier != "Bool")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append(arg.Identifier + ".Length");
            }
        }

        sb.Append(@");
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(bufferLength);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..bufferLength];");
        if (combinator.Name != null)
        {
            sb.Append(@"
        SetConstructor(unchecked((int)0x"+combinator.Name+"));");
        }

        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
        Set_" + arg.Identifier + @"(" + arg.Identifier + ");");
            }
            else if (arg.TypeTerm.Identifier == "true")
            {
                
            }
            else if (arg.TypeTerm.IsBare && arg.TypeTerm.OptionalType == null)
            {
                if (arg.ConditionalDefinition != null && 
                    arg.TypeTerm.Identifier != "string" && arg.TypeTerm.Identifier != "bytes" &&
                    arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int258")
                {
                    sb.Append(@"
        if("+arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])"+ @"
        {
            Set_"+arg.Identifier+"("+arg.Identifier+@");
        }");
                }
                else if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if("+arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])"+ @"
        {
            Set_"+arg.Identifier+"("+arg.Identifier+@");
        }");
                }
                else
                {
                    sb.Append(@"
        Set_"+arg.Identifier+"("+arg.Identifier+@");");         
                }
                    
            }
            else if (arg.TypeTerm.Identifier is "Vector" or "VectorBare" or "vector")
            {
                if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if("+arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])
        {
            Set_"+arg.Identifier+"("+arg.Identifier+@".ToReadOnlySpan());
        }");
                }
                else
                {
                    sb.Append(@"
        Set_"+arg.Identifier+"("+arg.Identifier+@".ToReadOnlySpan());");
                }
            }
            else //if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if("+arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])
        {
            Set_"+arg.Identifier+"("+arg.Identifier+@");
        }");
                }
                else
                {
                    sb.Append(@"
        Set_"+arg.Identifier+"("+arg.Identifier+@");");
                }
            }
            /*else
            {
                if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if(" + arg.Identifier + @" != null)
        {
            Set_"+arg.Identifier+"((("+ arg.TypeTerm.GetFullyQualifiedIdentifier() +")"+arg.Identifier+@").ToReadOnlySpan());
        }");
                }
                else
                {
                    sb.Append(@"
        Set_"+arg.Identifier+"("+arg.Identifier+@".ToReadOnlySpan());");
                }
                
            }*/
        }
        sb.Append(@"
    }");
    }
    private void GenerateProperties(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        int index = 1;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
    public readonly Flags " + arg.Identifier + @" => new Flags(MemoryMarshal.Read<int>(_buff[GetOffset(" + index +
                          ", _buff)..]));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(Flags value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "true" && 
                     arg.ConditionalDefinition != null)
            {
                sb.Append(@"
    public readonly bool " + arg.Identifier + @" => " + arg.ConditionalDefinition.Identifier + "[" +
                          arg.ConditionalDefinition.ConditionalArgumentBit + "];");
            }
            else if (arg.TypeTerm.Identifier == "Bool")
            {
                sb.Append(@"
    public readonly bool " + arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                              "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? false : " : "") +
                          "MemoryMarshal.Read<int>(_buff[GetOffset(" + index + ", _buff)..]) == unchecked((int)0x997275b5);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(bool value)
    {
        int t = unchecked((int)0x997275b5);
        int f = unchecked((int)0xbc799737);
        if(value)
        {
            MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref t);
        }
        else 
        {
            MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref f);
        }
    }");
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
    public readonly int " + arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                              "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? 0 : " : "") +
                          "MemoryMarshal.Read<int>(_buff[GetOffset(" + index +
                          ", _buff)..]);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "long")
            {
                sb.Append(@"
    public readonly long " + arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                              "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? 0 : " : "") +
                          "MemoryMarshal.Read<long>(_buff[GetOffset(" + index +
                          ", _buff)..]);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(long value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "double")
            {
                sb.Append(@"
    public readonly double " + arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                              "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? 0 : " : "") +
                          "MemoryMarshal.Read<double>(_buff[GetOffset(" + index +
                          ", _buff)..]);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(double value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                    "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? new ReadOnlySpan<byte>() : " : "") +
                          " _buff.Slice(GetOffset("+index+@", _buff), 16);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length != 16)
        {
            return;
        }
        value.CopyTo(_buff.Slice(GetOffset("+index+@", _buff), 16));
    }");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                    "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? new ReadOnlySpan<byte>(); : " : "") +
                          " _buff.Slice(GetOffset("+index+@", _buff), 32);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length != 32)
        {
            return;
        }
        value.CopyTo(_buff.Slice(GetOffset("+index+@", _buff), 32));
    }");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier + " => "+(arg.ConditionalDefinition != null ? 
                    "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? new ReadOnlySpan<byte>() : " : "") +
                          " BufferUtils.GetTLBytes(_buff, GetOffset("+index+@", _buff));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length == 0)
        {
            return;
        }
        var offset = GetOffset("+index+@", _buff);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset);
        if(_buff.Length < offset + lenBytes + value.Length) return;
        value.CopyTo(_buff[(offset + lenBytes)..]);
    }");
            }
            else if (arg.TypeTerm.Identifier is "Vector" or "VectorBare" or "vector")
            {
                sb.Append(@"
    public "+ arg.TypeTerm.GetFullyQualifiedIdentifier() +" "+arg.Identifier + " => "
                          +(arg.ConditionalDefinition != null ? 
                              "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? new "
                              + arg.TypeTerm.GetFullyQualifiedIdentifier() +"() : " : "")
                              + "new " + arg.TypeTerm.GetFullyQualifiedIdentifier() +
                              "(_buff.Slice(GetOffset("+index+@", _buff)));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset("+index+@", _buff)..]);
    }");
            }
            else //if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append(@"
    public Span<byte> " + arg.Identifier + " => " +(arg.ConditionalDefinition != null ? 
                    "!flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"] ? new Span<byte>() : " : "") +
                    "ObjectReader.Read(_buff);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset("+index+@", _buff)..]);
    }");
            }
            /*else 
            {
                sb.Append(@"
    public "+ arg.TypeTerm.GetFullyQualifiedIdentifier() +" "+arg.Identifier+@" => new "+ 
                          arg.TypeTerm.GetFullyQualifiedIdentifier() 
                          +@"(_buff.Slice(GetOffset("+index+@", _buff)));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset("+index+@", _buff)..]);
    }");
            }*/

            if (arg.TypeTerm.Identifier != "true")
            {
                index++;
            }
        }
    }
    private void GenerateBuilder(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    public ref struct TLObjectBuilder
    {
        public TLObjectBuilder(){}
");
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
        private Flags _" + arg.Identifier + " = new Flags();");
            }
            else if (arg.TypeTerm.Identifier == "true" && 
                     arg.ConditionalDefinition != null)
            {
                sb.Append(@"
        public TLObjectBuilder with_"+arg.Identifier+@"(bool value)
        {
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+@"] = value;
            return this;
        }");
            }
            else if (arg.TypeTerm.Identifier == "Bool")
            {
                sb.Append(@"
        private bool _" + arg.Identifier + @";
        public TLObjectBuilder with_"+arg.Identifier+@"(bool value)
        {
            _" + arg.Identifier + @" = value;" 
                          +(arg.ConditionalDefinition != null ? 
                              @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                              "")+
                          @"
            return this;
        }");
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
        private int _" + arg.Identifier + @";
        public TLObjectBuilder with_"+arg.Identifier+@"(int value)
        {
            _" + arg.Identifier + @" = value;" 
            +(arg.ConditionalDefinition != null ? 
                @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                "")+
            @"
            return this;
        }");
            }
            else if (arg.TypeTerm.Identifier == "long")
            {
                sb.Append(@"
        private long _" + arg.Identifier + @";
        public TLObjectBuilder with_"+arg.Identifier+@"(long value)
        {
            _" + arg.Identifier + @" = value;" 
                          +(arg.ConditionalDefinition != null ? 
                              @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                              "")+
                          @"
            return this;
        }");
            }
            else if (arg.TypeTerm.Identifier == "double")
            {
                sb.Append(@"
        private double _" + arg.Identifier + @";
        public TLObjectBuilder with_"+arg.Identifier+@"(double value)
        {
            _" + arg.Identifier + @" = value;" 
                          +(arg.ConditionalDefinition != null ? 
                              @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                              "")+
                          @"
            return this;
        }");
            }
            else if (arg.TypeTerm.Identifier is "Vector" or "VectorBare" or "vector")
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(@"
        private " + typeIdent + " _" + arg.Identifier + @";
        public TLObjectBuilder with_" + arg.Identifier + "(" + typeIdent + @" value)
        {
            _" + arg.Identifier + @" = value;" 
                          +(arg.ConditionalDefinition != null ? 
                              @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                              "")+
                          @"
            return this;
        }");
            }
            else //if (arg.TypeTerm.Identifier is "bytes" or "string" or "int128" or "int256"||
                 //    arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append(@"
        private ReadOnlySpan<byte> _" + arg.Identifier + @";
        public TLObjectBuilder with_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
        {
            _" + arg.Identifier + @" = value;" 
                          +(arg.ConditionalDefinition != null ? 
                              @"
            _"+arg.ConditionalDefinition.Identifier+"["+arg.ConditionalDefinition.ConditionalArgumentBit+"] = true;":
                              "")+
                          @"
            return this;
        }");
            }
            /*else
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(@"
        private " + typeIdent + " _" + arg.Identifier + @";
        public TLObjectBuilder with_" + arg.Identifier + "(" + typeIdent + @" value)
        {
            _" + arg.Identifier + @" = value;
            return this;
        }");
            }*/
        }
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        sb.Append(@"
        public " + typeName + @" Build()
        {
            return new " + typeName);
        
        sb.Append(@"(");
        int count = combinator.Arguments.Count;
        foreach (var arg in combinator.Arguments)
        {
            bool comma = --count != 0;
            if (arg.Identifier == "long")
            {
                arg.Identifier = "longitude";
            }
            if (arg.Identifier == combinator.Identifier || arg.Identifier == "out" || 
                arg.Identifier == "static" || arg.Identifier == "params" ||
                arg.Identifier == "default" || arg.Identifier == "public" ||
                arg.Identifier == "readonly")
            {
                arg.Identifier += "_";
            }

            if (arg.TypeTerm.Identifier is "true")
            {
                sb.Append("_" + arg.ConditionalDefinition!.Identifier + "[" + arg.ConditionalDefinition!.ConditionalArgumentBit +
                          "]" + (comma ? ", " : ""));
            }
            else
            {
                sb.Append("_" + arg.Identifier + (comma ? ", ": ""));
            }
        }
        sb.Append(@");
        }
    }
");
    }
    private void GenerateGetOffset(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = "+(combinator.Name != null?"4":"0")+@";");
        bool hasFlags = false;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.ConditionalDefinition != null)
            {
                hasFlags = true;
            }
        }

        if (hasFlags)
        {
            sb.Append(@"
        Flags f = new Flags(MemoryMarshal.Read<int>(buffer[offset..]));");
        }
        int index = 2;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 4;");
            }
            else if (arg.TypeTerm.Identifier == "Bool")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 4;");
            }
            else if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 4;");
            }
            else if (arg.TypeTerm.Identifier == "true")
            {
                
            }
            else if (arg.TypeTerm.Identifier is "long" or "double")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 8;");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 16;");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += 32;");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += BufferUtils.GetTLBytesLength(buffer, offset);");
            }
            else if (arg.TypeTerm.Identifier is "Vector" or "VectorBare" or "vector")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += "+arg.TypeTerm.GetFullyQualifiedIdentifier()+".ReadSize(buffer, offset);");
            }
            else //if (arg.TypeTerm.GetFullyQualifiedIdentifier() is "BoxedObject")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += ObjectReader.ReadSize(buffer[offset..]"+
                          (combinator.Name == null ? "": ", unchecked((int)0x"+combinator.Name+")")+");");
            }
            /*else
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += "+arg.TypeTerm.GetFullyQualifiedIdentifier()+".ReadSize(buffer, offset);");
            }*/
            if (arg.TypeTerm.Identifier != "true")
            {
                index++;
            }
        }
        sb.Append(@"
        return offset;
    }");
    }
}