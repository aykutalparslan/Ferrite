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

using System.Collections.Generic;
using System.IO;
using Ferrite.TLParser;
using Xunit;

namespace Ferrite.Tests.TL;

public class ParserTests
{
    [Fact]
    public void Parser_Should_ParseCombinator()
    {
        Lexer lexer = new Lexer(
            @"inputMediaUploadedDocument#5b38c6c1 flags:# nosound_video:flags.3?true force_file:flags.4?true file:InputFile thumb:flags.2?InputFile mime_type:string attributes:Vector<DocumentAttribute> stickers:flags.0?Vector<InputDocument> ttl_seconds:flags.1?int = InputMedia;
");
        Parser parser = new Parser(lexer);
        var combinator = parser.ParseCombinator();
        
        Assert.Equal(CombinatorType.Constructor, combinator.CombinatorType);
        Assert.Equal("inputMediaUploadedDocument", combinator.Identifier);
        Assert.Equal(9, combinator.Arguments.Count);
    }
    [Fact]
    public void Parser_ShouldParse_DcOption()
    {
        Lexer lexer = new Lexer(
            @"dcOption#18b7a10d flags:# ipv6:flags.0?true media_only:flags.1?true tcpo_only:flags.2?true cdn:flags.3?true static:flags.4?true this_port_only:flags.5?true id:int ip_address:string port:int secret:flags.10?bytes = DcOption;
");
        Parser parser = new Parser(lexer);
        var combinator = parser.ParseCombinator();
        
        Assert.Equal(CombinatorType.Constructor, combinator.CombinatorType);
        Assert.Equal("dcOption", combinator.Identifier);
        Assert.Equal(11, combinator.Arguments.Count);
    }
    
    [Fact]
    public void Parser_Should_ParseFunction()
    {
        Lexer lexer = new Lexer(
            @"
---functions---
account.getAllSecureValues#b288bc7d = Vector<SecureValue>;
");
        Parser parser = new Parser(lexer);
        var combinator = parser.ParseCombinator();
        
        Assert.Equal(CombinatorType.Function, combinator.CombinatorType);
        Assert.Equal("getAllSecureValues", combinator.Identifier);
        Assert.Equal("Vector", combinator.Type.Identifier);
        Assert.Equal("SecureValue", combinator.Type.OptionalType.Identifier);
        Assert.Equal(0, combinator.Arguments.Count);
    }
}