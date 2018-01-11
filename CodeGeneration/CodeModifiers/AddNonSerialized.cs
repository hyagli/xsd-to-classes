//=============================================================================
//
// Copyright (C) 2007 Michael Coyle, Blue Toque
// http://www.BlueToque.ca/Products/CodeGeneration.html
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
using System.CodeDom;
using System.Xml;

namespace CodeGeneration.CodeModifiers
{
    /// <summary>
    /// Add non serialized attribute to members that should not be 
    /// serialized using a binary formatter.
    /// </summary>
    public class AddNonSerialized : BaseCodeModifier
    {
        public AddNonSerialized() { }

        #region ICodeModifier Members

        public override void Execute(CodeNamespace codeNamespace)
        {
            // foreach datatype in the codeNamespace
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                if (type.IsEnum) continue;

                foreach (CodeTypeMember member in type.Members)
                {
                    CodeMemberField codeField = member as CodeMemberField;
                    if (codeField == null)
                        continue;

                    // check if the Field is XmlElement
                    //"[System.ComponentModel.TypeConverter(typeof(ByteTypeConverter))]";
                    if (codeField.Type.BaseType == typeof(XmlElement).ToString())
                    {
                        // add the custom type editor attribute
                        CodeAttributeDeclaration attr = new CodeAttributeDeclaration("System.NonSerialized");
                        codeField.CustomAttributes.Add(attr);

                    }
                }

            }
        }

        #endregion
    }
}
