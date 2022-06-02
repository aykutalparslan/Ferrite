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
        
        Assert.Equal("inputMediaUploadedDocument", combinator.Identifier);
        Assert.Equal(9, combinator.Arguments.Count);
    }
}