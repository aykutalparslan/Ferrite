using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Ferrite.TLParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ferrite.TLParser;

//https://github.com/dotnet/roslyn-sdk/blob/main/samples/CSharp/SourceGenerators/SourceGeneratorSamples/MathsGenerator.cs#L467
[Generator]
public class TLGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        Dictionary<string, List<CombinatorDeclarationSyntax>> types = new();
        List<CombinatorDeclarationSyntax> combinators = new();
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(@"resPQ#05162463 nonce:int128 server_nonce:int128 pq:bytes server_public_key_fingerprints:Vector<long> = ResPQ;
p_q_inner_data#83c95aec pq:bytes p:bytes q:bytes nonce:int128 server_nonce:int128 new_nonce:int256 = P_Q_inner_data;
p_q_inner_data_temp#3c6a84d4 pq:bytes p:bytes q:bytes nonce:int128 server_nonce:int128 new_nonce:int256 expires_in:int = P_Q_inner_data;
p_q_inner_data_dc#a9f55f95 pq:bytes p:bytes q:bytes nonce:int128 server_nonce:int128 new_nonce:int256 dc:int = P_Q_inner_data;
p_q_inner_data_temp_dc#56fddf88 pq:bytes p:bytes q:bytes nonce:int128 server_nonce:int128 new_nonce:int256 dc:int expires_in:int = P_Q_inner_data;
server_DH_params_ok#d0e8075c nonce:int128 server_nonce:int128 encrypted_answer:bytes = Server_DH_Params;
server_DH_inner_data#b5890dba nonce:int128 server_nonce:int128 g:int dh_prime:bytes g_a:bytes server_time:int = Server_DH_inner_data;
client_DH_inner_data#6643b654 nonce:int128 server_nonce:int128 retry_id:long g_b:bytes = Client_DH_Inner_Data;
dh_gen_ok#3bcbf734 nonce:int128 server_nonce:int128 new_nonce_hash1:int128 = Set_client_DH_params_answer;
dh_gen_retry#46dc1fb9 nonce:int128 server_nonce:int128 new_nonce_hash2:int128 = Set_client_DH_params_answer;
dh_gen_fail#a69dae02 nonce:int128 server_nonce:int128 new_nonce_hash3:int128 = Set_client_DH_params_answer;
bind_auth_key_inner#75a3f765 nonce:long temp_auth_key_id:long perm_auth_key_id:long temp_session_id:long expires_at:int = BindAuthKeyInner;
rpc_result#f35c6d01 req_msg_id:long result:Object = RpcResult;
rpc_error#2144ca19 error_code:int error_message:string = RpcError;
rpc_answer_unknown#5e2ad36e = RpcDropAnswer;
rpc_answer_dropped_running#cd78e586 = RpcDropAnswer;
rpc_answer_dropped#a43ad8b7 msg_id:long seq_no:int bytes:int = RpcDropAnswer;
future_salt#0949d9dc valid_since:int valid_until:int salt:long = FutureSalt;
future_salts#ae500895 req_msg_id:long now:int salts:vector<future_salt> = FutureSalts;
pong#347773c5 msg_id:long ping_id:long = Pong;
destroy_session_ok#e22045fc session_id:long = DestroySessionRes;
destroy_session_none#62d350c9 session_id:long = DestroySessionRes;
new_session_created#9ec20908 first_msg_id:long unique_id:long server_salt:long = NewSession;
msg_container#73f1f8dc messages:vector<%Message> = MessageContainer;
message msg_id:long seqno:int bytes:int body:Object = Message;
msg_copy#e06046b2 orig_message:Message = MessageCopy;
gzip_packed#3072cfa1 packed_data:bytes = GzipPacked;
msgs_ack#62d6b459 msg_ids:Vector<long> = MsgsAck;
bad_msg_notification#a7eff811 bad_msg_id:long bad_msg_seqno:int error_code:int = BadMsgNotification;
bad_server_salt#edab447b bad_msg_id:long bad_msg_seqno:int error_code:int new_server_salt:long = BadMsgNotification;
msg_resend_req#7d861a08 msg_ids:Vector<long> = MsgResendReq;
msgs_state_req#da69fb52 msg_ids:Vector<long> = MsgsStateReq;
msgs_state_info#04deb57d req_msg_id:long info:bytes = MsgsStateInfo;
msgs_all_info#8cc0d131 msg_ids:Vector<long> info:bytes = MsgsAllInfo;
msg_detailed_info#276d3ec6 msg_id:long answer_msg_id:long bytes:int status:int = MsgDetailedInfo;
msg_new_detailed_info#809db6df answer_msg_id:long bytes:int status:int = MsgDetailedInfo;
destroy_auth_key_ok#f660e1d4 = DestroyAuthKeyRes;
destroy_auth_key_none#0a9f2259 = DestroyAuthKeyRes;
destroy_auth_key_fail#ea109b13 = DestroyAuthKeyRes;
---functions---
req_pq_multi#be7e8ef1 nonce:int128 = ResPQ;
req_DH_params#d712e4be nonce:int128 server_nonce:int128 p:bytes q:bytes public_key_fingerprint:long encrypted_data:bytes = Server_DH_Params;
set_client_DH_params#f5045f1f nonce:int128 server_nonce:int128 encrypted_data:bytes = Set_client_DH_params_answer;
rpc_drop_answer#58e4a740 req_msg_id:long = RpcDropAnswer;
get_future_salts#b921bd04 num:int = FutureSalts;
ping#7abe77ec ping_id:long = Pong;
ping_delay_disconnect#f3427b8c ping_id:long disconnect_delay:int = Pong;
destroy_session#e7512126 session_id:long = DestroySessionRes;
http_wait#9299359f max_delay:int wait_after:int max_wait:int = HttpWait;
destroy_auth_key#d1435160 = DestroyAuthKeyRes;
");
        Parser parser = new Parser(lexer);
        var combinator = parser.ParseCombinator();
        while (combinator != null)
        {
            var ns = "";
            if (combinator.Type.NamespaceIdentifier != null)
            {
                ns += combinator.Namespace + ".";
            }

            var id = ns + combinator.Type.Identifier;
            if (combinator.CombinatorType == CombinatorType.Constructor &&
                !types.ContainsKey(id))
            {
                types.Add(id, new List<CombinatorDeclarationSyntax>() { combinator });
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(context, combinator);
            }
            else if(combinator.CombinatorType == CombinatorType.Constructor)
            {
                types[id].Add(combinator);
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateSourceFile(context, combinator);
            }
            else if(combinator.CombinatorType == CombinatorType.Function)
            {
                if (combinator.Name != null)
                {
                    combinators.Add(combinator);
                }
                GenerateFunctionSource(context, combinator);
            }

            combinator = parser.ParseCombinator();
        }

        foreach (var item in types)
        {
            if (item.Value[0].Name != null)
            {
                GenerateBaseType(context, item.Value);
            }
        }
        GenerateBoxedObject(context, combinators);
    }
    private void GenerateBaseType(GeneratorExecutionContext context, IReadOnlyList<CombinatorDeclarationSyntax> combinators)
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct " + combinators[0].Type.Identifier + @" : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private " + combinators[0].Type.Identifier + @"(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
    }
    internal " + combinators[0].Type.Identifier + @"(byte* buffer, in int length)
    {
        _buff = buffer;
        Length = length;
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
}
");
        context.AddSource(combinators[0].Type.Identifier + "_Wrapper.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }
    private void GenerateBoxedObject(GeneratorExecutionContext context, IReadOnlyList<CombinatorDeclarationSyntax> combinators)
    {
        StringBuilder sb = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct BoxedObject : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private BoxedObject(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
    }
    internal BoxedObject(byte* buffer, in int length)
    {
        _buff = buffer;
        Length = length;
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
}
");
        context.AddSource("BoxedObject.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }
    private void GenerateFunctionSource(GeneratorExecutionContext context, CombinatorDeclarationSyntax? combinator)
    {
        var typeName = combinator.Identifier;
        StringBuilder sourceBuilder = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct " + typeName + @" : ITLObjectReader, ITLSerializable, IDisposable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private " + typeName + @"(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private " + typeName + @"(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    "+
    (combinator.Name != null ?                                                    
    @"
    public ref readonly int Constructor => ref *(int*)_buff;

    private void SetConstructor(int constructor)
    {
        var p = (int*)_buff;
        *p = constructor;
    }": "")+
    
    @"
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new (_buff, Length);
    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
                                                        @", (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new " + typeName + @"(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
                                                        @", buffer + offset, length);
        var obj = new " + typeName + @"(buffer + offset, bytesRead, null);
        return obj;
    }
");
        GenerateGetRequiredBufferSize(sourceBuilder, combinator);
        GenerateCreate(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", buffer + offset, length);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        var str = @"
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
";
        sourceBuilder.Append(str);
        // inject the created source into the users compilation
        context.AddSource(combinator.Identifier + ".g.cs",
            SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
    private void GenerateSourceFile(GeneratorExecutionContext context, CombinatorDeclarationSyntax? combinator)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        StringBuilder sourceBuilder = new StringBuilder(@"//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct " + typeName + @" : ITLObjectReader, ITLSerializable, IDisposable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private " + typeName + @"(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private " + typeName + @"(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    "+
    (combinator.Name != null ?                                                    
    @"
    public " + combinator.Type.Identifier + @" GetAs" + combinator.Type.Identifier + @"()
    {
        return new " + combinator.Type.Identifier + @"(_buff, Length);
    }
    public ref readonly int Constructor => ref *(int*)_buff;

    private void SetConstructor(int constructor)
    {
        var p = (int*)_buff;
        *p = constructor;
    }": "")+
    
    @"
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new (_buff, Length);
    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
                                                        @", (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new " + typeName + @"(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(" + (combinator.Arguments.Count + 1) +
                                                        @", buffer + offset, length);
        var obj = new " + typeName + @"(buffer + offset, bytesRead, null);
        return obj;
    }
");
        GenerateGetRequiredBufferSize(sourceBuilder, combinator);
        GenerateCreate(sourceBuilder, combinator);
        sourceBuilder.Append(@"
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return GetOffset(" + (combinator.Arguments.Count + 1) +
                             @", buffer + offset, length);
    }");
        GenerateProperties(sourceBuilder, combinator);
        GenerateGetOffset(sourceBuilder, combinator);
        var str = @"
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
";
        sourceBuilder.Append(str);
        // inject the created source into the users compilation
        context.AddSource(combinator.Identifier + ".g.cs",
            SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        
    }

    private void GenerateGetRequiredBufferSize(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    public static int GetRequiredBufferSize(");
        bool first = true;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "int" &&
                arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append("bool has_" + arg.Identifier);
            }
            else if (arg.TypeTerm.Identifier != "Flags" && arg.TypeTerm.Identifier != "int" &&
                     arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                     arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256")
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
        if (combinator.Name != null)
        {
            sb.Append("4");
            sb.Append(combinator.Arguments.Count > 0 ? " + " : ";");
        }

        for (int i = 0; i < combinator.Arguments.Count; i++)
        {
            var arg = combinator.Arguments[i];
            if (arg.TypeTerm.Identifier == "Flags")
            {
                sb.Append("4");
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?""4"":""0"")" : "4");
            }
            else if (arg.TypeTerm.Identifier is "long" or "double")
            {
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?""8"":""0"")" : "8");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?""16"":""0"")" : "16");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                sb.Append(arg.ConditionalDefinition != null ? "(has_" + arg.Identifier + @"?""32"":""0"")" : "32");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                sb.Append("BufferUtils.CalculateTLBytesLength(len_" + arg.Identifier+")");
            }
            else
            {
                sb.Append("len_" + arg.Identifier);
            }

            sb.Append(i == combinator.Arguments.Count - 1 ? ";" : " + ");
        }

        sb.Append(@"
    }");
    }

    private void GenerateCreate(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        var typeName = (combinator.Name != null ? combinator.Identifier : combinator.Type.Identifier);
        sb.Append(@"
    public static " + typeName +
                  @" Create(MemoryPool<byte> pool");
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier is "bytes" or "string" or "int128" or "int256")
            {
                sb.Append(", ReadOnlySpan<byte> " + arg.Identifier);
            }
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append(", ITLSerializable " + arg.Identifier);
            }
            else
            {
                string typeIdent = arg.TypeTerm.GetFullyQualifiedIdentifier();
                sb.Append(", " + typeIdent + " " + arg.Identifier);
            }
        }

        sb.Append(@")
    {
        var length = GetRequiredBufferSize(");
        bool first = true;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.ConditionalDefinition != null && arg.TypeTerm.Identifier != "int" &&
                arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256")
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append(arg.Identifier + " ?= null");
            }
            else if (arg.TypeTerm.Identifier != "int" &&
                     arg.TypeTerm.Identifier != "long" && arg.TypeTerm.Identifier != "double" &&
                     arg.TypeTerm.Identifier != "int128" && arg.TypeTerm.Identifier != "int256")
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
        var memory = pool.Rent(length);
        var obj = new " + typeName + @"(memory.Memory.Span[..length], memory);");
        if (combinator.Name != null)
        {
            sb.Append(@"
        obj.SetConstructor(unchecked((int)0x"+combinator.Name+"));");
        }

        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.IsBare && arg.TypeTerm.OptionalType == null)
            {
                sb.Append(@"
        obj.Set_"+arg.Identifier+"("+arg.Identifier+");");            
            }
            else
            {
                sb.Append(@"
        obj.Set_"+arg.Identifier+"("+arg.Identifier+".ToReadOnlySpan());");
            }
        }
        sb.Append(@"
        return obj;
    }");
    }

    private void GenerateProperties(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        int index = 1;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "Flags")
            {
                sb.Append(@"
    public ref readonly Flags " + arg.Identifier + @" => ref *(Flags*)(_buff + GetOffset("+index+", _buff, Length));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in Flags value)
    {
        var p = (Flags*)(_buff + GetOffset("+index+@", _buff, Length));
        *p = value;
    }");
            }
            else if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
    public ref readonly int " + arg.Identifier + @" => ref *(int*)(_buff + GetOffset("+index+", _buff, Length));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in int value)
    {
        var p = (int*)(_buff + GetOffset("+index+@", _buff, Length));
        *p = value;
    }");
            }
            else if (arg.TypeTerm.Identifier == "long")
            {
                sb.Append(@"
    public ref readonly long "+arg.Identifier+@" => ref *(long*)(_buff + GetOffset("+index+", _buff, Length));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in long value)
    {
        var p = (long*)(_buff + GetOffset("+index+@", _buff, Length));
        *p = value;
    }");
            }
            else if (arg.TypeTerm.Identifier == "double")
            {
                sb.Append(@"
    public ref readonly double "+arg.Identifier+@" => ref *(double*)(_buff + GetOffset("+index+", _buff, Length));");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(in double value)
    {
        var p = (double*)(_buff + GetOffset("+index+@", _buff, Length));
        *p = value;
    }");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier+@" => new (_buff + GetOffset("+index+@", _buff, Length), 16);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length != 16)
        {
            return;
        }
        fixed (byte* p = value)
        {
            int offset = GetOffset("+index+@", _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, 16);
        }
    }");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier+@" => new (_buff + GetOffset("+index+@", _buff, Length), 32);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length != 32)
        {
            return;
        }
        fixed (byte* p = value)
        {
            int offset = GetOffset("+index+@", _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, 32);
        }
    }");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                sb.Append(@"
    public ReadOnlySpan<byte> "+arg.Identifier+@" => BufferUtils.GetTLBytes(_buff, GetOffset("+index+@", _buff, Length), Length);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        if(value.Length == 0)
        {
            return;
        }
        var offset = GetOffset("+index+@", _buff, Length);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset, Length);
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset + lenBytes,
                Length - offset, value.Length);
        }
    }");
            }
            else if (arg.TypeTerm.GetFullyQualifiedIdentifier() == "BoxedObject")
            {
                sb.Append(@"
    public ITLSerializable "+arg.Identifier+@" => "+
                          arg.TypeTerm.GetFullyQualifiedIdentifier() 
                          +@".Read(_buff, Length, GetOffset("+index+@", _buff, Length), out var bytesRead);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset("+index+@", _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, value.Length);
        }
    }");
            }
            else 
            {
                sb.Append(@"
    public "+ arg.TypeTerm.GetFullyQualifiedIdentifier() +" "+arg.Identifier+@" => ("+arg.TypeTerm.GetFullyQualifiedIdentifier() +")"+ 
                          arg.TypeTerm.GetFullyQualifiedIdentifier() 
                          +@".Read(_buff, Length, GetOffset("+index+@", _buff, Length), out var bytesRead);");
                sb.Append(@"
    private void Set_"+arg.Identifier+@"(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset("+index+@", _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, value.Length);
        }
    }");
            }

            index++;
        }
    }
    
    private void GenerateGetOffset(StringBuilder sb, CombinatorDeclarationSyntax combinator)
    {
        sb.Append(@"
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = "+(combinator.Name != null?"4":"0")+@";");
        int index = 2;
        foreach (var arg in combinator.Arguments)
        {
            if (arg.TypeTerm.Identifier == "int")
            {
                sb.Append(@"
        if(index >= "+index+@") offset += 4;");
            }
            else if (arg.TypeTerm.Identifier is "long" or "double")
            {
                sb.Append(@"
        if(index >= "+index+@") offset += 8;");
            }
            else if (arg.TypeTerm.Identifier == "int128")
            {
                sb.Append(@"
        if(index >= "+index+@") offset += 16;");
            }
            else if (arg.TypeTerm.Identifier == "int256")
            {
                sb.Append(@"
        if(index >= "+index+@") offset += 32;");
            }
            else if (arg.TypeTerm.Identifier is "bytes" or "string")
            {
                sb.Append(@"
        if(index >= "+index+@") offset += BufferUtils.GetTLBytesLength(buffer, offset, length);");
            }
            else
            {
                sb.Append(@"
        if(index >= "+index+@") offset += "+arg.TypeTerm.GetFullyQualifiedIdentifier()+".ReadSize(buffer, length, offset);");
            }
            index++;
        }
        sb.Append(@"
        return offset;
    }");
    }
}