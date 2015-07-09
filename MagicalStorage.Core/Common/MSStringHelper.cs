using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// This class provides static utility functions dealing with strings.
    /// </summary>
    public static class MSStringHelper
    {
        // C# keywords: http://msdn.microsoft.com/en-us/library/x53a06bb(v=vs.71).aspx
        static string[] keywords = new[]
                {
                "abstract",
"as",
"base",
"bool",
"break",
"byte",
"case",
"catch",
"char",
"checked",
"class",
"const",
"continue",
"decimal",
"default",
"delegate",
"do",
"double",
"else",
"enum",
"event",
"explicit",
"extern",
"false",
"finally",
"fixed",
"float",
"for",
"foreach",
"goto",
"if",
"implicit",
"in",
"int",
"interface",
"internal",
"is",
"lock",
"long",
"namespace",
"new",
"null",
"object",
"operator",
"out",
"override",
"params",
"private",
"protected",
"public",
"readonly",
"ref",
"return",
"sbyte",
"sealed",
"short",
"sizeof",
"stackalloc",
"static",
"string",
"struct",
"switch",
"this",
"throw",
"true",
"try",
"typeof",
"uint",
"ulong",
"unchecked",
"unsafe",
"ushort",
"using",
"virtual",
"void",
"volatile",
"var", // TuanNV added
"while"
                };

        // definition of a valid C# identifier: http://msdn.microsoft.com/en-us/library/aa664670(v=vs.71).aspx
        const string formattingCharacter = @"\p{Cf}";
        const string connectingCharacter = @"\p{Pc}";
        const string decimalDigitCharacter = @"\p{Nd}";
        const string combiningCharacter = @"\p{Mn}|\p{Mc}";
        const string letterCharacter = @"\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}";
        const string identifierPartCharacter = letterCharacter + "|" +
        decimalDigitCharacter + "|" +
        connectingCharacter + "|" +
        combiningCharacter + "|" +
        formattingCharacter;
        const string identifierPartCharacters = "(" + identifierPartCharacter + ")+";
        const string identifierStartCharacter = "(" + letterCharacter + "|_)";
        const string identifierOrKeyword = identifierStartCharacter + "(" +
        identifierPartCharacters + ")*";

        static Regex validIdentifierRegex = new Regex("^" + identifierOrKeyword + "$", RegexOptions.Compiled);

        /// <summary>
        /// Check if a string is a valid identifier.
        /// </summary>
        /// <param name="identifier">String</param>
        /// <returns>Boolean</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            if (String.IsNullOrWhiteSpace(identifier)) return false;

            var normalizedIdentifier = identifier.Normalize();

            // 1. check that the identifier match the validIdentifer regex and it's not a C# keyword
            if (validIdentifierRegex.IsMatch(normalizedIdentifier) && !keywords.Contains(normalizedIdentifier))
            {
                return true;
            }

            // 2. check if the identifier starts with @
            if (normalizedIdentifier.StartsWith("@") && validIdentifierRegex.IsMatch(normalizedIdentifier.Substring(1)))
            {
                return true;
            }

            // 3. it's not a valid identifier
            return false;
        }



        public static int LengthOfString(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return 0;
            }
            else
            {
                return input.Length;
            }
        }


        //public static bool IsLikeIgnoreCase(string me, string another)
        //{
        //    if (String.IsNullOrWhiteSpace(me) || String.IsNullOrWhiteSpace(another))
        //    {
        //        throw new ArgumentNullException("Param 'me' and 'another' must be not null");
        //    }
        //    return IsLike(me.ToLower(), another.ToLower());
        //}

        //public static bool IsLike(string me, string another)
        //{
        //    if (String.IsNullOrWhiteSpace(me) || String.IsNullOrWhiteSpace(another))
        //    {
        //        throw new ArgumentNullException("Param 'me' and 'another' must be not null");
        //    }
        //    if (!another.Contains("%"))
        //    {
        //        another = "%" + another + "%";
        //    }
        //    return (new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(another, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(me));
        //}
    }
}
