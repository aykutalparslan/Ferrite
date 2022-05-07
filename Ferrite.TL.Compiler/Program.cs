/*
 *   Project Ferrite is an Implementation of the Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Globalization;
using Ferrite.TL.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

class Compiler
{
    static string CreateAbstractClass(string className, string namespaceName)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation of the Telegram Server API\r\n" +
                  " *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>\r\n" +
                  " *\r\n" +
                  " *   This program is free software: you can redistribute it and/or modify\r\n" +
                  " *   it under the terms of the GNU Affero General Public License as published by\r\n" +
                  " *   the Free Software Foundation, either version 3 of the License, or\r\n" +
                  " *   (at your option) any later version.\r\n" +
                  " *\r\n" +
                  " *   This program is distributed in the hope that it will be useful,\r\n" +
                  " *   but WITHOUT ANY WARRANTY; without even the implied warranty of\r\n" +
                  " *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\r\n" +
                  " *   GNU Affero General Public License for more details.\r\n" +
                  " *\r\n" +
                  " *   You should have received a copy of the GNU Affero General Public License\r\n" +
                  " *   along with this program.  If not, see <https://www.gnu.org/licenses/>.\r\n" +
                  " */\r\n")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.IO")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Ferrite.Utils"))
            )
            .AddMembers(SyntaxFactory.FileScopedNamespaceDeclaration(
                SyntaxFactory.ParseName("Ferrite.TL" + (namespaceName.Length > 0 ? "." + namespaceName.Replace("/", ".") : ""))));
        var cls = SyntaxFactory.ClassDeclaration(className.ToPascalCase())
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLObject")));

        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Constructor")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression("throw new NotImplementedException();")))
            );


        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("ReadOnlySequence<byte>"), "TLBytes")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression("throw new NotImplementedException();")))
            );
        var throwBlock = SyntaxFactory.ParseStatement("throw new NotImplementedException();");
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Parse")
                .WithParameterList(SyntaxFactory.ParseParameterList("(ref SequenceReader buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .WithBody(SyntaxFactory.Block(throwBlock)));

        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "WriteTo")
                .WithParameterList(SyntaxFactory.ParseParameterList("(Span<byte> buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .WithBody(SyntaxFactory.Block(throwBlock)));
        syntax = syntax.AddMembers(cls);
        var code = syntax
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }
    static string CreateTLObjectClass(TLConstructor constructor, string namespaceName)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation of the Telegram Server API\r\n" +
                  " *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>\r\n" +
                  " *\r\n" +
                  " *   This program is free software: you can redistribute it and/or modify\r\n" +
                  " *   it under the terms of the GNU Affero General Public License as published by\r\n" +
                  " *   the Free Software Foundation, either version 3 of the License, or\r\n" +
                  " *   (at your option) any later version.\r\n" +
                  " *\r\n" +
                  " *   This program is distributed in the hope that it will be useful,\r\n" +
                  " *   but WITHOUT ANY WARRANTY; without even the implied warranty of\r\n" +
                  " *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\r\n" +
                  " *   GNU Affero General Public License for more details.\r\n" +
                  " *\r\n" +
                  " *   You should have received a copy of the GNU Affero General Public License\r\n" +
                  " *   along with this program.  If not, see <https://www.gnu.org/licenses/>.\r\n" +
                  " */\r\n")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.IO")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Ferrite.Utils"))
            )
            .AddMembers(SyntaxFactory.FileScopedNamespaceDeclaration(
                SyntaxFactory.ParseName("Ferrite.TL" + (namespaceName.Length > 0 ? "." + namespaceName.Replace("/", ".") : ""))));

        string className = constructor.Predicate;
        if (className.Contains("."))
        {
            className = className.Split('.')[1];
        }
        className += "Impl";

        string baseClassName = constructor.Type;
        if (baseClassName.Contains("."))
        {
            baseClassName = baseClassName.Split('.')[1];
        }
        var cls = SyntaxFactory.ClassDeclaration(className.ToPascalCase())
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseClassName)));
        //cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLObject")));

        cls = AddField(cls, "SparseBufferWriter<byte>", "writer", "new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared)", true);
        cls = AddField(cls, "ITLObjectFactory", "factory", true);
        cls = AddField(cls, "bool", "serialized", "false");
        var constructorBlock = SyntaxFactory.ParseStatement("factory = objectFactory;");
        cls = cls.AddMembers(SyntaxFactory.ConstructorDeclaration(className.ToPascalCase())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(SyntaxFactory.ParseParameterList("(ITLObjectFactory objectFactory)"))
                .WithBody(SyntaxFactory.Block(constructorBlock)));

        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Constructor")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(constructor.Id + ";\r\n   ")))
            );

        var clearWriter = SyntaxFactory.ParseStatement("writer.Clear();");
        var setSerialized = SyntaxFactory.ParseStatement("serialized = true;");
        var returnBytes = SyntaxFactory.ParseStatement("return writer.ToReadOnlySequence();");
        var writeConstructor = SyntaxFactory.ParseStatement("writer.WriteInt32(Constructor, true);");
        var bytesBlock = SyntaxFactory.Block(
                       SyntaxFactory.IfStatement(SyntaxFactory.ParseExpression("serialized"), returnBytes),
                       clearWriter, writeConstructor);

        SerializeBlock(constructor, ref cls, setSerialized, returnBytes, ref bytesBlock);
        cls = FieldsBlock(constructor, cls);
        cls = ParseBlock(constructor, cls);

        var writeToBlock = SyntaxFactory.ParseStatement("TLBytes.CopyTo(buff);");
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "WriteTo")
                .WithParameterList(SyntaxFactory.ParseParameterList("(Span<byte> buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .WithBody(SyntaxFactory.Block(writeToBlock)));

        syntax = syntax.AddMembers(cls);

        var code = syntax
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }

    private static ClassDeclarationSyntax ParseBlock(TLConstructor constructor, ClassDeclarationSyntax cls)
    {
        var parseBlock = SyntaxFactory.Block(SyntaxFactory.ParseStatement("serialized  = false;"));
        foreach (var item in constructor.Params)
        {
            string typeName = GetTypeName(item);
            bool conditional = false;
            int bit = 0;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string prefix = conditional ? "if(_flags[" + bit + "]){" : "";
            string suffix = conditional ? "}" : "";
            if (typeName.StartsWith("Vector<"))
            {
                var b = typeName.Replace("Vector<", "").Replace(">", "");
                typeName = "Vector<" + b.ToPascalCase() + ">";
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                var b = typeName.Replace("VectorBare<", "").Replace("Vector<", "").Replace(">", "");
                typeName = "VectorBare<" + b.ToPascalCase() + ">";
            }

            if (typeName == "int")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt32(true);" + suffix)
                    );
            }
            else if (typeName == "bool")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = Bool.Read(ref buff); " + suffix)
                    );
            }
            else if (typeName == "True")
            {
                //use the flags
            }
            else if (typeName == "Flags")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.Read<Flags>();" + suffix)
                    );
            }
            else if (typeName == "long")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt64(true);" + suffix)
                    );
            }
            else if (typeName == "double")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt64(true);" + suffix)
                    );
            }
            else if (typeName == "string")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadTLString();" + suffix)
                    );
            }
            else if (typeName == "bytes")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadTLBytes().ToArray();" + suffix)
                    );
            }
            else if (typeName == "int128")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<Int128>(ref buff);" + suffix)
                    );
            }
            else if (typeName == "int256")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<Int256>(ref buff);" + suffix)
                    );
            }
            else if (typeName.StartsWith("vector<%"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + "factory.Read<" + item.Type.Replace("vector<%", "VectorBare<") + ">(ref buff);" + suffix)
                    );
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<" + item.Type + ">(ref buff);" + suffix)
                    );
            }
            else if (typeName == ("ITLObject"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read(buff.ReadInt32(true), ref  buff); " + suffix)
                    );
            }
            else if (typeName.StartsWith("Vector"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + " buff.Skip(4);" + item.Name.ToCamelCase() + " = factory.Read<"+typeName+">(ref  buff); " + suffix)
                    );
            }
            else
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = ("+typeName+")factory.Read(buff.ReadInt32(true), ref  buff); " + suffix)
                    );
            }
        }
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Parse")
                .WithParameterList(SyntaxFactory.ParseParameterList("(ref SequenceReader buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .WithBody(parseBlock));
        return cls;
    }

    private static ClassDeclarationSyntax FieldsBlock(TLConstructor constructor, ClassDeclarationSyntax cls)
    {
        foreach (var p in constructor.Params)
        {
            string typeName = GetTypeName(p);
            bool conditional = false;
            int bit = 0;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string setFlag = conditional ? " _flags[" + bit + "] = true;" : "";
            if (typeName.StartsWith("Vector<"))
            {
                var b = typeName.Replace("Vector<", "").Replace(">", "");
                typeName = "Vector<" + b.ToPascalCase() + ">";
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                var b = typeName.Replace("VectorBare<", "").Replace("Vector<", "").Replace(">", "");
                typeName = "VectorBare<" + b.ToPascalCase() + ">";
            }
            if (!typeName.Contains("Vector"))
            {
                typeName = typeName.Replace("bytes", "byte[]");
            }
            if (typeName == "True")
            {
                cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public bool " + p.Name.ToPascalCase() +
        "{get => _flags[" + bit + "]; set {serialized = false; _flags[" + bit + "] = value;}}"
                ));
            }
            else
            {
                cls = cls.AddMembers(SyntaxFactory
                .FieldDeclaration(SyntaxFactory
                    .VariableDeclaration(SyntaxFactory
                    .ParseTypeName(typeName))
                    .AddVariables(SyntaxFactory
                    .VariableDeclarator(p.Name.ToCamelCase())))
                .AddModifiers(SyntaxFactory
                    .Token(SyntaxKind.PrivateKeyword)));

                cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public " + typeName + " " + p.Name.ToPascalCase() +
        "{get => " + p.Name.ToCamelCase() + "; set {serialized = false; " + setFlag + p.Name.ToCamelCase() + " = value;}}"
                ));
            }

        }

        return cls;
    }

    private static void SerializeBlock(TLConstructor constructor, ref ClassDeclarationSyntax cls, StatementSyntax setSerialized, StatementSyntax returnBytes, ref BlockSyntax bytesBlock)
    {
        foreach (var item in constructor.Params)
        {
            bool conditional = false;
            int bit = 0;
            string typeName = item.Type;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string prefix = conditional ? "if(_flags[" + bit + "]){" : "";
            string suffix = conditional ? "}" : "";
            if (typeName == "int")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt32(" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "Bool")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt32(Bool.GetConstructor(" + item.Name.ToCamelCase() + "), true);" + suffix)
                    );
            }
            else if (typeName == "true")
            {
                //use the flags
            }
            else if (typeName == "#")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.Write<Flags>(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else if (typeName == "long")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "double")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt64((long)" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "string")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteTLString(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else if (typeName == "bytes")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteTLBytes(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.Write(" + item.Name.ToCamelCase() + ".TLBytes, false);" + suffix)
                    );
            }
        }
        bytesBlock = bytesBlock.AddStatements(setSerialized, returnBytes);
        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName("ReadOnlySequence<byte>"), "TLBytes")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
            .AddAccessorListAccessors(
               SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
               .WithBody(bytesBlock)));
    }

    static string CreateTLMethod(TLMethod constructor, string namespaceName, BlockSyntax? previousExecuteBody = null,
        SyntaxList<UsingDirectiveSyntax>? usings = null)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation of the Telegram Server API\r\n" +
                  " *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>\r\n" +
                  " *\r\n" +
                  " *   This program is free software: you can redistribute it and/or modify\r\n" +
                  " *   it under the terms of the GNU Affero General Public License as published by\r\n" +
                  " *   the Free Software Foundation, either version 3 of the License, or\r\n" +
                  " *   (at your option) any later version.\r\n" +
                  " *\r\n" +
                  " *   This program is distributed in the hope that it will be useful,\r\n" +
                  " *   but WITHOUT ANY WARRANTY; without even the implied warranty of\r\n" +
                  " *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\r\n" +
                  " *   GNU Affero General Public License for more details.\r\n" +
                  " *\r\n" +
                  " *   You should have received a copy of the GNU Affero General Public License\r\n" +
                  " *   along with this program.  If not, see <https://www.gnu.org/licenses/>.\r\n" +
                  " */\r\n")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.Buffers")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotNext.IO")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Ferrite.Utils"))

            )
            .AddMembers(SyntaxFactory.FileScopedNamespaceDeclaration(
                SyntaxFactory.ParseName("Ferrite.TL" + (namespaceName.Length > 0 ? "." + namespaceName.Replace("/", ".") : ""))));

        if (usings != null)
        {
            foreach (var item in usings)
            {
                string s = "" + item.Name;
                if (s != "System" &&
                    s != "System.Buffers" &&
                    s != "DotNext.Buffers" &&
                    s != "DotNext.IO" &&
                    s != "Ferrite.Utils")
                {
                    syntax = syntax.AddUsings(item);
                }
            }
        }

        string methodName = constructor.Method;
        if (methodName.Contains("."))
        {
            methodName = methodName.Split('.')[1];
        }
        methodName = methodName.ToPascalCase();

        var cls = SyntaxFactory.ClassDeclaration(methodName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLObject")));
        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLMethod")));


        cls = AddField(cls, "SparseBufferWriter<byte>", "writer", "new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared)",true);
        cls = AddField(cls, "ITLObjectFactory", "factory", true);
        cls = AddField(cls, "bool", "serialized", "false");
        var constructorBlock = SyntaxFactory.ParseStatement("factory = objectFactory;");
        cls = cls.AddMembers(SyntaxFactory.ConstructorDeclaration(methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(SyntaxFactory.ParseParameterList("(ITLObjectFactory objectFactory)"))
                .WithBody(SyntaxFactory.Block(constructorBlock)));

        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Constructor")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(constructor.Id + ";\r\n   ")))
            );

        var clearWriter = SyntaxFactory.ParseStatement("writer.Clear();");
        var setSerialized = SyntaxFactory.ParseStatement("serialized = true;");
        var returnBytes = SyntaxFactory.ParseStatement("return writer.ToReadOnlySequence();");
        var writeConstructor = SyntaxFactory.ParseStatement("writer.WriteInt32(Constructor, true);");
        var bytesBlock = SyntaxFactory.Block(
                       SyntaxFactory.IfStatement(SyntaxFactory.ParseExpression("serialized"), returnBytes),
                       clearWriter, writeConstructor);

        SerializeBlockMedhod(constructor, ref cls, setSerialized, returnBytes, ref bytesBlock);

        cls = FieldsBlockMethod(constructor, cls);

        var throwBlock = SyntaxFactory.ParseStatement("throw new NotImplementedException();");
        if (previousExecuteBody != null)
        {
            cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<ITLObject>"), "ExecuteAsync")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithParameterList(SyntaxFactory.ParseParameterList("(TLExecutionContext ctx)"))
                .WithBody(previousExecuteBody));
        }
        else
        {
            cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<ITLObject>"), "ExecuteAsync")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithParameterList(SyntaxFactory.ParseParameterList("(TLExecutionContext ctx)"))
                .WithBody(SyntaxFactory.Block(throwBlock)));
        }

        cls = ParseBlockMethod(constructor, cls);

        var writeToBlock = SyntaxFactory.ParseStatement("TLBytes.CopyTo(buff);");
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "WriteTo")
                .WithParameterList(SyntaxFactory.ParseParameterList("(Span<byte> buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(writeToBlock)));

        syntax = syntax.AddMembers(cls);
        var code = syntax
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }

    private static ClassDeclarationSyntax ParseBlockMethod(TLMethod constructor, ClassDeclarationSyntax cls)
    {
        var parseBlock = SyntaxFactory.Block(SyntaxFactory.ParseStatement("serialized  = false;"));
        foreach (var item in constructor.Params)
        {
            string typeName = GetTypeName(item);
            bool conditional = false;
            int bit = 0;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string prefix = conditional ? "if(_flags[" + bit + "]){" : "";
            string suffix = conditional ? "}" : "";
            if (typeName.StartsWith("Vector<"))
            {
                var b = typeName.Replace("Vector<", "").Replace(">", "");
                typeName = "Vector<" + b.ToPascalCase() + ">";
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                var b = typeName.Replace("VectorBare<", "").Replace("Vector<", "").Replace(">", "");
                typeName = "VectorBare<" + b.ToPascalCase() + ">";
            }

            if (typeName == "int")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt32(true);" + suffix)
                    );
            }
            else if (typeName == "bool")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = Bool.Read(ref buff); " + suffix)
                    );
            }
            else if (typeName == "True")
            {
                //use the flags
            }
            else if (typeName == "Flags")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.Read<Flags>();" + suffix)
                    );
            }
            else if (typeName == "long")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt64(true);" + suffix)
                    );
            }
            else if (typeName == "double")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadInt64(true);" + suffix)
                    );
            }
            else if (typeName == "string")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadTLString();" + suffix)
                    );
            }
            else if (typeName == "bytes")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = buff.ReadTLBytes().ToArray();" + suffix)
                    );
            }
            else if (typeName == "int128")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<Int128>(ref buff);" + suffix)
                    );
            }
            else if (typeName == "int256")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<Int256>(ref buff);" + suffix)
                    );
            }
            else if (typeName.StartsWith("vector<%"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + "factory.Read<" + item.Type.Replace("vector<%", "VectorBare<") + ">(ref buff);" + suffix)
                    );
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read<" + item.Type + ">(ref buff);" + suffix)
                    );
            }
            else if (typeName == ("ITLObject"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = factory.Read(buff.ReadInt32(true), ref  buff); " + suffix)
                    );
            }
            else if (typeName.StartsWith("Vector"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + " buff.Skip(4);" + item.Name.ToCamelCase() + " = factory.Read<" + typeName + ">(ref  buff); " + suffix)
                    );
            }
            else
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + item.Name.ToCamelCase() + " = (" + typeName + ")factory.Read(buff.ReadInt32(true), ref  buff); " + suffix)
                    );
            }
        }
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Parse")
                .WithParameterList(SyntaxFactory.ParseParameterList("(ref SequenceReader buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(parseBlock));
        return cls;
    }

    private static ClassDeclarationSyntax FieldsBlockMethod(TLMethod constructor, ClassDeclarationSyntax cls)
    {
        foreach (var p in constructor.Params)
        {
            string typeName = GetTypeName(p);
            bool conditional = false;
            int bit = 0;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string setFlag = conditional ? " _flags[" + bit + "] = true;" : "";
            if (typeName.StartsWith("Vector<"))
            {
                var b = typeName.Replace("Vector<", "").Replace(">", "");
                typeName = "Vector<" + b.ToPascalCase() + ">";
            }
            else if (typeName.StartsWith("VectorBare<"))
            {
                var b = typeName.Replace("VectorBare<", "").Replace("Vector<", "").Replace(">", "");
                typeName = "VectorBare<" + b.ToPascalCase() + ">";
            }
            if (!typeName.Contains("Vector"))
            {
                typeName = typeName.Replace("bytes", "byte[]");
            }
            if (typeName == "True")
            {
                cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public bool " + p.Name.ToPascalCase() +
        "{get => _flags[" + bit + "]; set {serialized = false; _flags[" + bit + "] = value;}}"
                ));
            }
            else
            {
                cls = cls.AddMembers(SyntaxFactory
                .FieldDeclaration(SyntaxFactory
                    .VariableDeclaration(SyntaxFactory
                    .ParseTypeName(typeName))
                    .AddVariables(SyntaxFactory
                    .VariableDeclarator(p.Name.ToCamelCase())))
                .AddModifiers(SyntaxFactory
                    .Token(SyntaxKind.PrivateKeyword)));

                cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public " + typeName + " " + p.Name.ToPascalCase() +
        "{get => " + p.Name.ToCamelCase() + "; set {serialized = false; " + setFlag + p.Name.ToCamelCase() + " = value;}}"
                ));
            }

        }

        return cls;
    }

    private static void SerializeBlockMedhod(TLMethod constructor, ref ClassDeclarationSyntax cls, StatementSyntax setSerialized, StatementSyntax returnBytes, ref BlockSyntax bytesBlock)
    {
        foreach (var item in constructor.Params)
        {
            bool conditional = false;
            int bit = 0;
            string typeName = item.Type;
            if (typeName.StartsWith("flags."))
            {
                string[] tmp = typeName.Split('?');
                typeName = tmp[1];
                conditional = true;
                bit = int.Parse(tmp[0].Split('.')[1]);
            }
            string prefix = conditional ? "if(_flags[" + bit + "]){" : "";
            string suffix = conditional ? "}" : "";
            if (typeName == "int")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt32(" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "Bool")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt32(Bool.GetConstructor(" + item.Name.ToCamelCase() + "), true);" + suffix)
                    );
            }
            else if (typeName == "true")
            {
                //use the flags
            }
            else if (typeName == "#")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.Write<Flags>(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else if (typeName == "long")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "double")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteInt64((long)" + item.Name.ToCamelCase() + ", true);" + suffix)
                    );
            }
            else if (typeName == "string")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteTLString(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else if (typeName == "bytes")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.WriteTLBytes(" + item.Name.ToCamelCase() + ");" + suffix)
                    );
            }
            else
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement(prefix + "writer.Write(" + item.Name.ToCamelCase() + ".TLBytes, false);" + suffix)
                    );
            }
        }
        bytesBlock = bytesBlock.AddStatements(setSerialized, returnBytes);
        cls = cls.AddMembers(SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName("ReadOnlySequence<byte>"), "TLBytes")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
               SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
               .WithBody(bytesBlock)));
    }

    private static string GetTypeName(TLParam item)
    {
        if (item.Type.Contains("JSONObjectValue"))
        {
            return item.Type;
        }
        return item.Type.Replace("#", "Flags")
                        .Replace("true", "True")
                        .Replace("Bool", "bool")
                        .Replace("int128", "Int128")
                        .Replace("int256", "Int256")
                        .Replace("Object", "ITLObject")
                        .Replace("!X", "ITLObject")
                        .Replace("Vector<int>", "VectorOfInt")
                        .Replace("Vector<long>", "VectorOfLong")
                        .Replace("Vector<double>", "VectorOfDouble")
                        .Replace("Vector<string>", "VectorOfString")
                        .Replace("Vector<bytes>", "VectorOfBytes")
                        .Replace("vector<%", "VectorBare<")
                        .Replace("vector<", "VectorBare<");
    }

    private static ClassDeclarationSyntax AddField(ClassDeclarationSyntax cls, string typeName,
        string fieldname, string fieldValue, bool readOnly = false)
    {
        var field =SyntaxFactory
                        .FieldDeclaration(SyntaxFactory
                            .VariableDeclaration(SyntaxFactory
                            .ParseTypeName(typeName))
                            .AddVariables(SyntaxFactory
                            .VariableDeclarator(fieldname).WithInitializer(
                                SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(fieldValue)))))
                        .AddModifiers(SyntaxFactory
                            .Token(SyntaxKind.PrivateKeyword));
        if (readOnly)
        {
            field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }
        cls = cls.AddMembers(field);
        return cls;
    }

    private static ClassDeclarationSyntax AddField(ClassDeclarationSyntax cls,
        string typeName, string fieldname, bool readOnly=false)
    {
        var field = SyntaxFactory
                        .FieldDeclaration(SyntaxFactory
                            .VariableDeclaration(SyntaxFactory
                            .ParseTypeName(typeName))
                            .AddVariables(SyntaxFactory
                            .VariableDeclarator(fieldname)))
                        .AddModifiers(SyntaxFactory
                            .Token(SyntaxKind.PrivateKeyword));
        if (readOnly)
        {
            field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }
        cls = cls.AddMembers(field);
        return cls;
    }

    public static string GenerateTLConstructorConstants(TLSchema schema, string namespaceName)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation of the Telegram Server API\r\n" +
                  " *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>\r\n" +
                  " *\r\n" +
                  " *   This program is free software: you can redistribute it and/or modify\r\n" +
                  " *   it under the terms of the GNU Affero General Public License as published by\r\n" +
                  " *   the Free Software Foundation, either version 3 of the License, or\r\n" +
                  " *   (at your option) any later version.\r\n" +
                  " *\r\n" +
                  " *   This program is distributed in the hope that it will be useful,\r\n" +
                  " *   but WITHOUT ANY WARRANTY; without even the implied warranty of\r\n" +
                  " *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\r\n" +
                  " *   GNU Affero General Public License for more details.\r\n" +
                  " *\r\n" +
                  " *   You should have received a copy of the GNU Affero General Public License\r\n" +
                  " *   along with this program.  If not, see <https://www.gnu.org/licenses/>.\r\n" +
                  " */\r\n"))
            )
            .AddMembers(SyntaxFactory.FileScopedNamespaceDeclaration(
                SyntaxFactory.ParseName("Ferrite.TL"+(namespaceName.Length>0?"."+namespaceName:""))));

        var cls = SyntaxFactory.ClassDeclaration("TLConstructor")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        foreach (var item in schema.Constructors)
        {
            string[] n = item.Predicate.Split('.');
            string fieldname = item.Predicate.ToPascalCase();
            if (n.Length > 1)
            {
                fieldname = n[0].ToPascalCase()+"_" +n[1].ToPascalCase();
            }
            var field = SyntaxFactory
                        .FieldDeclaration(SyntaxFactory
                            .VariableDeclaration(SyntaxFactory
                            .ParseTypeName("int"))
                            .AddVariables(SyntaxFactory
                            .VariableDeclarator(fieldname).WithInitializer(
                                SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(item.Id)))))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.ConstKeyword));
            cls = cls.AddMembers(field);
        }
        foreach (var item in schema.Methods)
        {
            string[] n = item.Method.Split('.');
            string fieldname = item.Method.ToPascalCase();
            if (n.Length > 1)
            {
                fieldname = n[0].ToPascalCase() + "_" + n[1].ToPascalCase();
            }
            var field = SyntaxFactory
                        .FieldDeclaration(SyntaxFactory
                            .VariableDeclaration(SyntaxFactory
                            .ParseTypeName("int"))
                            .AddVariables(SyntaxFactory
                            .VariableDeclarator(fieldname).WithInitializer(
                                SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(item.Id)))))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
            cls = cls.AddMembers(field);
        }
        syntax = syntax.AddMembers(cls);
        var code = syntax
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }

    static void Main()
    {
        var mtProtoSchema = TLSchema.Load("mtproto.json");
        var tlLayer139Schema = TLSchema.Load("schema.L139.json");
        var tlSchemaForMissingTypes = TLSchema.Load("schema.missing.json");
        if (!Directory.Exists("../../../../Ferrite.TL/mtproto/"))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/mtproto/");
        }
        foreach (var item in mtProtoSchema.Constructors)
        {
            if (item.Predicate == "vector" ||
                item.Predicate == "boolTrue" ||
                item.Predicate == "boolFalse" ||
                item.Predicate == "error" ||
                item.Predicate == "true" ||
                item.Predicate == "null")
            {
                continue;
            }
            string fileName = item.Predicate.ToPascalCase();
 
            if (!File.Exists("../../../../Ferrite.TL/mtproto/" + item.Predicate.ToPascalCase() + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/mtproto/" + fileName + ".cs", false))
                {
                    writer.Write(CreateTLObjectClass(item, "mtproto"));
                }
            }
        }
        foreach (var item in tlLayer139Schema.Constructors)
        {
            if (item.Predicate == "vector" ||
                item.Predicate == "boolTrue" ||
                item.Predicate == "boolFalse" ||
                item.Predicate == "error" ||
                item.Predicate == "true" ||
                item.Predicate == "null")
            {
                continue;
            }

            string[] pre = item.Predicate.Split('.');
            string fileName = item.Predicate.FirstLetterToUpperCase();
            string nameSpaceName = "layer139";
            if (pre.Length > 1)
            {
                fileName = pre[1].FirstLetterToUpperCase();
                nameSpaceName += "/"+pre[0];
            }
            fileName += "Impl";
            
            if (!Directory.Exists("../../../../Ferrite.TL/" + nameSpaceName))
            {
                Directory.CreateDirectory("../../../../Ferrite.TL/" + nameSpaceName);
            }
            string[] preBase = item.Type.Split('.');
            string fileNameBase = item.Type;
            if (preBase.Length > 1)
            {
                fileNameBase = preBase[1];
            }
            string baseClass = CreateAbstractClass(fileNameBase, nameSpaceName);
            if (!File.Exists("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileNameBase + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileNameBase + ".cs", false))
                {
                    writer.Write(baseClass);
                }
            }
            if (!File.Exists("../../../../Ferrite.TL/"+nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileName + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/" + nameSpaceName +(nameSpaceName.Length>0?"/":"") + fileName + ".cs", false))
                {
                    writer.Write(CreateTLObjectClass(item, nameSpaceName));
                }
            }
        }
        foreach (var item in mtProtoSchema.Methods)
        {
            if (File.Exists("../../../../Ferrite.TL/mtproto/" + item.Method.ToPascalCase() + ".cs"))
            {/*
                StreamReader sr = new StreamReader("../../../../Ferrite.TL/mtproto/" + item.Method.ToPascalCase() + ".cs");
                SyntaxTree tree = CSharpSyntaxTree.ParseText(sr.ReadToEnd());
                IEnumerable<MethodDeclarationSyntax> methods = tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>().ToList();

                var root = (CompilationUnitSyntax)tree.GetRoot();

                foreach (var method in methods)
                {
                    if (method.Identifier.Text == "Execute")
                    {
                        using (var writer = new StreamWriter("../../../../Ferrite.TL/mtproto/" + item.Method.ToPascalCase() + ".cs", false))
                        {
                            writer.Write(CreateTLMethod(item, "mtproto", method.Body, root.Usings));
                        }
                    }
                }*/
            }
            else
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/mtproto/" + item.Method.ToPascalCase() + ".cs", false))
                {
                    writer.Write(CreateTLMethod(item, "mtproto"));
                }
            }
        }

        foreach (var item in tlLayer139Schema.Methods)
        {
            string[] pre = item.Method.Split('.');
            string fileName = item.Method.FirstLetterToUpperCase();
            string nameSpaceName = "layer139";
            if (pre.Length > 1)
            {
                fileName = pre[1].FirstLetterToUpperCase();
                nameSpaceName += "/" + pre[0];
            }
            if (!Directory.Exists("../../../../Ferrite.TL/" + nameSpaceName))
            {
                Directory.CreateDirectory("../../../../Ferrite.TL/" + nameSpaceName);
            }
            if (!File.Exists("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileName + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileName + ".cs", false))
                {
                    writer.Write(CreateTLMethod(item, nameSpaceName));
                }
            }
        }

        foreach (var item in tlSchemaForMissingTypes.Methods)
        {
            string[] pre = item.Method.Split('.');
            string fileName = item.Method.FirstLetterToUpperCase();
            string nameSpaceName = "";
            if (pre.Length > 1)
            {
                fileName = pre[1].FirstLetterToUpperCase();
                nameSpaceName += "/" + pre[0];
            }
            if (!Directory.Exists("../../../../Ferrite.TL/" + nameSpaceName))
            {
                Directory.CreateDirectory("../../../../Ferrite.TL/" + nameSpaceName);
            }
            if (!File.Exists("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileName + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/" + nameSpaceName + (nameSpaceName.Length > 0 ? "/" : "") + fileName + ".cs", false))
                {
                    writer.Write(CreateTLMethod(item, nameSpaceName));
                }
            }
        }

        if (File.Exists("../../../../Ferrite.TL/TLObjectFactory.cs"))
        {
            StreamReader sr = new StreamReader("../../../../Ferrite.TL/TLObjectFactory.cs");
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sr.ReadToEnd());
            var root = (CompilationUnitSyntax)tree.GetRoot();
            IEnumerable<MethodDeclarationSyntax> methods = root
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>().ToList();
            
            foreach (var method in methods)
            {
                if (method.Identifier.Text == "Read" && method.ReturnType.GetText().ToString().Contains("ITLObject"))
                {
                    SwitchExpressionSyntax oldSyntax = (SwitchExpressionSyntax)method.ExpressionBody.Expression;
                    SeparatedSyntaxList<SwitchExpressionArmSyntax> arms = new SeparatedSyntaxList<SwitchExpressionArmSyntax>();
                    foreach (var item in mtProtoSchema.Constructors)
                    {
                        if(item.Predicate == "vector")
                        {
                            continue;
                        }
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<"+item.Predicate.ToPascalCase()+">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab,SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    foreach (var item in mtProtoSchema.Methods)
                    {
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<" + item.Method.ToPascalCase() + ">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    foreach (var item in tlLayer139Schema.Constructors)
                    {
                        if (item.Predicate == "vector" ||
                            item.Predicate == "boolTrue" ||
                            item.Predicate == "boolFalse" ||
                            item.Predicate == "error" ||
                            item.Predicate == "true" ||
                            item.Predicate == "null")
                        {
                            continue;
                        }
                        string[] arr = item.Predicate.Split('.');
                        string fileName = item.Predicate.ToPascalCase();
                        string nameSpaceName = "layer139";
                        if (arr.Length > 1)
                        {
                            fileName = arr[1].ToPascalCase();
                            nameSpaceName += "." + arr[0];
                        }
                        fileName += "Impl";
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<"+nameSpaceName+"." + fileName + ">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    foreach (var item in tlLayer139Schema.Methods)
                    {
                        string[] arr = item.Method.Split('.');
                        string fileName = item.Method.FirstLetterToUpperCase();
                        string nameSpaceName = "layer139";
                        if (arr.Length > 1)
                        {
                            fileName = arr[1].FirstLetterToUpperCase();
                            nameSpaceName += "." + arr[0];
                        }
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<" + nameSpaceName + "." + fileName + ">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    foreach (var item in tlSchemaForMissingTypes.Methods)
                    {
                        string[] arr = item.Method.Split('.');
                        string fileName = item.Method.FirstLetterToUpperCase();
                        string nameSpaceName = "";
                        if (arr.Length > 1)
                        {
                            fileName = arr[1].FirstLetterToUpperCase();
                            nameSpaceName += "." + arr[0];
                        }
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<" + nameSpaceName + (nameSpaceName.Length>0?".":"") + fileName + ">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression("_")),
                            SyntaxFactory.ParseExpression("throw new DeserializationException(\"Constructor \"+ string.Format(\"0x{0:X}\", constructor) + \" not found.\")"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    SwitchExpressionSyntax newSyntax = oldSyntax.WithArms(arms).NormalizeWhitespace();
                    root = root.ReplaceNode(oldSyntax, newSyntax);
                }
                
            }
            using (var writer = new StreamWriter("../../../../Ferrite.TL/TLObjectFactory.cs", false))
            {
                writer.Write(root.NormalizeWhitespace().ToFullString());
            }

            using (var writer = new StreamWriter("../../../../Ferrite.TL/TLConstructor.cs", false))
            {
                writer.Write(GenerateTLConstructorConstants(mtProtoSchema, ""));
            }
            using (var writer = new StreamWriter("../../../../Ferrite.TL/layer139/TLConstructor.cs", false))
            {
                writer.Write(GenerateTLConstructorConstants(tlLayer139Schema, "layer139"));
            }
        }
    }
}



