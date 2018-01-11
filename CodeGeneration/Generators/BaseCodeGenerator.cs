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
using System.Collections.Specialized;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Reflection;
using CodeGeneration.CodeModifiers;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace CodeGeneration.Generators
{
    /// <summary>
    /// The base class implementation of a code generator
    /// contains utility classes for generating code
    /// </summary>
    public class BaseCodeGenerator : ICodeGenerator
    {
        #region fields
        private XmlDocument m_sourceDocument = new XmlDocument();
        private BaseCodeGeneratorOptions m_options = new BaseCodeGeneratorOptions();
        private CodeModifierCollection m_codeModifiers = new CodeModifierCollection();
        private CompilerParameters m_compilerParameters = new CompilerParameters();
        private StringCollection m_errorStrings = new StringCollection();
        private CodeNamespace m_codeNamespace;
        private Assembly m_assembly = null;
        private string m_codeProvider = "CSharp";
        private string m_codeNamespaceString;
        private string m_codeString;
        private CompilerErrorCollection m_compilerErrors = new CompilerErrorCollection();
        private bool m_throwExceptions = false;
        #endregion

        #region ICodeGenerator Members

        /// <summary>
        /// The source document being used to generated this code
        /// </summary>
        public XmlDocument SourceDocument { get { return m_sourceDocument; } set { m_sourceDocument = value; } }

        /// <summary>
        /// Get and set the code generator options
        /// </summary>
        public BaseCodeGeneratorOptions CodeGeneratorOptions
        {
            get { return m_options; }
            set { m_options = value; }
        }

        /// <summary>
        /// Returns the course code as a string if the code was generated successfully
        /// </summary>
        public string CodeString 
        { 
            get { return m_codeString; }
            internal set { m_codeString = value; }
        }
        
        /// <summary>
        /// Returns true if there are any errors
        /// </summary>
        public bool HasErrors { get { return m_compilerErrors.Count > 0; } }
        
        /// <summary>
        /// returns true if the resulting assembly has been compiled, false otherwise
        /// </summary>
        public bool IsCompiled { get { return Assembly != null; } }

        /// <summary>
        /// A collection of error strings resulting from the compile
        /// </summary>
        public CompilerErrorCollection Errors { get { return m_compilerErrors; } }

        /// <summary>
        /// A list of code modifiers to apply to the code dom after compilation
        /// </summary>
        public CodeModifierCollection CodeModifiers { get { return m_codeModifiers; } }

        /// <summary>
        /// the code dom namespace, this is the structure that represents the code
        /// </summary>
        public CodeNamespace CodeNamespace { get { return m_codeNamespace; } }

        /// <summary>
        /// A list of referenced assemblies
        /// </summary>
        public StringCollection ReferencedAssemblies { get { return m_compilerParameters.ReferencedAssemblies; } }

        /// <summary>
        /// A pointer to the in-memory assembly that is the result of compilation
        /// </summary>
        public Assembly Assembly { get { return m_assembly; } }

        /// <summary>
        /// The logging level to use for the run of this code generator (for testing)
        /// </summary>
        public static SourceLevels Logging
        {
            get { return Trace.Switch.Level; }
            set { Trace.Switch.Level = value; }
        }

        /// <summary>
        /// The code namespace to use for code generation
        /// </summary>
        public string CodeNamespaceString
        {
            get { return m_codeNamespaceString; }
            set { m_codeNamespaceString = value; }
        }

        /// <summary>
        /// Parameters to pass to the compiler controling where the code generator puts code etc.
        /// </summary>
        public CompilerParameters CompilerParameters
        {
            get { return m_compilerParameters; }
            set { m_compilerParameters = value; }
        }

        /// <summary>
        /// The code provider, or what language to generate the code in
        /// </summary>
        public string CodeProvider
        {
            get { return m_codeProvider; }
            set { m_codeProvider = value; }
        }

        /// <summary>
        /// Set this to TRUE and instead of returning FALSE, the Compile method
        /// will throw exceptions. Default is FALSE
        /// </summary>
        public bool ThrowExceptions
        {
            get { return m_throwExceptions; }
            set { m_throwExceptions = value; }
        }
        #endregion

        #region protected

        /// <summary>
        /// Create the code namespace
        /// </summary>
        protected void CreateCodeNamespace()
        {
            if (string.IsNullOrEmpty(m_codeNamespaceString))
                m_codeNamespace = new CodeNamespace();
            else
                m_codeNamespace = new CodeNamespace(m_codeNamespaceString);

        }

        /// <summary>
        /// modify the generated code structures in memory
        /// </summary>
        protected void ModifyCodeDom()
        {
            foreach (ICodeModifier codeModifier in m_codeModifiers)
                codeModifier.Execute(m_codeNamespace, this);
        }

        /// <summary>
        /// Method to get an CodeDomProvider with which this class can create code.
        /// </summary>
        /// <returns></returns>
        //protected virtual ICodeGenerator GetCodeWriter()
        protected virtual CodeDomProvider GetCodeWriter()
        {
            return CodeDomProvider.CreateProvider(m_codeProvider);
        }

        /// <summary>
        /// Generate the code
        /// </summary>
        /// <returns></returns>
        protected bool GenerateCode()
        {
            m_assembly = null;

            #region Generate the code
            if (this.CodeGeneratorOptions.GenerateCodeString)
            {
                //CodeCompileUnit compileUnit = new CodeCompileUnit();
                //compileUnit.Namespaces.Add(m_codeNamespace);
                //CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                //provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);

                StringWriter sw = new StringWriter();
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.VerbatimOrder = true;
                GetCodeWriter().GenerateCodeFromNamespace(m_codeNamespace, sw, options);

                m_codeString = sw.ToString();
            }
            #endregion

            AddReferencedAssemblies();

            #region compile the generated code
            CompilerResults compilerResults = null;
            if (this.CodeGeneratorOptions.CompileAssembly)
            {
                CodeCompileUnit compileUnit = new CodeCompileUnit();
                compileUnit.Namespaces.Add(m_codeNamespace);
                CodeDomProvider provider = GetCodeWriter();
                compilerResults = provider.CompileAssemblyFromDom(m_compilerParameters, new CodeCompileUnit[] { compileUnit });

                // handle the errors if there are any
                if (compilerResults.Errors.HasErrors)
                {
                    m_compilerErrors.AddRange(compilerResults.Errors);

                    // check to see if there are fatal errors
                    bool fatalErrors = false;
                    foreach (CompilerError error in compilerResults.Errors)
                    {
                        if (!error.IsWarning)
                            fatalErrors = true;
                    }

                    #region trace the errors at this point
                    if (Trace.Switch.ShouldTrace(TraceEventType.Error))
                    {
                        string errorString= BuildCompileErrorString(compilerResults);
                        Trace.TraceError(errorString);
                    }
                    #endregion

                    #region throw exception if we've ben told to
                    if (this.ThrowExceptions && fatalErrors)
                    {
                        string errorString= BuildCompileErrorString(compilerResults);
                        throw new ApplicationException(errorString); 
                    }
                    #endregion

                    return !fatalErrors;
                }
            }

            if (compilerResults!=null)
                m_assembly = compilerResults.CompiledAssembly;
            #endregion

            return true;
        }

        /// <summary>
        /// Build a string from compilation errors
        /// </summary>
        /// <param name="compilerResults"></param>
        /// <returns></returns>
        protected static string BuildCompileErrorString(CompilerResults compilerResults)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Error compiling assembly:");

            foreach (CompilerError error in compilerResults.Errors)
            {
                sb.AppendFormat("{0} {1} ({2}): {3}, {4}\r\n",
                    error.IsWarning ? "Warning" : "Error",
                    error.ErrorText,
                    error.ErrorNumber,
                    error.Line,
                    error.Column);
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// this method is meant to be overridden in a derived class
        /// </summary>
        protected virtual void AddReferencedAssemblies() { }

    }
}
