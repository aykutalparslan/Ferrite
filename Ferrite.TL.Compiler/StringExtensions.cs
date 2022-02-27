/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Globalization;

namespace Ferrite.TL.Compiler
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string str){
            str = str.ToLower().Replace("_", " ");
            TextInfo info = CultureInfo.InvariantCulture.TextInfo;
            return info.ToTitleCase(str).Replace(" ", string.Empty);
        }
        public static string ToCamelCase(this string str)
        {
            var chars = str.ToPascalCase().ToCharArray();
            chars[0] = Char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}

