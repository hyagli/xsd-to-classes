using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using XsdCodeGeneration.CodeModifiers;

namespace XsdCodeGeneration
{
    public class XsdCodeGenerator
    {
        public XsdCodeGenerator()
        {
            m_codeModifiers = new List<ICodeModifier>();
        }

        #region fields
        private string m_codeString;
        private StringCollection m_errorStrings;
        private List<ICodeModifier> m_codeModifiers;
        #endregion

        #region properties
        public string CodeString
        {
            get { return m_codeString; }
            set { m_codeString = value; }
        }

        public StringCollection ErrorStrings
        {
            get { return m_errorStrings; }
            set { m_errorStrings = value; }
        }

        public List<ICodeModifier> CodeModifiers
        {
            get { return m_codeModifiers; }
            set { m_codeModifiers = value; }
        }
        #endregion

        public ObjectCollection GenerateClasses(XmlSchema schema)
        {
            #region Generate the CodeDom from the XSD
            CodeNamespace codeNamespace = new CodeNamespace("TestNameSpace");

            XmlSchemas xmlSchemas = new XmlSchemas();
            xmlSchemas.Add(schema);
            XmlSchemaImporter schemaImporter = new XmlSchemaImporter(xmlSchemas);

            XmlCodeExporter codeExporter = new XmlCodeExporter(
                codeNamespace,
                new CodeCompileUnit(),
                CodeGenerationOptions.GenerateProperties);

            foreach (XmlSchemaElement element in schema.Elements.Values)
            {
                try
                {
                    XmlTypeMapping map = schemaImporter.ImportTypeMapping(element.QualifiedName);
                    codeExporter.ExportTypeMapping(map);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error Loading Schema: ", ex);
                }

            }
            #endregion

            #region Modify the CodeDom

            foreach (ICodeModifier codeModifier in m_codeModifiers)
                codeModifier.Execute(codeNamespace);

            #endregion

            #region Generate the code
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(codeNamespace);
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
#if DEBUG
            StringWriter sw = new StringWriter();
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.VerbatimOrder = true;
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
            m_codeString = sw.ToString();
#endif
            #endregion

            #region Compile an assembly
            CompilerParameters compilerParameters = new CompilerParameters();

            #region add references to assemblies
            // reference for 
            //  System.CodeDom.Compiler
            //  System.CodeDom
            //  System.Diagnostics
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("mscorlib.dll");

            // System.Xml
            compilerParameters.ReferencedAssemblies.Add("system.xml.dll");

            // reference to this assembly for the custom collection editor
            compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParameters.ReferencedAssemblies.Add("System.Drawing.dll");

            // System.ComponentModel
            #endregion

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;

            CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, new CodeCompileUnit[] { compileUnit });

            // handle the errors if there are any
            if (results.Errors.HasErrors)
            {
                m_errorStrings = new StringCollection();
                m_errorStrings.Add("Error compiling assembly:\r\n");
                foreach (CompilerError error in results.Errors)
                    m_errorStrings.Add(error.ErrorText + "\r\n");
                return null;
            }

            #endregion

            #region Find the exported classes
            Assembly assembly = results.CompiledAssembly;
            Type[] exportedTypes = assembly.GetExportedTypes();

            // try to create an instance of the exported types
            ObjectCollection objectCollection = new ObjectCollection();
            objectCollection.Clear();
            foreach (Type type in exportedTypes)
            {
                object obj = Activator.CreateInstance(type);
                objectCollection.Add(new ObjectItem(type.Name, obj));
            }

            #endregion

            return objectCollection;

        }

        public static void InstantiateAllMembers(object obj)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                // if there's already a value then we don't create a new one
                object value = property.GetValue(obj, null);
                if (value != null)
                    continue;

                Type propertyType = property.PropertyType;

                // check to see if we have a parameterless constructor
                if (propertyType.GetConstructor(new Type[0] { }) == null)
                    continue;
      
                // the magic happens here
                object propertyObject = Activator.CreateInstance(propertyType);

                property.SetValue(obj, propertyObject, null);

            }
        }
    }
}
