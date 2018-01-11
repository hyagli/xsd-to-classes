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
using System.CodeDom;
using System.Xml;
using CodeGeneration.Generators;
using CodeGeneration.Internal;

namespace CodeGeneration.CodeModifiers
{
    /// <summary>
    /// A base class implementation of the ICodeModifier interface
    /// Contains basic utility methods.
    /// </summary>
    public class BaseCodeModifier : ICodeModifier
    {
        XmlElement m_xmlOptions;
        ICodeGenerator m_codeGenerator;
        CodeNamespace m_codeNamespace;

        public XmlElement XmlOptions
        {
            get { return m_xmlOptions; }
            set { m_xmlOptions = value; }
        }

        public ICodeGenerator CodeGenerator
        {
            get { return m_codeGenerator; }
        }

        public CodeNamespace CodeNamespace
        {
            get { return m_codeNamespace; }
            set { m_codeNamespace = value; }
        }
        
        public virtual void Execute(CodeNamespace codeNamespace)
        {
            Execute(codeNamespace, null);
        }

        public virtual void Execute(CodeNamespace codeNamespace, ICodeGenerator codeGenerator)
        {
            m_codeGenerator = codeGenerator;
            Execute(codeNamespace);
        }

        protected T GetOptions<T>() where T : class, new()
        {
            T options = null;
            if (this.XmlOptions == null)
            {
                options = new T();
                this.XmlOptions = Serializer.SerializeToElement<T>(options);
            }
            else
            {
                try
                {
                    options = Serializer.Deserialize<T>(this.XmlOptions);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    options = new T();
                    this.XmlOptions = Serializer.SerializeToElement<T>(options);
                }
            }

            return options;
        }

    }
}
