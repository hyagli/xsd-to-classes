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
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization.Advanced;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace CodeGeneration.ImporterExtensions
{
    /// <summary>
    /// The porpose of this schema importer extension (SIE) is an example
    /// since this particular one does the same thing as the "StripProxyTypesCodeModifier"
    /// This SIE strips any bnusiness objects from a generated web proxy.
    /// It will also add a "using" statement to the code that will point the types to another
    /// assemble where they are stored. The purpose of this is so that the client and the 
    /// server code can share an assembly that contains the types.
    /// This is not to be used by a XsdToClasses code generator
    /// 
    /// </summary>
    public class StripBusinessObjectsSchemaImporterExtension : SchemaImporterExtension
    {
        private static string[] importNamespaces = new string[0];

        public override string ImportSchemaType(
            string name, 
            string ns,
            XmlSchemaObject context, 
            XmlSchemas schemas,
            XmlSchemaImporter importer,
            CodeCompileUnit compileUnit, 
            CodeNamespace codeNamespace,
            CodeGenerationOptions options, 
            CodeDomProvider codeGenerator)
        {

            if (IsBaseType(name, ns))
            {
                return base.ImportSchemaType(name, ns,
                context, schemas,
                importer,
                compileUnit, codeNamespace,
                options, codeGenerator);
            }

            // Add the Namespace, except the first
            for (int i = 1; i < ImportNamespaces.Length; i++)
            {
                string _Import = ImportNamespaces[i];
                codeNamespace.Imports.Add(new CodeNamespaceImport(_Import));
            }

            return name;
        }

        private bool IsBaseType(string aName, string aNamespace)
        {
            if (aNamespace == "http://www.w3.org/2001/XMLSchema")
                return true;

            if (aName.StartsWith("ArrayOf"))
                return true;

            return false;
        }

        public static string[] ImportNamespaces
        {
            get { return importNamespaces; }
            set { importNamespaces = value; }
        }
    }
}
