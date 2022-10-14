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
using Xunit.Abstractions;

namespace Ferrite.Tests.TL;

public class LexerTests
{
    [Fact]
    public void Lexer_Should_LexMTProtoSchema()
    {
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(File.ReadAllText("testdata/mtproto.tl"));
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }
        
        Assert.Equal(1014, tokens.Count);
    }
    [Fact]
    public void Lexer_Should_LexTLSchema()
    {
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(File.ReadAllText("testdata/schema.tl"));
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }
        
        Assert.Equal(24443, tokens.Count);
    }
    [Fact]
    public void Lexer_Should_LexComments()
    {
        List<Token> tokens = new List<Token>();
        string source = @"
//line comment 1
//line comment 2
//line comment 3

/*multi
line
comment 1*/

/*multi
line
comment 2*/
";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(14, tokens.Count);
        Assert.Equal("line comment 1", tokens[1].Value);
        Assert.Equal("multi\nline\ncomment 1", tokens[8].Value);
    }
    [Theory]
    [InlineData("(3+5)=8", 8)]
    [InlineData("((1/3)*0.5)<1/4", 16)]
    [InlineData(" -[]{}()=.:;?#%<>*`+", 21)]
    public void Lexer_Should_LexPunctuationsAndNumbers(string expression, int tokenCount)
    {
        List<Token> tokens = new List<Token>();
        
        Lexer lexer = new Lexer(expression);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(tokenCount, tokens.Count);
    }
    [Fact]
    public void Lexer_Should_LexFunctionsAndTypes()
    {
        List<Token> tokens = new List<Token>();
        string source = @"
---   functions   ---
---   types   ---
---functions---
---types---
";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(10, tokens.Count);
        Assert.Equal(TokenType.Functions, tokens[1].Type);
        Assert.Equal(TokenType.Types, tokens[3].Type);
        Assert.Equal(TokenType.Functions, tokens[5].Type);
        Assert.Equal(TokenType.Types, tokens[7].Type);
    }
    [Fact]
    public void Lexer_Should_LexHexConstants()
    {
        List<Token> tokens = new List<Token>();
        string source = @"23B972    3530AEAC EB548E10";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenType.HexConstant, tokens[0].Type);
        Assert.Equal(TokenType.HexConstant, tokens[2].Type);
        Assert.Equal(TokenType.HexConstant, tokens[4].Type);
    }
    [Fact]
    public void Lexer_Should_LexNamespaceIdentifier()
    {
        List<Token> tokens = new List<Token>();
        string source = @"test123.aaaaaa";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenType.NamespaceIdentifier, tokens[0].Type);
        Assert.Equal("test123", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.HexConstant, tokens[2].Type);
    }
    [Fact]
    public void Lexer_Should_LexCombinatorIdentifier()
    {
        List<Token> tokens = new List<Token>();
        string source = @"test123.testMethod1#aabbccdd";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenType.NamespaceIdentifier, tokens[0].Type);
        Assert.Equal("test123", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.CombinatorIdentifier, tokens[2].Type);
        Assert.Equal("testMethod1", tokens[2].Value);
        Assert.Equal(TokenType.Hash, tokens[3].Type);
        Assert.Equal(TokenType.HexConstant, tokens[4].Type);
    }
    [Fact]
    public void Lexer_ShouldNot_LexCombinatorIdentifier()
    {
        List<Token> tokens = new List<Token>();
        string source = @"int ? = Int;
resPQ#05162463";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }
        Assert.NotEqual(TokenType.CombinatorIdentifier, tokens[0].Type);
    }
    [Fact]
    public void Lexer_Should_LexVariableIdentifier()
    {
        List<Token> tokens = new List<Token>();
        string source = @"test123.testMethod1#aabbccdd testVar12:";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(9, tokens.Count);
        Assert.Equal(TokenType.NamespaceIdentifier, tokens[0].Type);
        Assert.Equal("test123", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.CombinatorIdentifier, tokens[2].Type);
        Assert.Equal("testMethod1", tokens[2].Value);
        Assert.Equal(TokenType.Hash, tokens[3].Type);
        Assert.Equal(TokenType.HexConstant, tokens[4].Type);
        Assert.Equal(TokenType.VariableIdentifier, tokens[6].Type);
        Assert.Equal("testVar12", tokens[6].Value);
    }
    [Fact]
    public void Lexer_Should_LexConditionalIdentifier()
    {
        List<Token> tokens = new List<Token>();
        string source = @"test123.testMethod1#aabbccdd testVar12:flags.2?";
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(13, tokens.Count);
        Assert.Equal(TokenType.NamespaceIdentifier, tokens[0].Type);
        Assert.Equal("test123", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.CombinatorIdentifier, tokens[2].Type);
        Assert.Equal("testMethod1", tokens[2].Value);
        Assert.Equal(TokenType.Hash, tokens[3].Type);
        Assert.Equal(TokenType.HexConstant, tokens[4].Type);
        Assert.Equal(TokenType.VariableIdentifier, tokens[6].Type);
        Assert.Equal("testVar12", tokens[6].Value);
        Assert.Equal(TokenType.Colon, tokens[7].Type);
        Assert.Equal(TokenType.ConditionalIdentifier, tokens[8].Type);
        Assert.Equal(TokenType.Dot, tokens[9].Type);
        Assert.Equal(TokenType.Number, tokens[10].Type);
        Assert.Equal(TokenType.QuestionMark, tokens[11].Type);
    }
    [Theory]
    [InlineData(@"test123.testMethod1#aabbccdd testVar12:flags.2?CustomType ")]
    [InlineData(@"test123.testMethod1#aabbccdd testVar12:flags.2?CustomType;")]
    public void Lexer_Should_LexTypeIdentifier(string source)
    {
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(15, tokens.Count);
        Assert.Equal(TokenType.NamespaceIdentifier, tokens[0].Type);
        Assert.Equal("test123", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.CombinatorIdentifier, tokens[2].Type);
        Assert.Equal("testMethod1", tokens[2].Value);
        Assert.Equal(TokenType.Hash, tokens[3].Type);
        Assert.Equal(TokenType.HexConstant, tokens[4].Type);
        Assert.Equal(TokenType.VariableIdentifier, tokens[6].Type);
        Assert.Equal("testVar12", tokens[6].Value);
        Assert.Equal(TokenType.Colon, tokens[7].Type);
        Assert.Equal(TokenType.ConditionalIdentifier, tokens[8].Type);
        Assert.Equal(TokenType.Dot, tokens[9].Type);
        Assert.Equal(TokenType.Number, tokens[10].Type);
        Assert.Equal(TokenType.QuestionMark, tokens[11].Type);
        Assert.Equal(TokenType.TypeIdentifier, tokens[12].Type);
        Assert.Equal("CustomType", tokens[12].Value);
    }
    [Theory]
    [InlineData(@"test123.testMethod1#aabbccdd testVar12:flags.2?customType ", new[]{12})]
    [InlineData(@"test123.testMethod1#aabbccdd testVar12:flags.2?customType<innerType>", new[]{12})]
    [InlineData(@"test123.testMethod1#aabbccdd testVar12:flags.2?customType<innerType>", new[]{12, 14})]
    public void Lexer_Should_LexBareTypeIdentifier(string source, int[] locations)
    {
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        foreach (var l in locations)
        {
            Assert.Equal(TokenType.BareTypeIdentifier, tokens[l].Type);
        }
    }
    [Theory]
    [InlineData(@"http_wait#9299359f max_delay:int wait_after:int max_wait:int = HttpWait;", 21)]
    [InlineData(@"resPQ#05162463 nonce:int128 server_nonce:int128 pq:bytes server_public_key_fingerprints:Vector<long> = ResPQ;", 28)]
    [InlineData(@"p_q_inner_data_dc#a9f55f95 pq:bytes p:bytes q:bytes nonce:int128 server_nonce:int128 new_nonce:int256 dc:int = P_Q_inner_data;
",38)]
    [InlineData(@"int ? = Int;
long ? = Long;
double ? = Double;
string ? = String;", 36)]
    [InlineData(@"vector {t:Type} # [ t ] = Vector t;
int128 4*[ int ] = Int128;", 38)]
    public void Lexer_Should_LexMTProtoTypes(string source, int count)
    {
        List<Token> tokens = new List<Token>();
        Lexer lexer = new Lexer(source);
        var token = lexer.Lex();
        tokens.Add(token);
        while (token.Type != TokenType.EOF)
        {
            token = lexer.Lex();
            tokens.Add(token);
        }

        Assert.Equal(count, tokens.Count);
    }
}