//=============================================================================
//
// Copyright (C) 2007 Michael Coyle, Blue Toque
// http://www.BlueToque.ca/Products/XsdToClasses.html
// michael.coyle@BlueToque.ca
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// http://www.gnu.org/licenses/gpl.txt
//
//=============================================================================
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;

namespace BlueToque.XsdToClasses
{
    class Utility
    {

        /// <summary>
        /// Takes a candidate c# identifier and makes it valid by removing whitespace, 
        /// checking collision with c# keywords, and ensuring the composing characters 
        /// are valid identifier characters.
        /// </summary>
        /// <param name="candidate">the candidate identifier</param>
        /// <returns>a valid c# identifier</returns>
        internal static string GetValidCSharpIdentifier(string candidate)
        {
            // return CodeIdentifier.MakeValid(candidate);

            const char underscore = '_';
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            //if the candidate is null, set it to a single underscore.
            if (candidate == null)
            {
                candidate = underscore.ToString();
            }

            //remove any spaces
            candidate = candidate.Replace(" ", string.Empty);
            StringBuilder attempt;
            try
            {
                //the CreateValidIdentifier() call only checks collisions with reserved keywords
                attempt = new StringBuilder(provider.CreateValidIdentifier(candidate));
                if (attempt.Length < 1)
                {
                    attempt = new StringBuilder(new string(underscore, 1));
                }
            }
            catch (Exception ex)
            {
                attempt = new StringBuilder(new string(underscore, 1));
                Trace.TraceWarning(ex.ToString());
            }

            //loop through the string, ensuring that each character is a letter, digit or underscore
            for (int index = 0; index < attempt.Length; index++)
            {
                if (!char.IsLetterOrDigit(attempt[index]) && attempt[index] != underscore)
                {
                    attempt[index] = underscore;
                }
            }

            //finally check that the first digit is a letter or underscore
            if (char.IsDigit(attempt[0]))
            {
                attempt.Insert(0, underscore);
            }
            return attempt.ToString();
        }
    }            
}
