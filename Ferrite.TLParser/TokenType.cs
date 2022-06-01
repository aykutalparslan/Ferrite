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

namespace Ferrite.TLParser
{
    public enum TokenType
    {
        HexConstant,
        LowercaseIdentifier,
        TypeIdentifier,
        Equal,
        EOL,
        Spaces,
        Dot,
        Semicolon,
        QuestionMark,
        ExclamationMark,
        Hash,
        Percent,
        Langle,
        Rangle,
        Colon,
        OpenBracket,
        CloseBracket,
        OpenBrace,
        CloseBrace,
        Functions,
        Types,
        EOF,
        None,
        Number,
        Multiply,
        LineComment,
        MultilineComment,
        NamespaceIdentifier,
        CombinatorIdentifier,
        VariableIdentifier,
        TypeTerm,
        ConditionalIdentifier,
        BareTypeIdentifier,
        OpenParen,
        CloseParen,
        BackTick,
        Plus,
        Minus
    }
}