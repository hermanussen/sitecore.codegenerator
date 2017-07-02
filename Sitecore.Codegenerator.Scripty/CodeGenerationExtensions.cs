using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Sitecore.Codegenerator.Scripty
{
    /// <summary>
    /// Code copied and slightly changed from https://github.com/olegsych/T4Toolbox/blob/master/src/T4Toolbox/CSharpTemplate.cs
    /// This needs some improvement, as Roslyn is able to verify keywords and check if identifiers are valid.
    /// </summary>
    public static class CodeGenerationExtensions
    {
        /// <summary>
        /// Contains C# reserved keywords defined in MSDN documentation at <a href="http://msdn.microsoft.com/en-us/library/x53a06bb.aspx"/>.
        /// </summary>
        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally",
            "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int",
            "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "volatile", "void", "while"
        };

        /// <summary>
        /// Converts a given string to a valid C# field name using camelCase notation.
        /// </summary>
        /// <remarks>
        /// This method converts the first letter of the given string to lower case and calls the <see cref="Identifier"/> method to ensure that it's valid.
        /// You can override this method in your template to replace the default implementation.
        /// </remarks>
        public static string FieldName(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            name = name.Trim();
            name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            return name.Identifier();
        }

        /// <summary>
        /// Converts a given string into a valid C# identifier.
        /// </summary>
        /// <remarks>
        /// This method is based on rules defined in <a href="http://msdn.microsoft.com/en-us/library/aa664670.aspx">C# language specification</a>. 
        /// It removes white space characters and concatenates words using camelCase notation. If the resulting string is 
        /// a C# keyword, the the method prepends it with the @ symbol to change it into a literal C# identifier. You can override this method
        /// in your template to replace the default implementation.
        /// </remarks>
        public static string Identifier(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var builder = new StringBuilder();

            char c = '\x0000';     // current character within name
            int i = 0;             // current index within name

            // Skip invalid characters from the beginning of the name 
            while (i < name.Length)
            {
                c = name[i++];

                // First character must be a letter or _
                if (char.IsLetter(c) || c == '_')
                {
                    break;
                }
            }

            if (i <= name.Length)
            {
                builder.Append(c);
            }

            bool capitalizeNext = false;

            // Strip invalid characters from the remainder of the name and convert it to camelCase
            while (i < name.Length)
            {
                c = name[i++];

                // Subsequent character can be a letter, a digit, combining, connecting or formatting character
                UnicodeCategory category = char.GetUnicodeCategory(c);
                if (!char.IsLetterOrDigit(c) &&
                    category != UnicodeCategory.SpacingCombiningMark &&
                    category != UnicodeCategory.ConnectorPunctuation &&
                    category != UnicodeCategory.Format)
                {
                    capitalizeNext = true;
                    continue;
                }

                if (capitalizeNext)
                {
                    c = char.ToUpperInvariant(c);
                    capitalizeNext = false;
                }

                builder.Append(c);
            }

            string identifier = builder.ToString();

            // If identifier is a reserved C# keyword
            if (ReservedKeywords.Contains(identifier))
            {
                // Convert it to literal identifer
                return "@" + identifier;
            }

            return identifier;
        }

        /// <summary>
        /// Converts a given string to a valid C# property name using PascalCase notation.
        /// </summary>
        /// <remarks>
        /// This method converts the first letter of the given string to upper case and calls the <see cref="Identifier"/> method to ensure that it's valid.
        /// You can override this method in your template to replace the default implementation.
        /// </remarks>
        public static string PropertyName(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            name = name.Trim();
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);
            return name.Identifier();
        }
    }
}
