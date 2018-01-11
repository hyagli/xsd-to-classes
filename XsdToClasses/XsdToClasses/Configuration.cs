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
using System.IO;
using System.Xml.Serialization;

using BlueToque.XsdToClasses.Properties;
using CodeGeneration.CodeModifiers;

namespace BlueToque.XsdToClasses
{
    /// <summary>
    /// This class contains configuration settings for XsdToClasses
    /// </summary>
    partial class Configuration
    {
        private static Configuration m_configuration;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Configuration()
        {
            this.CodeModifiers = new AssemblyTypeCollection();
            this.SchemaImporterExtensions = new AssemblyTypeCollection();
            this.EnableDataBinding = true;
            this.GenerateComplexTypes = true;
            this.GenerateComplexTypesSpecified = true;
            this.GenerateDebuggableCode = true;
            this.GenerateProperties = true;
            this.GenerateSoapTypes = false;
        }

        private static Configuration CreateDefaultConfiguration()
        {
            Configuration config = new Configuration();
            config.EnableDataBinding = true;
            config.GenerateOrder= true;
            config.GenerateProperties = true;
            config.GenerateComplexTypes = true;
            config.GenerateComplexTypesSpecified = true;

            config.CodeModifiers.Add(new AssemblyType(typeof(CodeGeneration.CodeModifiers.ConvertArraysToCollections)));
            config.CodeModifiers.Add(new AssemblyType(typeof(CodeGeneration.CodeModifiers.RemoveDebuggerAttribute)));
            //config.CodeModifiers.Add(new AssemblyType(typeof(CodeGeneration.CodeModifiers.RemoveSpecifiedTypes)));
            //config.CodeModifiers.Add(new AssemblyType(typeof(CodeGeneration.CodeModifiers.ModifyImports)));
            AssemblyType importFixer = new AssemblyType(typeof(CodeGeneration.CodeModifiers.ImportFixer));
            importFixer.Any = CodeGeneration.CodeModifiers.ImportFixer.DefaultOptions;
            config.CodeModifiers.Add(importFixer);
            
            return config;
        }

        /// <summary>
        /// Get the filename to save the configuration to.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string GetConfigFileName(string fileName)
        {
            string configFile = Path.ChangeExtension(fileName, Resources.GeneratedCodeExtension + ".xml");
            return configFile;
        }

        #region load and save
        /// <summary>
        /// Load the configuration from the output file if it exists
        /// Otherwise, create a new configuration
        /// </summary>
        /// <param name="fileName"></param>
        public static void Load(string fileName)
        {
            try
            {
                string configFile = GetConfigFileName(fileName);

#if REPLACEORIGINAL
            if (!File.Exists(configFile))
                m_configuration = new Configuration();

            StringBuilder sb = new StringBuilder();


            // parse the input file and look for the configuration
            using (StreamReader sr = new StreamReader(configFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith(Resources.Configuration_StartDelimeter))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith(Resources.Configuration_EndDelimeter))
                                break;
                            sb.Append(line);
                        }

                        break;
                    }
                }

            }

            // if we found the configuration, deserialize it
            if (sb.Length > 0)
            {
                try
                {
                    m_configuration = Serializer.Deserialize<Configuration>(sb.ToString());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    m_configuration = new Configuration();
                }
                
            }
            else
            {
                m_configuration = new Configuration();
            }
#else
                if (File.Exists(configFile))
                {
                    m_configuration = Serializer.DeserializeFromFile<Configuration>(configFile);
                }
                else
                {
                    m_configuration = CreateDefaultConfiguration();
                }
#endif
            }
            catch (Exception ex)
            {
                string message = string.Format("Error loading XmlToClasses configuration file {0}. See Inner Exception below for details", fileName);
                throw new Exception(message, ex);
            }
        }

        /// <summary>
        /// Save the output to the given file
        /// If the file already exists, don't overwrite it
        /// </summary>
        /// <param name="fileName"></param>
        public static void Save(string fileName)
        {
            try
            {
                string configFile = GetConfigFileName(fileName);

#if REPLACEORIGINAL
            StringBuilder original = new StringBuilder();
            
            string str;
            // read the original file
            using (StreamReader sr = new StreamReader(configFile))
                str = sr.ReadToEnd();

            original.Append(str);
            // find the delimeters and delete the old configuration
            int start = str.IndexOf(Resources.Configuration_StartDelimeter);
            if (start >= 0)
            {
                int end = str.LastIndexOf(Resources.Configuration_EndDelimeter) + Resources.Configuration_EndDelimeter.Length;
                original.Remove(start, end - start);
            }
            else
            {
                start = str.IndexOf("<xs:schema");
            }

            // insert the new configuration
            string config = Output();
            original.Insert(start, config);

            // write it out
            using (StreamWriter sw = new StreamWriter(configFile, false))
                sw.Write(original.ToString());
#else
                if (!File.Exists(configFile))
                    Serializer.SerializeToFile<Configuration>(m_configuration, configFile);

#endif
            }
            catch (Exception ex)
            {
                string message = string.Format("Error saving XmlToClasses configuration file {0}. See Inner Exception below for details", fileName);
                throw new Exception(message, ex);
            }
        }

        /// <summary>
        /// Save the configuration to a string
        /// </summary>
        /// <returns></returns>
        public static string Save()
        {
            return Serializer.Serialize<Configuration>(m_configuration);
        }
        
        #endregion

