//=============================================================================
//
// Copyright (C) 2007 Michael Coyle, Blue Toque
// http://www.BlueToque.ca/Products/XmlGridControl2.html
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

namespace CodeGeneration.CodeModifiers
{
    /// <summary>
    /// The collection type converter give a slightly more useable view of a collection
    /// in the property grid
    /// </summary>
    public class RemoveSpecifiedTypes : BaseCodeModifier
    {

        RemoveSpecifiedTypesOptions m_options;

        public RemoveSpecifiedTypesOptions Options
        {
            get
            {
                if (m_options == null)
                    m_options = GetOptions<RemoveSpecifiedTypesOptions>();
                return m_options;
            }            
        }

        #region ICodeModifier Members

        public override void Execute(CodeNamespace codeNamespace)
        {
            CodeTypeDeclarationCollection typesToRemove = new CodeTypeDeclarationCollection();
            // foreach datatype in the codeNamespace
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                if (Options.Type.ContainsName(codeNamespace.Name+"."+type.Name) ||
                    Options.Type.ContainsName( type.Name))
                    typesToRemove.Add(type);
            }

            foreach (CodeTypeDeclaration type in typesToRemove)
                codeNamespace.Types.Remove(type);

        }

        #endregion
    }
}
