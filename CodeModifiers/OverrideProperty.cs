//=============================================================================
//
// Copyright (C) 2013 Michael Coyle, Blue Toque
// http://www.bluetoque.ca/products/xsdtoclasses/
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
using BlueToque.XmlLibrary.CodeModifiers.Schemas;
using CodeGeneration.CodeModifiers;

namespace BlueToque.XmlLibrary.CodeModifiers
{
    /// <summary>
    /// Make a property in a class override
    /// </summary>
    public class OverrideProperty : BaseCodeModifier
    {
        OverridePropertyOptions m_options;

        public OverridePropertyOptions Options
        {
            get
            {
                if (m_options == null)
                    m_options = GetOptions<OverridePropertyOptions>();
                return m_options;
            }
        }

        #region ICodeModifier Members
        public override void Execute(CodeNamespace codeNamespace)
        {
            if (Options == null || Options.Property == null || Options.Property.Count == 0)
                return;

            // foreach datatype in the codeNamespace
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {

                // if the qualified name doesn't start with the name of the class, continue.
                PropertyType propertyType = Options.Property.Find(x => x.QualifiedName.StartsWith(type.Name));
                if (propertyType == null)
                    continue;

                // for each property in the type
                foreach (CodeTypeMember member in type.Members)
                {
                    if (!(member is CodeMemberProperty))
                        continue;

                    CodeMemberProperty property = (member as CodeMemberProperty);
                    if (propertyType.QualifiedName.EndsWith(property.Name) ||
                        propertyType.QualifiedName.EndsWith("*"))
                    {
                        property.Attributes =
                            MemberAttributes.Override |
                            MemberAttributes.Public;
                    }
                }
            }
        }

        #endregion
    }
}