#if REPLACEORIGINAL

        /// <summary>
        /// Get the configuration output to save to the generated file
        /// </summary>
        /// <returns></returns>
        public static string Output()
        {
            string config = string.Format(
                Resources.Configuration_FormatString,
                Resources.Configuration_StartDelimeter,
                Serializer.Serialize<Configuration>(m_configuration),
                Resources.Configuration_EndDelimeter);
            return config;
        }
#endif

        /// <summary>
        /// Static accessor for the current configuration
        /// </summary>
        public static Configuration Current { get { return m_configuration; } }

        #region properties

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool GenerateDebuggableCode
        {
            get
            {
                return this.CodeModifiers.Contains(typeof(CodeGeneration.CodeModifiers.RemoveDebuggerAttribute));
            }
            set
            {

                if ((this.GenerateDebuggableCode != value))
                {
                    if (value)
                    {
                        this.CodeModifiers.Add(typeof(CodeGeneration.CodeModifiers.RemoveDebuggerAttribute));
                    }
                    else
                    {
                        this.CodeModifiers.Remove(typeof(CodeGeneration.CodeModifiers.RemoveDebuggerAttribute));
                    }
                    this.RaisePropertyChanged("GenerateDebuggableCode");
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool GenerateSoapTypes
        {
            get
            {
                return this.CodeModifiers.Contains(typeof(CodeGeneration.ImporterExtensions.SoapTypeExtension));
            }
            set
            {

                if ((this.GenerateSoapTypes != value))
                {
                    if (value)
                    {
                        this.CodeModifiers.Add(typeof(CodeGeneration.ImporterExtensions.SoapTypeExtension));
                    }
                    else
                    {
                        this.CodeModifiers.Remove(typeof(CodeGeneration.ImporterExtensions.SoapTypeExtension));
                    }
                    this.RaisePropertyChanged("GenerateSoapTypes");
                }

            }
        }
        #endregion

    }

    public partial class AssemblyType
    {
        public AssemblyType()
        {
        }

        public AssemblyType(Type type)
        {
            this.Type = type.FullName;
            this.Assembly = type.Assembly.FullName;
            ICodeModifier modifier = Activator.CreateInstance(type) as ICodeModifier;
            this.Any = modifier.XmlOptions;
        }

    }

    public partial class AssemblyTypeCollection
    {
        /// <summary>
        /// Returns true if this collection contains a reference to this type
        /// </summary>
        /// <param name="type"></param>
        public bool Contains(Type type)
        {
            foreach (AssemblyType x in this)
                if (x.Type == type.FullName &&
                    x.Assembly == type.Assembly.FullName)
                    return true;
            return false;
        }

        /// <summary>
        /// Add an AssemblyType if it's not already added
        /// </summary>
        /// <param name="type"></param>
        public new void Add(AssemblyType type)
        {
            foreach (AssemblyType x in this)
                if (x.Type == type.Type &&
                    x.Assembly == type.Assembly)
                    return;
            base.Add(type);
        }

        /// <summary>
        /// Add a AssemblyType if it's not already added
        /// </summary>
        /// <param name="type"></param>
        public void Add(Type type)
        {
            foreach (AssemblyType x in this)
                if (x.Type == type.FullName &&
                    x.Assembly == type.Assembly.FullName)
                    return;
            base.Add(new AssemblyType(type));
        }

        /// <summary>
        /// Remove a CodeModifierType
        /// </summary>
        /// <param name="type"></param>
        public void Remove(Type type)
        {
            foreach (AssemblyType x in this)
                if (x.Type == type.FullName &&
                    x.Assembly == type.Assembly.FullName)
                    base.Remove(x);

        }
    }
}
