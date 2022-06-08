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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ferrite.TLParser
{
    //https://github.com/dotnet/roslyn-sdk/blob/main/samples/CSharp/SourceGenerators/SourceGeneratorSamples/MathsGenerator.cs
    public class Lexer
    {
        private readonly (TokenType, string)[] _tokenStrings =
        {
            (TokenType.EOL, @"[\r\n|\r|\n]"),
            (TokenType.LineComment, @"\/\/[^\n\r]+?(?:\*\)|[\n\r])"), //https://stackoverflow.com/a/49180687/2015348
            (TokenType.MultilineComment, @"(?s)/\*.*?\*/"), //https://stackoverflow.com/a/36328890/2015348
            (TokenType.Functions, @"(---\s?functions\s?---)"),
            (TokenType.Types, @"(---\s?types\s?---)"),
            (TokenType.Spaces, @"\s+"),
            (TokenType.OpenBracket, @"\["),
            (TokenType.CloseBracket, @"\]"),
            (TokenType.OpenBrace, @"\{"),
            (TokenType.CloseBrace, @"\}"),
            (TokenType.OpenParen, @"\("),
            (TokenType.CloseParen, @"\)"),
            (TokenType.Equal, @"\="),
            (TokenType.Dot, @"\."),
            (TokenType.Colon, @"\:"),
            (TokenType.Semicolon, @"\;"),
            (TokenType.QuestionMark, @"\?"),
            (TokenType.ExclamationMark, @"\!"),
            (TokenType.Hash, @"\#"),
            (TokenType.Percent, @"\%"),
            (TokenType.Langle, @"\<"),
            (TokenType.Rangle, @"\>"),
            (TokenType.Multiply, @"\*"),
            (TokenType.BackTick, @"\`"),
            (TokenType.Plus, @"\+"),
            (TokenType.Minus, @"\-"),
            (TokenType.HexConstant, @"[a-fA-F0-9]{6,8}"),
            (TokenType.NamespaceIdentifier, @"[a-z][_a-zA-Z0-9]*(?=\.[a-zA-Z])"),
            (TokenType.CombinatorIdentifier, @"[a-z][_a-zA-Z0-9]+(?=([#]+[a-f0-9]+)+\s)"),
            (TokenType.VariableIdentifier, @"[a-z][_a-zA-Z0-9]*(?=\:)"),
            (TokenType.ConditionalIdentifier, @"[a-z][_a-zA-Z0-9]*(?=[\.]{1}[0-9]+[\?]{1})"),
            (TokenType.TypeIdentifier, @"[A-Z][_a-zA-Z0-9]*"),
            (TokenType.BareTypeIdentifier, @"[a-z][_a-zA-Z0-9]+(?=[\s\<\>])(?![\:])"),
            (TokenType.LowercaseIdentifier, @"[a-z][_a-zA-Z0-9]*"),
            (TokenType.Number, @"[0-9]+"),

            //
            //(TokenType.CombinatorIdentifier,  @"^[a-z][_a-zA-Z0-9]+([#]+[a-f0-9]+)?(?=\s)"),
            //(TokenType.SimpleArgument,  @"^[a-z][_a-z0-9]+[:]{1}[a-zA-Z0-9]+([<]{1}[\%]?[a-zA-Z0-9]+[>]{1})?(?=\s)"),
        };

        private readonly IEnumerable<(TokenType, Regex)> _tokenExpressions;
        private readonly StringReader _sr;
        private int _currentLine;
        private int _currentColumn;
        private string _current = "";

        public Lexer(string source)
        {
            _tokenExpressions =
                _tokenStrings.Select(
                    t => (t.Item1, new Regex($"^{t.Item2}",
                        RegexOptions.Compiled | RegexOptions.Singleline)));
            _sr = new StringReader(source);
            _currentLine = 0;
            _currentColumn = 0;
        }

        public Token Lex()
        {
            if (_current.Length == 0)
            {
                _current = _sr.ReadLine();
                if (_current == null)
                {
                    return new Token
                    {
                        Type = TokenType.EOF,
                    };
                }
                _current += "\n";
            }

            var matchLength = 0;
            var tokenType = TokenType.None;
            string value = null;

            foreach (var (type, rule) in _tokenExpressions)
            {
                var match = rule.Match(_current);
                if (match.Success)
                {
                    matchLength = match.Length;
                    tokenType = type;
                    value = match.Value;
                    break;
                }
            }

            if (matchLength == 0)
            {

                throw new Exception($"Unrecognized symbol");
            }

            _currentColumn += matchLength;
            if (tokenType == TokenType.EOL)
            {
                _currentLine += 1;
                _currentColumn = 0;
            }

            _current = _current.Substring(matchLength);

            if (tokenType != TokenType.Spaces &&
                tokenType != TokenType.LineComment &&
                tokenType != TokenType.MultilineComment &&
                tokenType != TokenType.Number &&
                tokenType != TokenType.HexConstant &&
                tokenType != TokenType.LowercaseIdentifier &&
                tokenType != TokenType.TypeIdentifier &&
                tokenType != TokenType.CombinatorIdentifier &&
                tokenType != TokenType.VariableIdentifier &&
                tokenType != TokenType.ConditionalIdentifier &&
                tokenType != TokenType.BareTypeIdentifier &&
                tokenType != TokenType.NamespaceIdentifier)
            {
                return new Token
                {
                    Type = tokenType,
                    Line = _currentLine,
                    Column = _currentColumn
                };
            }

            return new Token
            {
                Type = tokenType,
                Value = value,
                Line = _currentLine,
                Column = _currentColumn
            };
        }
    }
}