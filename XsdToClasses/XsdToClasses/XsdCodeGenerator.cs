//=============================================================================
//
// Copyright (C) 2007 Michael Coyle, Blue Toque
// http://www.BlueToque.ca/Products/XsdToClasses.html
// Michael.Coyle@BlueToque.ca
// Based on examples and some code provided by:
//         Daniel Cazzulino - kzu.net@gmail.com
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
#region Usage
/* Usage:
 * Add an .xsd file to the project and set:
 *	Build Action: Content
 *	Custom Tool: XsdToClasses
 * 
 * Author: Michael Coyle - Michael.Coyle@BlueToque.ca
 */
#endregion Usage

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;
using BlueToque.XsdToClasses.Properties;
using CodeGeneration.CodeModifiers;
using CodeGeneration.Generators;
using EnvDTE;

namespace BlueToque.XsdToClasses
{
	/// <summary>
	/// Uses the XsdGeneratorLibrary to process XSD files and generate the corresponding 
	/// classes. Also use Code generation extensions and intercepts to modify the generated 
    /// code.
	/// </summary>
    [Guid("596E7927-FC7C-4b31-8AE7-8C128FA0D637")]
	[ComVisible(true)]
    [CustomTool("XsdToClasses", "XSD to Classes Generator", true)]
    [VersionSupport("8.0")]     // Visual studio 2005
    [VersionSupport("9.0")]     // Visual studio 2008
    [VersionSupport("10.0")]    // Visual studio 2010
    [VersionSupport("11.0")]    // Visual studio 2012
    [VersionSupport("12.0")]    // Visual studio 2013
    [CategorySupport(CategorySupportAttribute.CSharpCategory)]
	//[CategorySupport(CategorySupportAttribute.VBCategory)]
	public class XsdCodeGenerator : CustomTool
	{
		/// <summary>
		/// Generates the output.
		/// </summary>
		protected override string OnGenerateCode(string inputFileName, string inputFileContent)
        {
            #region load the configuration
            Configuration.Load(base.InputFilePath);
            StringBuilder output = new StringBuilder();
            output.Append(CustomTool.GetToolGeneratedCodeWarning(typeof(XsdCodeGenerator)));
            #endregion

            #region read the schema(s)
            XmlSchema xsd;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(base.InputFilePath));

            using (FileStream fs = File.OpenRead(base.InputFilePath))
            {
                xsd = XmlSchema.Read(fs, null);
            }
            #endregion

            #region create the class generator and set default options
            XsdClassGenerator xsdClassGenerator = new XsdClassGenerator(xsd);

            xsdClassGenerator.CodeGenerationOptions = CodeGenerationOptions.None;

            if (Configuration.Current.EnableDataBinding)
                xsdClassGenerator.CodeGenerationOptions |= CodeGenerationOptions.EnableDataBinding;
            
            if (Configuration.Current.GenerateOrder)
                xsdClassGenerator.CodeGenerationOptions |= CodeGenerationOptions.GenerateOrder;

            if (Configuration.Current.GenerateProperties)
                xsdClassGenerator.CodeGenerationOptions |= CodeGenerationOptions.GenerateProperties;

            xsdClassGenerator.CodeNamespaceString = base.FileNameSpace;
            xsdClassGenerator.CodeGeneratorOptions.GenerateCodeString = true;
            xsdClassGenerator.CodeGeneratorOptions.GenerateObjects = false;
            xsdClassGenerator.CodeGeneratorOptions.CompileAssembly = false;
            #endregion

            #region load the code modifiers
            foreach (AssemblyType assembly in Configuration.Current.CodeModifiers)
            {
                ObjectHandle handle = Activator.CreateInstance(assembly.Assembly, assembly.Type);
                if (handle == null)
                {
                    output.AppendFormat("//\tWarning, could not create CodeModifier type {0} from assembly {0}", assembly.Type, assembly.Assembly);
                    continue;
                }

                ICodeModifier modifier = handle.Unwrap() as ICodeModifier;
                if (modifier == null)
                {
                    output.AppendFormat("//\tWarning CodeModifier {0} from assembly {0} does not derive from ICodeModifier", assembly.Type, assembly.Assembly);
                    continue;
                }

                modifier.XmlOptions = assembly.Any;

                xsdClassGenerator.CodeModifiers.Add(modifier);
            }
            #endregion

            #region load the SchemaImporterExtensions
            foreach (AssemblyType assembly in Configuration.Current.SchemaImporterExtensions)
            {
                ObjectHandle handle = Activator.CreateInstance(assembly.Assembly, assembly.Type);
                if (handle == null)
                {
                    output.AppendFormat("//\tWarning, could not create SchemaImporterExtensions type {0} from assembly {0}", assembly.Type, assembly.Assembly);
                    continue;
                }

                SchemaImporterExtension extension = handle.Unwrap() as SchemaImporterExtension;
                if (extension == null)
                {
                    output.AppendFormat("//\tWarning SchemaImporterExtensions {0} from assembly {0} does not derive from SchemaImporterExtension", assembly.Type, assembly.Assembly);
                    continue;
                }

                xsdClassGenerator.SchemaImporterExtensions.Add(extension);
            }            
            #endregion

            // generate code
            xsdClassGenerator.Compile();

            // save config file and make sure it's added to the project
            Configuration.Save(base.InputFilePath);
            AddToProject(Configuration.GetConfigFileName(base.InputFilePath));

            #region Workaround for known bug with fixed attributes:
            output.AppendLine(Resources.Message_1591);
            output.AppendLine(Resources.Pragma_1591_Disable);
            output.Append(xsdClassGenerator.CodeString);
            output.Append(Resources.Pragma_1591_Enable);
            #endregion

            return output.ToString();
		}

        /// <summary>
        /// Add a file to the project dependant uping the file we're currently generating code for.
        /// </summary>
        /// <param name="tempFileName"></param>
        internal void AddToProject(string fileName)
        {
            if (this.Project == null)
                return;

            ProjectItem item = this.CurrentItem;
            if (item == null)
                return;

            // check to make sure the item is not already in the project
            string fileNameOnly = Path.GetFileName(fileName);
            
            foreach (ProjectItem projItem in item.ProjectItems)
            {
                if (projItem.Name == fileNameOnly)
                    return;
            }

            // check to make sure the file exists
            if (!File.Exists(fileName))
                return; 

            try
            {
                // add as “DependentUpon”
                item.ProjectItems.AddFromFile(fileName);

                // expand the files below the selected item
                item.ExpandView();

                // save the project
                this.Project.Save(this.Project.FullName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error adding file \'{0}\' to project \'{1}\':\r\n{2}",
                    fileName, 
                    this.Project.FullName,
                    ex.ToString());
            }
        }

        #region Registration and Installation

        /// <summary>Registers the generator.</summary>
		[ComRegisterFunction]
		public static void RegisterClass(Type type) { CustomTool.Register(typeof(XsdCodeGenerator)); }

		/// <summary>Unregisters the generator.</summary>
		[ComUnregisterFunction]
		public static void UnregisterClass(Type t) { CustomTool.UnRegister(typeof(XsdCodeGenerator)); }

        #endregion 
	}
}