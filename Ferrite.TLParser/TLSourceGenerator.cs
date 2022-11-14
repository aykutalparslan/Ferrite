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
    readonly SortedSet<string?> _bareNamespaces = new();
    readonly SortedList<string, CombinatorDeclarationSyntax> _combinators = new();
    private readonly Dictionary<string?, int> _typeCount = new();

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

    private IEnumerable<GeneratedSource> GenerateSources(string nameSpace, Lexer lexer,
        Dictionary<string, List<CombinatorDeclarationSyntax>> types)
    {
        List<CombinatorDeclarationSyntax> combinators = new();
        ParseCombinators(nameSpace, lexer, combinators);
        foreach (var combinator in combinators)
        {
            DoRenameKeywords(combinator);
            var ns = GetNamespace(nameSpace, combinator);
            var id = ns + "." + combinator?.Type?.Identifier;
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

    private static string GetNamespace(string nameSpace, CombinatorDeclarationSyntax combinator)
    {
        var ns = nameSpace;
        if (combinator?.CombinatorType == CombinatorType.Constructor &&
            combinator.Type?.NamespaceIdentifier != null)
        {
            ns += (ns.Length > 0 ? "." : "") + combinator.Type.NamespaceIdentifier;
        }
        else if (combinator?.CombinatorType == CombinatorType.Function &&
                 combinator.Namespace != null)
        {
            ns += (ns.Length > 0 ? "." : "") + combinator.Namespace;
        }

        return ns;
    }

    private void DoRenameKeywords(CombinatorDeclarationSyntax combinator)
    {
        if (_typeCount[combinator.Identifier?.TrimEnd('_')] > 1)
        {
            combinator.Identifier = combinator.Namespace != null
                ? combinator.Namespace.ToPascalCase() + combinator.Identifier
                : combinator.Identifier;
        }
        if (combinator.Identifier?.ToLower() == "file")
        {
            combinator.Identifier = combinator.Namespace?.ToPascalCase()+combinator.Identifier;
        }

        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                if (arg.Identifier == "long")
                {
                    arg.Identifier = "longitude";
                }
                if (arg.Identifier?.ToLowerInvariant() == combinator.Identifier?.ToLowerInvariant() || 
                    arg.Identifier == "out" || arg.Identifier == "length" ||
                    arg.Identifier == "static" || arg.Identifier == "params" ||
                    arg.Identifier == "default" || arg.Identifier == "public" ||
                    arg.Identifier == "readonly" || arg.Identifier == "private")
                {
                    arg.Identifier += "Property";
                }
            }
    }

    private void ParseCombinators(string nameSpace, Lexer lexer, List<CombinatorDeclarationSyntax> combinators)
    {
        var parser = new Parser(lexer);
        var c = parser.ParseCombinator();
        
        while (c != null)
        {
            c.Identifier = c.Identifier?.ToPascalCase();
            c.ContainingNamespace = nameSpace;
            var ns = nameSpace;
            if (c.CombinatorType == CombinatorType.Constructor &&
                c.Type?.NamespaceIdentifier != null)
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

            if (c.Arguments != null)
                foreach (var arg in c.Arguments)
                {
                    arg.Identifier = arg.Identifier?.ToCamelCase();
                }

            c = parser.ParseCombinator();
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
using Ferrite.TL.slim" + (ns.Length > 0 ? "." + ns : "") + ";");
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
        _objectReaders.Add(unchecked((int)0x" + combinator.Name + @"), " + combinator.Identifier + @".Read);
        _sizeReaders.Add(unchecked((int)0x" + combinator.Name + @"), " + combinator.Identifier + @".ReadSize);");
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
            return reader(buff, 0);
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
    public const int " + combinator.ContainingNamespace + "_" + combinator.Identifier + " = unchecked((int)0x" + combinator.Name + @");");
        }

        sb.Append(@"
}
");
        return new GeneratedSource("Constructors.g.cs",
            sb.ToString());
    }

    private GeneratedSource GenerateFunctionSource(CombinatorDeclarationSyntax combinator, string nameSpace)
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

namespace Ferrite.TL.slim" + (nameSpace.Length > 0 ? "." + nameSpace : "") + @";

public readonly ref struct " + typeName + @"
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;");
        GenerateCreate(sourceBuilder, combinator);
        if (combinator.Arguments != null)
        {
            sourceBuilder.Append(
                @"
    public " + typeName + @"(Span<byte> buff)
    {
        _buff = buff;
    }
    " +
                (combinator.Name != null
                    ? @"
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }"
                    : "") +
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
        }

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
        return new GeneratedSource((nameSpace.Length > 0 ? nameSpace.Replace('.', '_') + "_" : "") +
                                   combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }

    private GeneratedSource GenerateSourceFile(CombinatorDeclarationSyntax combinator, string nameSpace)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type?.Identifier);
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
        if (combinator.Arguments != null)
        {
            sourceBuilder.Append(
                @"
    public " + typeName + @"(Span<byte> buff)
    {
        _buff = buff;
    }
    " +
                (combinator.Name != null
                    ? @"
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }"
                    : "") +
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
        }

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
        return new GeneratedSource((nameSpace.Length > 0 ? nameSpace.Replace('.', '_') + "_" : "") +
                                   combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }

    private void GenerateGetRequiredBufferSize(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    public static int GetRequiredBufferSize(");
        bool first = true;
        if (combinator.Arguments != null)
        {
            foreach (var arg in combinator.Arguments)
            {
                if (arg.TypeTerm != null && arg.ConditionalDefinition != null && arg.TypeTerm != null 
                    && arg.TypeTerm.Identifier != "true" &&
                    arg.TypeTerm.IsBare)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    if (arg.TypeTerm?.Identifier is "int" or "long" or "double" or "int128" or "int258" or "Bool")
                    {
                        sb.Append("bool has" + arg.Identifier?.ToPascalCase());
                    }
                    else
                    {
                        sb.Append("bool has" + arg.Identifier?.ToPascalCase() + ", int len" + arg.Identifier?.ToPascalCase());
                    }
                }
                else if (arg.ConditionalDefinition != null && arg.TypeTerm?.Identifier == "Bool")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append("bool has" + arg.Identifier?.ToPascalCase());
                }
                else if (arg.ConditionalDefinition != null && arg.TypeTerm?.Identifier != "true")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append("int len" + arg.Identifier?.ToPascalCase());
                }
                else if (arg.TypeTerm?.Identifier != "#" && arg.TypeTerm?.Identifier != "int" &&
                         arg.TypeTerm?.Identifier != "long" && arg.TypeTerm?.Identifier != "double" &&
                         arg.TypeTerm?.Identifier != "int128" && arg.TypeTerm?.Identifier != "int256" &&
                         arg.TypeTerm?.Identifier != "true" && arg.TypeTerm?.Identifier != "Bool")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append("int len" + arg.Identifier?.ToPascalCase());
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
                if (arg.TypeTerm?.Identifier == "#")
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
                else if (arg.TypeTerm?.Identifier == "true")
                {
                }
                else if (arg.TypeTerm?.Identifier == "int")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    sb.Append(arg.ConditionalDefinition != null ? "(has" + arg.Identifier?.ToPascalCase() + @"?4:0)" : "4");
                }
                else if (arg.TypeTerm?.Identifier == "Bool")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    sb.Append(arg.ConditionalDefinition != null ? "(has" + arg.Identifier?.ToPascalCase() + @"?4:0)" : "4");
                }
                else if (arg.TypeTerm?.Identifier is "long" or "double")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    sb.Append(arg.ConditionalDefinition != null ? "(has" + arg.Identifier?.ToPascalCase() + @"?8:0)" : "8");
                }
                else if (arg.TypeTerm?.Identifier == "int128")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    sb.Append(arg.ConditionalDefinition != null ? "(has" + arg.Identifier?.ToPascalCase() + @"?16:0)" : "16");
                }
                else if (arg.TypeTerm?.Identifier == "int256")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    sb.Append(arg.ConditionalDefinition != null ? "(has" + arg.Identifier?.ToPascalCase() + @"?32:0)" : "32");
                }
                else if (arg.TypeTerm?.Identifier is "bytes" or "string")
                {
                    if (appended)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        appended = true;
                    }

                    if (arg.ConditionalDefinition != null)
                    {
                        sb.Append("(has" + arg.Identifier?.ToPascalCase() + "?BufferUtils.CalculateTLBytesLength(len" +
                                  arg.Identifier?.ToPascalCase() +
                                  "):0)");
                    }
                    else
                    {
                        sb.Append("BufferUtils.CalculateTLBytesLength(len" + arg.Identifier?.ToPascalCase() + ")");
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

                    sb.Append("len" + arg.Identifier?.ToPascalCase());
                }
            }

            if (!appended)
            {
                sb.Append("0");
            }
        }

        sb.Append(@";
    }");
    }

    private void GenerateCreate(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type?.Identifier);
        sb.Append(@"
    public " + typeName +
                  @"(");
        if (combinator.Arguments != null)
        {
            int count = combinator.Arguments.Count;
            GenerateCreateArguments(sb, combinator, count);
        }

        sb.Append(@")
    {
        var bufferLength = GetRequiredBufferSize(");
        bool first = true;
        GenerateRequiredBufferSizeArguments(sb, combinator, first);
        sb.Append(@");
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(bufferLength);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..bufferLength];");
        if (combinator.Name != null)
        {
            sb.Append(@"
        SetConstructor(unchecked((int)0x" + combinator.Name + "));");
        }
        GenerateSetValues(sb, combinator);
        sb.Append(@"
    }");
    }

    private static void GenerateSetValues(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                if (arg.TypeTerm?.Identifier == "#")
                {
                    GenerateSetFlagsValue(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier == "true")
                {
                }
                else if (arg.TypeTerm is { IsBare: true, OptionalType: null })
                {
                    GenerateSetBareTypeValue(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier is "Vector" or "VectorBare" or "vector")
                {
                    GenerateSetVectorValue(sb, arg);
                }
                else
                {
                    GenerateSetDefaultValue(sb, arg);
                }
            }
    }

    private static void GenerateSetFlagsValue(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
        Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(" + arg.Identifier + ");");
    }

    private static void GenerateSetBareTypeValue(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        if (arg.ConditionalDefinition != null &&
            arg.TypeTerm?.Identifier != "string" && arg.TypeTerm?.Identifier != "bytes" &&
            arg.TypeTerm?.Identifier != "int128" && arg.TypeTerm?.Identifier != "int258")
        {
            sb.Append(@"
        if(" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])" +
                      @"
        {
            Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @");
        }");
        }
        else if (arg.ConditionalDefinition != null)
        {
            sb.Append(@"
        if(" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])" +
                      @"
        {
            Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @");
        }");
        }
        else
        {
            sb.Append(@"
        Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @");");
        }
    }

    private static void GenerateSetDefaultValue(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        if (arg.ConditionalDefinition != null)
        {
            sb.Append(@"
        if(" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])
        {
            Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @");
        }");
        }
        else
        {
            sb.Append(@"
        Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @");");
        }
    }

    private static void GenerateSetVectorValue(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        if (arg.ConditionalDefinition != null)
        {
            sb.Append(@"
        if(" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit + @"])
        {
            Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @".ToReadOnlySpan());
        }");
        }
        else
        {
            sb.Append(@"
        Set" + arg.Identifier?.FirstLetterToUpperCase() + "(" + arg.Identifier + @".ToReadOnlySpan());");
        }
    }

    private static void GenerateRequiredBufferSizeArguments(StringBuilder sb, CombinatorDeclarationSyntax combinator,
        bool first)
    {
        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                if (arg.ConditionalDefinition != null && arg.TypeTerm != null &&
                    arg.TypeTerm?.Identifier != "true" && arg.TypeTerm!.IsBare)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    if (arg.TypeTerm?.Identifier != "int" && arg.TypeTerm?.Identifier != "long" &&
                        arg.TypeTerm?.Identifier != "double" && arg.TypeTerm?.Identifier != "int128" &&
                        arg.TypeTerm?.Identifier != "int256")
                    {
                        sb.Append(arg.ConditionalDefinition.Identifier + "[" +
                                  arg.ConditionalDefinition.ConditionalArgumentBit + "], " + arg.Identifier +
                                  ".Length");
                    }
                    else
                    {
                        sb.Append(arg.ConditionalDefinition.Identifier + "[" +
                                  arg.ConditionalDefinition.ConditionalArgumentBit + "]");
                    }
                }
                else if (arg.ConditionalDefinition != null && arg.TypeTerm?.Identifier == "Bool")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append(arg.ConditionalDefinition.Identifier + "[" +
                              arg.ConditionalDefinition.ConditionalArgumentBit + "]");
                }
                else if (arg.ConditionalDefinition != null && arg.TypeTerm?.Identifier != "true")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append("(" + arg.ConditionalDefinition.Identifier + "[" +
                              arg.ConditionalDefinition.ConditionalArgumentBit + "]?" + arg.Identifier + ".Length:0)");
                }
                else if (arg.TypeTerm?.Identifier != "#" && arg.TypeTerm?.Identifier != "int" &&
                         arg.TypeTerm?.Identifier != "long" && arg.TypeTerm?.Identifier != "double" &&
                         arg.TypeTerm?.Identifier != "int128" && arg.TypeTerm?.Identifier != "int256" &&
                         arg.TypeTerm?.Identifier != "true" && arg.TypeTerm?.Identifier != "Bool")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    sb.Append(arg.Identifier + ".Length");
                }
            }
    }

    private static void GenerateCreateArguments(StringBuilder sb, CombinatorDeclarationSyntax combinator, int count)
    {
        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                bool comma = --count != 0;

                if (arg.TypeTerm?.Identifier == "#")
                {
                    sb.Append("Flags " + arg.Identifier + (comma ? ", " : ""));
                }
                else if (arg.TypeTerm?.Identifier is "true" or "Bool")
                {
                    sb.Append("bool " + arg.Identifier + (comma ? ", " : ""));
                }
                else if (arg.TypeTerm?.Identifier is "bytes" or "string" or "int128" or "int256")
                {
                    sb.Append("ReadOnlySpan<byte> " + arg.Identifier + (comma ? ", " : ""));
                }
                else if (arg.TypeTerm?.Identifier is "int" or "double" or "long")
                {
                    string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                    sb.Append(typeIdent + " " + arg.Identifier + (comma ? ", " : ""));
                }
                else if (arg.TypeTerm?.Identifier is "Vector" or "VectorBare" or "vector")
                {
                    string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                    sb.Append(typeIdent + " " + arg.Identifier + (comma ? ", " : ""));
                }
                else if (arg.TypeTerm?.Identifier != "true")
                {
                    sb.Append("ReadOnlySpan<byte> " + arg.Identifier + (comma ? ", " : ""));
                }
            }
    }

    private void GenerateProperties(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        int index = 1;
        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                if (arg.TypeTerm?.Identifier == "#")
                {
                    GenerateFlagsProperty(sb, arg, index);
                }
                else if (arg.TypeTerm?.Identifier == "true" &&
                         arg.ConditionalDefinition != null)
                {
                    GenerateFlagsAccessorProperty(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier == "Bool")
                {
                    GenerateTLBoolProperty(sb, arg, index);
                }
                else if (arg.TypeTerm?.Identifier is "int" or "long" or "double")
                {
                    GenerateBareTypeProperty(sb, arg, index);
                }
                else if (arg.TypeTerm?.Identifier == "int128")
                {
                    GenerateFixedSizeProperty(sb, arg, index, 16);
                }
                else if (arg.TypeTerm?.Identifier == "int256")
                {
                    GenerateFixedSizeProperty(sb, arg, index, 32);
                }
                else if (arg.TypeTerm?.Identifier is "bytes" or "string")
                {
                    GenerateStringProperty(sb, arg, index);
                }
                else if (arg.TypeTerm?.Identifier is "Vector" or "VectorBare" or "vector")
                {
                    GenerateVectorProperty(sb, arg, index);
                }
                else
                {
                    GenerateObjectProperty(sb, arg, index);
                }

                if (arg.TypeTerm?.Identifier != "true")
                {
                    index++;
                }
            }
    }

    private static void GenerateFlagsProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public readonly Flags " + arg.Identifier + @" => new Flags(MemoryMarshal.Read<int>(_buff[GetOffset(" + index +
                  ", _buff)..]));");
        sb.Append(@"
    private void Set" + arg.Identifier?.ToPascalCase() + @"(Flags value)
    {
        MemoryMarshal.Write(_buff[GetOffset(" + index + @", _buff)..], ref value);
    }");
    }

    private static void GenerateFlagsAccessorProperty(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
    public readonly bool " + arg.Identifier?.FirstLetterToUpperCase() + @" => " + arg.ConditionalDefinition?.Identifier + "[" +
                  arg.ConditionalDefinition?.ConditionalArgumentBit + "];");
    }

    private static void GenerateTLBoolProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public readonly bool " + arg.Identifier?.FirstLetterToUpperCase() + " => " + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit + "] ? false : "
                      : "") +
                  "MemoryMarshal.Read<int>(_buff[GetOffset(" + index +
                  ", _buff)..]) == unchecked((int)0x997275b5);");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(bool value)
    {
        int t = unchecked((int)0x997275b5);
        int f = unchecked((int)0xbc799737);
        if(value)
        {
            MemoryMarshal.Write(_buff[GetOffset(" + index + @", _buff)..], ref t);
        }
        else 
        {
            MemoryMarshal.Write(_buff[GetOffset(" + index + @", _buff)..], ref f);
        }
    }");
    }

    private static void GenerateFixedSizeProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index, int size)
    {
        sb.Append(@"
    public ReadOnlySpan<byte> " + arg.Identifier?.FirstLetterToUpperCase() + " => " + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit +
                        "] ? new ReadOnlySpan<byte>() : "
                      : "") +
                  " _buff.Slice(GetOffset(" + index + @", _buff), "+size+@");");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(ReadOnlySpan<byte> value)
    {
        if(value.Length != "+size+@")
        {
            return;
        }
        value.CopyTo(_buff.Slice(GetOffset(" + index + @", _buff), "+size+@"));
    }");
    }

    private static void GenerateObjectProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public Span<byte> " + arg.Identifier?.FirstLetterToUpperCase() + " => " + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit + "] ? new Span<byte>() : "
                      : "") +
                  "ObjectReader.Read(_buff);");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset(" + index + @", _buff)..]);
    }");
    }

    private static void GenerateVectorProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public " + arg.TypeTerm?.GetFullyQualifiedIdentifier() + " " + arg.Identifier?.FirstLetterToUpperCase() + " => "
                  + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit + "] ? new "
                        + arg.TypeTerm?.GetFullyQualifiedIdentifier() + "() : "
                      : "")
                  + "new " + arg.TypeTerm?.GetFullyQualifiedIdentifier() +
                  "(_buff.Slice(GetOffset(" + index + @", _buff)));");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset(" + index + @", _buff)..]);
    }");
    }

    private static void GenerateStringProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public ReadOnlySpan<byte> " + arg.Identifier?.FirstLetterToUpperCase() + " => " + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit +
                        "] ? new ReadOnlySpan<byte>() : "
                      : "") +
                  " BufferUtils.GetTLBytes(_buff, GetOffset(" + index + @", _buff));");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"(ReadOnlySpan<byte> value)
    {
        if(value.Length == 0)
        {
            return;
        }
        var offset = GetOffset(" + index + @", _buff);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset);
        if(_buff.Length < offset + lenBytes + value.Length) return;
        value.CopyTo(_buff[(offset + lenBytes)..]);
    }");
    }

    private static void GenerateBareTypeProperty(StringBuilder sb, SimpleArgumentSyntax arg, int index)
    {
        sb.Append(@"
    public readonly "+arg.TypeTerm?.Identifier+" " + arg.Identifier?.FirstLetterToUpperCase() + " => " + (arg.ConditionalDefinition != null
                      ? "!"+arg.ConditionalDefinition.Identifier+"[" + arg.ConditionalDefinition.ConditionalArgumentBit + "] ? 0 : "
                      : "") +
                  "MemoryMarshal.Read<"+arg.TypeTerm?.Identifier+">(_buff[GetOffset(" + index +
                  ", _buff)..]);");
        sb.Append(@"
    private void Set" + arg.Identifier?.FirstLetterToUpperCase() + @"("+arg.TypeTerm?.Identifier+@" value)
    {
        MemoryMarshal.Write(_buff[GetOffset(" + index + @", _buff)..], ref value);
    }");
    }

    private void GenerateBuilder(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    public ref struct TLObjectBuilder
    {
        public TLObjectBuilder(){}
");
        if (combinator.Arguments != null)
            foreach (var arg in combinator.Arguments)
            {
                if (arg.TypeTerm?.Identifier == "#")
                {
                    GenerateBuilderFlags(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier == "true" &&
                         arg.ConditionalDefinition != null)
                {
                    GenerateBuilderSetFlags(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier is "int" or "long" or "double" or "Bool")
                {
                    GenerateBuilderAppendBareType(sb, arg);
                }
                else if (arg.TypeTerm?.Identifier is "Vector" or "VectorBare" or "vector")
                {
                    GenerateBuilderAppendVector(sb, arg);
                }
                else
                {
                    GenerateBuilderAppendDefault(sb, arg);
                }
            }

        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type?.Identifier);
        sb.Append(@"
        public " + typeName + @" Build()
        {
            return new " + typeName);

        GenerateBuilderReturnParameters(sb, combinator);

        sb.Append(@"
        }
    }
    public static TLObjectBuilder Builder()
    {
        return new TLObjectBuilder();
    }
");
    }

    private static void GenerateBuilderFlags(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
        private Flags _" + arg.Identifier + " = new Flags();");
    }

    private static void GenerateBuilderSetFlags(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
        public TLObjectBuilder " + arg.Identifier?.FirstLetterToUpperCase() + @"(bool value)
        {
            _" + arg.ConditionalDefinition?.Identifier + "[" + arg.ConditionalDefinition?.ConditionalArgumentBit +
                  @"] = value;
            return this;
        }");
    }

    private static void GenerateBuilderAppendBareType(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
        private "+arg.TypeTerm?.Identifier?.ToLower()+" _" + arg.Identifier + @";
        /// <summary>
        /// This parameter is "+(arg.ConditionalDefinition == null ? "":"NOT ") +@"required.
        /// </summary>
        /// <param name=""value"">"+ arg.TypeTerm?.GetFullyQualifiedIdentifier() +@"</param>
        public TLObjectBuilder " + arg.Identifier?.FirstLetterToUpperCase() + "("+arg.TypeTerm?.Identifier?.ToLower()+@" value)
        {
            _" + arg.Identifier + @" = value;"
                  + (arg.ConditionalDefinition != null
                      ? @"
            _" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit +
                        "] = true;"
                      : "") +
                  @"
            return this;
        }");
    }

    private static void GenerateBuilderAppendVector(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        string typeIdent = arg.TypeTerm?.GetFullyQualifiedIdentifier() ?? string.Empty;
        sb.Append(@"
        private " + typeIdent + " _" + arg.Identifier + @";
        /// <summary>
        /// This parameter is "+(arg.ConditionalDefinition == null ? "":"NOT ") +@"required.
        /// </summary>
        /// <param name=""value"">"+ arg.TypeTerm?.GetFullyQualifiedIdentifier() +@"</param>
        public TLObjectBuilder " + arg.Identifier?.FirstLetterToUpperCase() + "(" + typeIdent + @" value)
        {
            _" + arg.Identifier + @" = value;"
                  + (arg.ConditionalDefinition != null
                      ? @"
            _" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit +
                        "] = true;"
                      : "") +
                  @"
            return this;
        }");
    }

    private static void GenerateBuilderAppendDefault(StringBuilder sb, SimpleArgumentSyntax arg)
    {
        sb.Append(@"
        private ReadOnlySpan<byte> _" + arg.Identifier + @";
        /// <summary>
        /// This parameter is "+(arg.ConditionalDefinition == null ? "":"NOT ") +@"required.
        /// </summary>
        /// <param name=""value"">"+ arg.TypeTerm?.GetFullyQualifiedIdentifier() +@"</param>
        public TLObjectBuilder " + arg.Identifier?.FirstLetterToUpperCase() + @"(ReadOnlySpan<byte> value)
        {
            _" + arg.Identifier + @" = value;"
                  + (arg.ConditionalDefinition != null
                      ? @"
            _" + arg.ConditionalDefinition.Identifier + "[" + arg.ConditionalDefinition.ConditionalArgumentBit +
                        "] = true;"
                      : "") +
                  @"
            return this;
        }");
    }

    private static void GenerateBuilderReturnParameters(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"(");
        if (combinator.Arguments != null)
        {
            int count = combinator.Arguments.Count;
            foreach (var arg in combinator.Arguments)
            {
                bool comma = --count != 0;


                if (arg.TypeTerm?.Identifier is "true")
                {
                    sb.Append("_" + arg.ConditionalDefinition!.Identifier + "[" +
                              arg.ConditionalDefinition!.ConditionalArgumentBit +
                              "]" + (comma ? ", " : ""));
                }
                else
                {
                    sb.Append("_" + arg.Identifier + (comma ? ", " : ""));
                }
            }
        }

        sb.Append(@");");
    }

    private void GenerateGetOffset(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = " + (combinator.Name != null ? "4" : "0") + @";");
        bool hasFlags = false;
        if (combinator.Arguments != null)
        {
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
                if (arg.TypeTerm?.Identifier == "int")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 4;");
                }
                else if (arg.TypeTerm?.Identifier == "Bool")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 4;");
                }
                else if (arg.TypeTerm?.Identifier == "#")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 4;");
                }
                else if (arg.TypeTerm?.Identifier == "true")
                {
                }
                else if (arg.TypeTerm?.Identifier is "long" or "double")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 8;");
                }
                else if (arg.TypeTerm?.Identifier == "int128")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 16;");
                }
                else if (arg.TypeTerm?.Identifier == "int256")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += 32;");
                }
                else if (arg.TypeTerm?.Identifier is "bytes" or "string")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += BufferUtils.GetTLBytesLength(buffer, offset);");
                }
                else if (arg.TypeTerm?.Identifier is "Vector" or "VectorBare" or "vector")
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += " + arg.TypeTerm.GetFullyQualifiedIdentifier() +
                              ".ReadSize(buffer, offset);");
                }
                else
                {
                    sb.Append(@"
        if(index >= " + index +
                              (arg.ConditionalDefinition != null
                                  ? " && f[" + arg.ConditionalDefinition.ConditionalArgumentBit + "]"
                                  : "") + @") offset += ObjectReader.ReadSize(buffer[offset..]);");
                }

                if (arg.TypeTerm?.Identifier != "true")
                {
                    index++;
                }
            }
        }

        sb.Append(@"
        return offset;
    }");
    }
}