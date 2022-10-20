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

using Ferrite.TLParser;
using Xunit;

namespace Ferrite.Tests.TL;

public class TypeTermSyntaxTests
{
    [Theory]
    [InlineData("test#aabbccdd arg:Vector<TestType> arg2:flags.0?Vector<InputTestType> = Test;\n",0,"Vector")]
    [InlineData("test#aabbccdd arg:Vector<int> = Test;\n",0,"VectorOfInt")]
    [InlineData("test#aabbccdd arg:flags.0?Vector<InputTestType> = Test;\n",0,"Vector")]
    [InlineData("test#aabbccdd arg:vector<testns.TestType> = Test;\n",0,"VectorBare")]
    [InlineData("testns.test#aabbccdd arg:Vector<testns.TestType> = testns.Test;\n",0,"Vector")]
    [InlineData("test#aabbccdd arg:Vector<bytes> = Test;\n",0,"Vector")]
    public void TypeTermSyntax_Should_ReturnFullyQualifiedName(string tl, int argOffset, string name)
    {
        Lexer lexer = new Lexer(tl);
        Parser parser = new Parser(lexer);
        var c = parser.ParseCombinator();
        var actual = c.Arguments[argOffset].TypeTerm.GetFullyQualifiedIdentifier();
        Assert.Equal(name, actual);
    }
}