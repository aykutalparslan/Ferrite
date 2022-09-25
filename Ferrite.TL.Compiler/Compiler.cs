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
using Ferrite.TLParser;
using Microsoft.CodeAnalysis;

namespace Ferrite.TL.Compiler;

public class Compiler
{
    public static void Main(string[] args)
    {
        Generate();
    }
    static void Generate()
    {
        Dictionary<string, List<CombinatorDeclarationSyntax>> types = new();
        Dictionary<string, List<CombinatorDeclarationSyntax>> typesL142 = new();
        SortedSet<string> nameSpaces = new SortedSet<string>();
        nameSpaces.Add("mtproto");
        List<CombinatorDeclarationSyntax> combinators = new();
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(MTProtoSchema.Schema);
        Parser parser = new Parser(lexer);
        var combinator = parser.ParseCombinator();
        while (combinator != null)
        {
            var ns = "";
            if (combinator.Type.NamespaceIdentifier != null)
            {
                ns += combinator.Namespace;
            }

            var id = ns + "." + combinator.Type.Identifier;
            if ((combinator.CombinatorType == CombinatorType.Constructor || 
                combinator.CombinatorType == CombinatorType.Builtin) &&
                !types.ContainsKey(id))
            {
                types.Add(id, new List<CombinatorDeclarationSyntax>() { combinator });
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(combinator, "mtproto");
            }
            else if((combinator.CombinatorType == CombinatorType.Constructor || 
                     combinator.CombinatorType == CombinatorType.Builtin))
            {
                types[id].Add(combinator);
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(combinator, "mtproto");
            }
            else if(combinator.CombinatorType == CombinatorType.Function)
            {
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateFunctionSource(combinator, "mtproto");
            }

            combinator = parser.ParseCombinator();
        }
        
        lexer = new Lexer(Layer142Schema.Schema);
        parser = new Parser(lexer);
        combinator = parser.ParseCombinator();
        while (combinator != null)
        {
            var ns = "";
            if (combinator.Type.NamespaceIdentifier != null)
            {
                ns += combinator.Namespace;
                nameSpaces.Add(combinator.Type.NamespaceIdentifier);
            }

            var id = ns  + "." + combinator.Type.Identifier;
            if (combinator.CombinatorType == CombinatorType.Constructor &&
                !typesL142.ContainsKey(id))
            {
                typesL142.Add(id, new List<CombinatorDeclarationSyntax>() { combinator });
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(combinator, ns);
            }
            else if(combinator.CombinatorType == CombinatorType.Constructor)
            {
                typesL142[id].Add(combinator);
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(combinator, ns);
            }
            else if(combinator.CombinatorType == CombinatorType.Function)
            {
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateFunctionSource(combinator, ns);
            }
            
            combinator = parser.ParseCombinator();
        }
        
        foreach (var item in types)
        {
            if (item.Value[0].Name != null)
            {
                //GenerateBaseType(item.Value, "mtproto");
            }
        }
        foreach (var item in typesL142)
        {
            if (item.Value[0].Name != null)
            {
                //GenerateBaseType(item.Value, item.Value[0].Namespace ?? "");
            }
        }
        
        GenerateObjectReader(combinators, nameSpaces);
        GenerateConstructors(combinators, nameSpaces);
    }
    private static void GenerateBaseType(IReadOnlyList<CombinatorDeclarationSyntax> combinators, string nameSpace)
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim"+(nameSpace.Length > 0? "."+nameSpace:"")+@";

public readonly unsafe struct " + combinators[0].Type.Identifier + @" : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private " + combinators[0].Type.Identifier + @"(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    internal " + combinators[0].Type.Identifier + @"(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new (_buff, Length);
    public ref readonly int Constructor => ref *(int*)_buff;
    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        var ptr = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
");
        bool first = true;
        foreach (var combinator in combinators)
        {
            sb.Append(@"
        "+(!first?"else ":"")+@"if(*(int*)ptr == unchecked((int)0x"+combinator.Name+@"))
        {
            return "+combinator.Identifier+@".Read(data, offset, out bytesRead);
        }");
            first = false;
        }
        sb.Append(@"
        bytesRead = 0;
        return null;
    }

    public static unsafe ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
");
        first = true;
        foreach (var combinator in combinators)
        {
            sb.Append(@"
        "+(!first?"else ":"")+@"if(*(int*)buffer == unchecked((int)0x"+combinator.Name+@"))
        {
            return "+combinator.Identifier+@".Read(buffer, length, offset, out bytesRead);
        }");
            first = false;
        }
        sb.Append(@"
        bytesRead = 0;
        return null;
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        var ptr = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
");
        first = true;
        foreach (var combinator in combinators)
        {
            sb.Append(@"
        "+(!first?"else ":"")+@"if(*(int*)ptr == unchecked((int)0x"+combinator.Name+@"))
        {
            return "+combinator.Identifier+@".ReadSize(data, offset);
        }");
            first = false;
        }
        sb.Append(@"
        return 0;
    }

    public static unsafe int ReadSize(byte* buffer, in int length, in int offset)
    {
");
        first = true;
        foreach (var combinator in combinators)
        {
            sb.Append(@"
        "+(!first?"else ":"")+@"if(*(int*)buffer == unchecked((int)0x"+combinator.Name+@"))
        {
            return "+combinator.Identifier+@".ReadSize(buffer, length, offset);
        }");
            first = false;
        }
        sb.Append(@"
        return 0;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
");
        if (!Directory.Exists("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"")))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":""));
        }
        File.WriteAllText("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"") + 
                          combinators[0].Type.Identifier + "_Wrapper.g.cs",
            sb.ToString());
    }
    private static void GenerateObjectReader(IReadOnlyList<CombinatorDeclarationSyntax> combinators,ICollection<string> nameSpaces)
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Runtime.InteropServices;");
        foreach (var ns in nameSpaces)
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
         foreach (var combinator in combinators)
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
        if (!Directory.Exists("../../../../Ferrite.TL/slim/"))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/slim/");
        }
        File.WriteAllText("../../../../Ferrite.TL/slim/" + "ObjectReader.g.cs",
            sb.ToString());
    }
    private static void GenerateConstructors(IReadOnlyList<CombinatorDeclarationSyntax> combinators,ICollection<string> nameSpaces)
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
        foreach (var combinator in combinators)
        {
            sb.Append(@"
    public const int " + combinator.Identifier + " = unchecked((int)0x" + combinator.Name + @");");
        }
        sb.Append(@"
}
");
        if (!Directory.Exists("../../../../Ferrite.TL/slim/"))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/slim/");
        }
        File.WriteAllText("../../../../Ferrite.TL/slim/" + "Constructors.g.cs",
            sb.ToString());
    }
    private static void GenerateFunctionSource(CombinatorDeclarationSyntax? combinator, string nameSpace)
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

namespace Ferrite.TL.slim"+(nameSpace.Length > 0? "."+nameSpace:"")+@";

public readonly ref struct " + typeName + @"
{
    private readonly Span<byte> _buff;
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
        GenerateCreate(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", data[offset..]);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        var str = @"
}
";
        sourceBuilder.Append(str);
        if (!Directory.Exists("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"")))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":""));
        }
        File.WriteAllText("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"") + 
                          combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }
    private static void GenerateSourceFile(CombinatorDeclarationSyntax? combinator, string nameSpace)
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

namespace Ferrite.TL.slim"+(nameSpace.Length > 0? "."+nameSpace:"")+@";

public readonly ref struct " + typeName + @"
{
    private readonly Span<byte> _buff;
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
        GenerateCreate(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", data[offset..]);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        var str = @"
}
";
        sourceBuilder.Append(str);
        if (!Directory.Exists("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"")))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":""));
        }
        File.WriteAllText("../../../../Ferrite.TL/slim/"+(nameSpace.Length>0?nameSpace+"/":"") + 
                          combinator.Identifier + ".g.cs",
            sourceBuilder.ToString());
    }
    private static void GenerateGetRequiredBufferSize(StringBuilder sb, CombinatorDeclarationSyntax combinator)
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
                sb.Append("BufferUtils.CalculateTLBytesLength(len_" + arg.Identifier+")");
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
    private static void GenerateCreate(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        sb.Append(@"
    public static " + typeName +
                  @" Create(");
        foreach (var arg in combinator.Arguments)
        {
            if (arg.Identifier == "long")
            {
                arg.Identifier = "longitude";
            }
            if (arg.TypeTerm.Identifier == "#")
            {
                
            }
            else if (arg.TypeTerm.Identifier == "true")
            {
                sb.Append("bool " + arg.Identifier + ", ");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string" or "int128" or "int256")
            {
                sb.Append("ReadOnlySpan<byte> " + arg.Identifier + ", ");
            }
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append("ReadOnlySpan<byte> " + (arg.ConditionalDefinition!=null?"?":"")+ arg.Identifier+ ", ");
            }
            else if (arg.TypeTerm.Identifier != "true")
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(typeIdent + (arg.ConditionalDefinition!=null?"?":"") + " " + arg.Identifier +", ");
            }
        }

        sb.Append(@"out IMemoryOwner<byte> memory, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize(");
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
                sb.Append(arg.Identifier + " != null");
            }
            else if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "true")
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append("(" + arg.Identifier + " != null?(("+ arg.TypeTerm.GetFullyQualifiedIdentifier() + ")" +arg.Identifier + ").Length:0)");
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
        memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        memory.Memory.Span.Clear();
        var obj = new " + typeName + @"(memory.Memory.Span[..length]);");
        if (combinator.Name != null)
        {
            sb.Append(@"
        obj.SetConstructor(unchecked((int)0x"+combinator.Name+"));");
        }

        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
        Flags tempFlags = new Flags();");
                foreach (var a in combinator.Arguments)
                {
                    if (a.ConditionalDefinition != null && a.TypeTerm.Identifier == "true")
                    {
                        sb.Append(@"
        tempFlags["+a.ConditionalDefinition.ConditionalArgumentBit+@"] = "+a.Identifier+@";");
                    }
                    else if (a.ConditionalDefinition != null )
                    {
                        sb.Append(@"
        tempFlags["+a.ConditionalDefinition.ConditionalArgumentBit+@"] = "+a.Identifier+@" != null;");
                    }
                }
                sb.Append(@"
        obj.Set_"+arg.Identifier+@"(tempFlags);");            
            
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
        if(" + arg.Identifier + @" != null)"+ @"
        {
            obj.Set_"+arg.Identifier+"(("+ arg.TypeTerm.Identifier +")"+arg.Identifier+@");
        }");
                }
                else if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if(" + arg.Identifier + @" != null)"+ @"
        {
            obj.Set_"+arg.Identifier+"("+arg.Identifier+@");
        }");
                }
                else
                {
                    sb.Append(@"
        obj.Set_"+arg.Identifier+"("+arg.Identifier+@");");         
                }
                    
            }
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if(" + arg.Identifier + @" != null)
        {
            obj.Set_"+arg.Identifier+"((("+ arg.TypeTerm.GetFullyQualifiedIdentifier() +")"+arg.Identifier+@"));
        }");
                }
                else
                {
                    sb.Append(@"
        obj.Set_"+arg.Identifier+"("+arg.Identifier+@");");
                }
            }
            else
            {
                if (arg.ConditionalDefinition != null)
                {
                    sb.Append(@"
        if(" + arg.Identifier + @" != null)
        {
            obj.Set_"+arg.Identifier+"((("+ arg.TypeTerm.GetFullyQualifiedIdentifier() +")"+arg.Identifier+@").ToReadOnlySpan());
        }");
                }
                else
                {
                    sb.Append(@"
        obj.Set_"+arg.Identifier+"("+arg.Identifier+@".ToReadOnlySpan());");
                }
                
            }
        }
        sb.Append(@"
        return obj;
    }");
    }
    private static void GenerateProperties(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        int index = 1;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "#")
            {
                sb.Append(@"
    public ref readonly Flags " + arg.Identifier + @" => MemoryMarshal.Read<double>(_buff[GetOffset(" + index +
                          ", _buff)..]);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in Flags value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "true" && 
                     arg.ConditionalDefinition != null)
            {
                sb.Append(@"
    public readonly bool " + arg.Identifier + @" => flags["+arg.ConditionalDefinition.ConditionalArgumentBit+"];");
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
    public readonly int " + arg.Identifier + @" => MemoryMarshal.Read<int>(_buff[GetOffset(" + index +
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
    public readonly long " + arg.Identifier + @" => MemoryMarshal.Read<long>(_buff[GetOffset(" + index +
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
    public readonly double " + arg.Identifier + @" => MemoryMarshal.Read<double>(_buff[GetOffset(" + index +
                          ", _buff)..]);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in double value)
    {
        MemoryMarshal.Write(_buff[GetOffset("+index+@", _buff)..], ref value);
    }");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier+@" => _buff.Slice(GetOffset("+index+@", _buff), 16);");
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
    public ReadOnlySpan<byte> "+arg.Identifier+@" => _buff.Slice(GetOffset("+index+@", _buff), 32);");
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
    public ReadOnlySpan<byte> "+arg.Identifier+@" => BufferUtils.GetTLBytes(_buff, GetOffset("+index+@", _buff));");
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
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append(@"
    public Span<byte> " + arg.Identifier + @" => ObjectReader.Read(_buff);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset("+index+@", _buff)..]);
    }");
            }
            else 
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
            }

            if (arg.TypeTerm.Identifier != "true")
            {
                index++;
            }
        }
    }
    private static void GenerateGetOffset(StringBuilder sb, CombinatorDeclarationSyntax combinator)
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
        Flags f = MemoryMarshal.Read<double>(_buff[offset..]);");
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
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() is "BoxedObject")
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += ObjectReader.ReadSize(buffer[offset..]"+
                          (combinator.Name == null ? "": ", unchecked((int)0x"+combinator.Name+")")+");");
            }
            else
            {
                sb.Append(@"
        if(index >= "+index+(arg.ConditionalDefinition != null? " && f["+arg.ConditionalDefinition.ConditionalArgumentBit+"]": "")+@") offset += "+arg.TypeTerm.GetFullyQualifiedIdentifier()+".ReadSize(buffer, offset);");
            }
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