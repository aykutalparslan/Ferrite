/*
 *   Project Ferrite is an Implementation Telegram Server API
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
    static string CreateTLObjectClass(TLConstructor constructor, string namespaceName)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation Telegram Server API\r\n" +
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
                SyntaxFactory.ParseName("Ferrite.TL" + (namespaceName.Length > 0 ? "." + namespaceName : ""))));

        var cls = SyntaxFactory.ClassDeclaration(constructor.Predicate.ToPascalCase())
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLObject")));

        cls = AddField(cls, "SparseBufferWriter<byte>", "writer", "new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared)",true);
        cls = AddField(cls, "ITLObjectFactory", "factory", true);
        cls = AddField(cls, "bool", "serialized", "false");
        var constructorBlock = SyntaxFactory.ParseStatement("factory = objectFactory;");
        cls = cls.AddMembers(SyntaxFactory.ConstructorDeclaration(constructor.Predicate.ToPascalCase())
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
        foreach (var item in constructor.Params)
        {
            if (item.Type == "int")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt32(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "long")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "double")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "string")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteTLString(" + item.Name.ToCamelCase() + ");")
                    );
            }
            else if (item.Type == "bytes")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteTLBytes(" + item.Name.ToCamelCase() + ");")
                    );
            }
            else
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.Write(" + item.Name.ToCamelCase() + ".TLBytes, false);")
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


        foreach (var p in constructor.Params)
        {
            string typeName = GetTypeName(p);
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

            cls = cls.AddMembers(SyntaxFactory
                .FieldDeclaration(SyntaxFactory
                    .VariableDeclaration(SyntaxFactory
                    .ParseTypeName(typeName))
                    .AddVariables(SyntaxFactory
                    .VariableDeclarator(p.Name.ToCamelCase())))
                .AddModifiers(SyntaxFactory
                    .Token(SyntaxKind.PrivateKeyword)));

            cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public " + typeName + " " + p.Name.ToPascalCase() +
    "{get => " + p.Name.ToCamelCase() + "; set {serialized = false; " + p.Name.ToCamelCase() + " = value;}}"
            ));
        }

        
        var parseBlock = SyntaxFactory.Block(SyntaxFactory.ParseStatement("serialized  = false;"));
        foreach (var item in constructor.Params)
        {
            string typeName = GetTypeName(item);
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

            if (item.Type == "int")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt32(true);")
                    );
            }
            else if (item.Type == "long")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt64(true);")
                    );
            }
            else if (item.Type == "double")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt64(true);")
                    );
            }
            else if (item.Type == "string")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadTLString();")
                    );
            }
            else if (item.Type == "bytes")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadTLBytes().ToArray();")
                    );
            }
            else if (item.Type == "int128")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<Int128>(ref buff);")
                    );
            }
            else if (item.Type == "int256")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<Int256>(ref buff);")
                    );
            }
            else if (item.Type.StartsWith("vector<%"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement("factory.Read<" + item.Type.Replace("vector<%", "VectorBare<") + ">(ref buff);")
                    );
            }
            else if (item.Type.StartsWith("VectorBare<"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<" + item.Type + ">(ref buff);")
                    );
            }
            else if (item.Type == ("Object"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read(buff.ReadInt32(true), ref  buff); ")
                    );
            }
            else
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement("buff.Skip(4); " + item.Name.ToCamelCase() + " = factory.Read<" + typeName + ">(ref buff);")
                    );
            }
        }
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Parse")
                .WithParameterList(SyntaxFactory.ParseParameterList("(ref SequenceReader buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(parseBlock));

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
    static string CreateTLMethod(TLMethod constructor, string namespaceName, BlockSyntax? previousExecuteBody = null,
        SyntaxList<UsingDirectiveSyntax>? usings = null)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation Telegram Server API\r\n" +
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
                SyntaxFactory.ParseName("Ferrite.TL" + (namespaceName.Length > 0 ? "." + namespaceName : ""))));
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
        
        
        var cls = SyntaxFactory.ClassDeclaration(constructor.Method.ToPascalCase())
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLObject")));
        cls = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ITLMethod")));


        cls = AddField(cls, "SparseBufferWriter<byte>", "writer", "new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared)",true);
        cls = AddField(cls, "ITLObjectFactory", "factory", true);
        cls = AddField(cls, "bool", "serialized", "false");
        var constructorBlock = SyntaxFactory.ParseStatement("factory = objectFactory;");
        cls = cls.AddMembers(SyntaxFactory.ConstructorDeclaration(constructor.Method.ToPascalCase())
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
        foreach (var item in constructor.Params)
        {
            if (item.Type == "int")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt32(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "long")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "double")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteInt64(" + item.Name.ToCamelCase() + ", true);")
                    );
            }
            else if (item.Type == "string")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteTLString(" + item.Name.ToCamelCase() + ");")
                    );
            }
            else if (item.Type == "bytes")
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.WriteTLBytes(" + item.Name.ToCamelCase() + ");")
                    );
            }
            else
            {
                bytesBlock = bytesBlock.AddStatements(
                    SyntaxFactory.ParseStatement("writer.Write(" + item.Name.ToCamelCase() + ".TLBytes, false);")
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


        foreach (var p in constructor.Params)
        {
            string typeName = GetTypeName(p);

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

            cls = cls.AddMembers(SyntaxFactory
                .FieldDeclaration(SyntaxFactory
                    .VariableDeclaration(SyntaxFactory
                    .ParseTypeName(typeName))
                    .AddVariables(SyntaxFactory
                    .VariableDeclarator(p.Name.ToCamelCase())))
                .AddModifiers(SyntaxFactory
                    .Token(SyntaxKind.PrivateKeyword)));

            cls = cls.AddMembers(SyntaxFactory.ParseMemberDeclaration("public " + typeName + " " + p.Name.ToPascalCase() +
    "{get => " + p.Name.ToCamelCase() + "; set {serialized = false; " + p.Name.ToCamelCase() + " = value;}}"
            ));
        }

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


        var parseBlock = SyntaxFactory.Block(SyntaxFactory.ParseStatement("serialized  = false;"));
        foreach (var item in constructor.Params)
        {
            string typeName = GetTypeName(item);
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

            if (item.Type == "int")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt32(true);")
                    );
            }
            else if (item.Type == "long")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt64(true);")
                    );
            }
            else if (item.Type == "double")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadInt64(true);")
                    );
            }
            else if (item.Type == "string")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadTLString();")
                    );
            }
            else if (item.Type == "bytes")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = buff.ReadTLBytes().ToArray();")
                    );
            }
            else if (item.Type == "int128")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<Int128>(ref buff);")
                    );
            }
            else if (item.Type == "int256")
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<Int256>(ref buff);")
                    );
            }
            else if (item.Type.StartsWith("vector<%"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<" + item.Type.Replace("vector<%", "VectorBare<") + ">(ref buff);")
                    );
            }
            else if (item.Type.StartsWith("VectorBare<"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read<" + item.Type + ">(ref buff);")
                    );
            }
            else if (item.Type == ("Object"))
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement(item.Name.ToCamelCase() + " = factory.Read(buff.ReadInt32(true), ref  buff); ")
                    );
            }
            else
            {
                parseBlock = parseBlock.AddStatements(
                    SyntaxFactory.ParseStatement("buff.Skip(4); " + item.Name.ToCamelCase() + " = factory.Read<" + typeName + ">(ref buff);")
                    );
            }
        }
        cls = cls.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Parse")
                .WithParameterList(SyntaxFactory.ParseParameterList("(ref SequenceReader buff)"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(parseBlock));

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

    private static string GetTypeName(TLParam item)
    {
        return item.Type.Replace("bytes", "byte[]")
                        .Replace("int128", "Int128")
                        .Replace("int256", "Int256")
                        .Replace("Object", "ITLObject")
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

    public static string GenerateTLConstructorEnum(TLSchema schema)
    {
        var syntax = SyntaxFactory.CompilationUnit().AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
                .WithLeadingTrivia(SyntaxFactory.Comment("/*\r\n" +
                  " *   Project Ferrite is an Implementation Telegram Server API\r\n" +
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
                SyntaxFactory.ParseName("Ferrite.TL")));

        var enm = SyntaxFactory.EnumDeclaration("TLConstructor")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        foreach (var item in schema.Constructors)
        {
            enm = enm.AddMembers(SyntaxFactory.EnumMemberDeclaration(item.Predicate.ToPascalCase() + " = " + item.Id));
        }
        foreach (var item in schema.Methods)
        {
            enm = enm.AddMembers(SyntaxFactory.EnumMemberDeclaration(item.Method.ToPascalCase() + " = " + item.Id));
        }
        syntax = syntax.AddMembers(enm);
        var code = syntax
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }

    static void Main()
    {
        var schema = TLSchema.Load("mtproto.json");
        if (!Directory.Exists("../../../../Ferrite.TL/mtproto/"))
        {
            Directory.CreateDirectory("../../../../Ferrite.TL/mtproto/");
        }
        foreach (var item in schema.Constructors)
        {
            if (item.Predicate == "vector")
            {
                continue;
            }

            if (!File.Exists("../../../../Ferrite.TL/mtproto/" + item.Predicate.ToPascalCase() + ".cs"))
            {
                using (var writer = new StreamWriter("../../../../Ferrite.TL/mtproto/" + item.Predicate.ToPascalCase() + ".cs", false))
                {
                    writer.Write(CreateTLObjectClass(item, "mtproto"));
                }
            }
        }
        foreach (var item in schema.Methods)
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
                    foreach (var item in schema.Constructors)
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
                    foreach (var item in schema.Methods)
                    {
                        arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression(item.Id)),
                            SyntaxFactory.ParseExpression("Read<" + item.Method.ToPascalCase() + ">(ref buff),"))
                            .WithLeadingTrivia(SyntaxFactory.Tab, SyntaxFactory.Tab)
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
                    }
                    arms = arms.Add(SyntaxFactory.SwitchExpressionArm(
                            SyntaxFactory.ConstantPattern(SyntaxFactory.ParseExpression("_")),
                            SyntaxFactory.ParseExpression("throw new DeserializationException(\"Constructor \"+ string.Format(\"0x{ 0:X}\", constructor) + \" not found.\")"))
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
                writer.Write(GenerateTLConstructorEnum(schema));
            }
        }
    }
}



