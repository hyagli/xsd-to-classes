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
using System.Collections.Specialized;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using System.Xml.Schema;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;

using CodeGeneration.CodeModifiers;

namespace CodeGeneration.Generators
{
    /// <summary>
    /// Generate a classes from and XSD (Xml Schema Document)
    /// </summary>
    public class XsdClassGenerator : BaseCodeGenerator, ICodeGenerator
    {
        /// <summary>
        /// Constructor:
        /// - initialize the schema
        /// - compile the schema
        /// - set some defaults
        /// </summary>
        /// <param name="schema"></param>
        public XsdClassGenerator(XmlSchema schema)
        {
            m_schema = schema;
            if (m_schema == null)
                throw new ArgumentNullException("schema", "Xml Schema cannot be null");

            #region save the schema to an XmlDocument
            StringWriter sw = new StringWriter();
            m_schema.Write(sw);
            this.SourceDocument.LoadXml(sw.ToString());
            #endregion

            Utility.CompileSchema(schema);

            PreProcessSchemas();

            CompilerParameters.GenerateExecutable = false;
            CompilerParameters.GenerateInMemory = true;

            AddDefaultCodeModifiers();

        }


        /// <summary>
        /// Add the basic (default) code modifiers
        /// </summary>
        private void AddDefaultCodeModifiers()
        {
            CodeModifiers.Add(new CodeModifiers.ConvertArraysToCollections());
            CodeModifiers.Add(new CodeModifiers.AddDocComments());
            CodeModifiers.Add(new RemoveObjectBase());
        }

        #region fields
        private XsdClassGeneratorOptions m_xsdClassGeneratorOptions = new XsdClassGeneratorOptions();
        private CodeGenerationOptions m_codeGenerationOptions = CodeGenerationOptions.GenerateProperties;
        private XmlSchema m_schema;
        private List<XmlSchema> m_schemaList = new List<XmlSchema>();
        private List<object> m_objects = new List<object>();
        private SchemaImporterExtensionCollection m_schemaImporterExtensions = new SchemaImporterExtensionCollection();
        #endregion

        #region properties
        /// <summary>
        /// Options for this class generator
        /// </summary>
        public XsdClassGeneratorOptions ClassGeneratorOptions
        {
            get { return m_xsdClassGeneratorOptions; }
            set { m_xsdClassGeneratorOptions = value; }
        }

        /// <summary>
        /// Options to pass to the Microsoft code generator
        /// </summary>
        public CodeGenerationOptions CodeGenerationOptions
        {
            get { return m_codeGenerationOptions; }
            set { m_codeGenerationOptions = value; }
        }

        /// <summary>
        /// The Schema to generate code from
        /// </summary>
        public XmlSchema Schema
        {
            get { return m_schema; }
            set { m_schema = value; }
        }

        /// <summary>
        /// A list of instance objects from the compiled assembly
        /// </summary>
        public List<object> Objects { get { return m_objects; } }

        /// <summary>
        /// A collection of extensions that help while importing schemas
        /// </summary>
        public SchemaImporterExtensionCollection SchemaImporterExtensions { get { return m_schemaImporterExtensions; } }
        #endregion

        #region utility

        /// <summary>
        /// Generate code for all of the complex types in the schema
        /// </summary>
        /// <param name="xsd"></param>
        /// <param name="importer"></param>
        /// <param name="exporter"></param>
        private void GenerateForComplexTypes(
            XmlSchema xsd,
            XmlSchemaImporter importer,
            XmlCodeExporter exporter)
        {
            foreach (XmlSchemaObject type in xsd.SchemaTypes.Values)
            {
                XmlSchemaComplexType ct = type as XmlSchemaComplexType;

                if (ct != null)
                {
                    Trace.TraceInformation("Generating for Complex Type: {0}", ct.Name);

                    XmlTypeMapping mapping = importer.ImportSchemaType(ct.QualifiedName);
                    exporter.ExportTypeMapping(mapping);
                }
            }
        }

        /// <summary>
        /// Generate code for the elements in the Schema
        /// </summary>
        /// <param name="xsd"></param>
        /// <param name="importer"></param>
        /// <param name="exporter"></param>
        private void GenerateForElements(
            XmlSchema xsd,
            XmlSchemaImporter importer,
            XmlCodeExporter exporter)
        {
            foreach (XmlSchemaElement element in xsd.Elements.Values)
            {
                Trace.TraceInformation("Generating for element: {0}", element.Name);

                try
                {
                    XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);
                    exporter.ExportTypeMapping(mapping);
                }
                catch (Exception ex)
                {
                    Trace.TraceEvent(TraceEventType.Error, 10, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Pre-process the loaded schemas
        /// </summary>
        private void PreProcessSchemas()
        {
            Trace.Assert(m_schema != null);
            foreach (XmlSchemaExternal includedSchema in m_schema.Includes)
            {
                if (includedSchema != null &&
                    includedSchema.Schema != null &&
                    !m_schemaList.Contains(includedSchema.Schema))
                    m_schemaList.Add(includedSchema.Schema);
            }

        }

        /// <summary>
        /// Add assembly references to the list of assemblies to 
        /// include when compiling the code
        /// </summary>
        protected override void AddReferencedAssemblies()
        {
            if (!ReferencedAssemblies.Contains("System.dll"))
                ReferencedAssemblies.Add("System.dll");
            if (!ReferencedAssemblies.Contains("mscorlib.dll"))
                ReferencedAssemblies.Add("mscorlib.dll");
            if (!ReferencedAssemblies.Contains("system.xml.dll"))
                ReferencedAssemblies.Add("system.xml.dll");
            if (!ReferencedAssemblies.Contains(Assembly.GetExecutingAssembly().Location))
                ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            if (!ReferencedAssemblies.Contains("System.Drawing.dll"))
                ReferencedAssemblies.Add("System.Drawing.dll");
        }

        #endregion

        /// <summary>
        /// Compile the code
        /// </summary>
        /// <returns></returns>
        public bool Compile()
        {
            CodeString = string.Empty;

            CreateCodeNamespace();

            #region Generate the CodeDom from the XSD

            XmlSchemas xmlSchemas = new XmlSchemas();
            xmlSchemas.Add(m_schema);
            foreach (XmlSchema s in m_schemaList)
                xmlSchemas.Add(s);

            XmlSchemaImporter schemaImporter = new XmlSchemaImporter(xmlSchemas);
            foreach (SchemaImporterExtension extension in m_schemaImporterExtensions)
                schemaImporter.Extensions.Add(extension);

            XmlCodeExporter codeExporter = new XmlCodeExporter(
                CodeNamespace,
                new CodeCompileUnit(),
                m_codeGenerationOptions);

            try
            {
                GenerateForElements(m_schema, schemaImporter, codeExporter);
                GenerateForComplexTypes(m_schema, schemaImporter, codeExporter);
            }
            catch (Exception ex)
            {
                Trace.TraceEvent(TraceEventType.Error, 10, ex.ToString());
                throw new ApplicationException("Error Loading Schema: ", ex);
            }

            #endregion

            ModifyCodeDom();

            if (!GenerateCode())
                return false;

            #region Find the exported classes
            if (this.CodeGeneratorOptions.GenerateObjects && Assembly != null)
            {
                Type[] exportedTypes = Assembly.GetExportedTypes();

                // try to create an instance of the exported types
                m_objects = new List<object>();
                foreach (Type type in exportedTypes)
                {
                    if (type.IsAbstract)
                    {
                        Trace.TraceInformation("Type {0} is abstract, it is not created", type.ToString());
                        continue;
                    }

                    object obj = Activator.CreateInstance(type);
                    m_objects.Add(obj);
                }

            }
            #endregion

            return true;
        }

    }
}
