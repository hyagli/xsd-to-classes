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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

using CodeGeneration.CodeModifiers;
using CodeGeneration;
using System.Xml;

namespace CodeGeneration.Generators
{
    /// <summary>
    /// ICodegenerator interface defines an object that generates code
    /// </summary>
    public interface ICodeGenerator
    {
        Assembly Assembly { get; }

        CodeModifierCollection CodeModifiers { get; }
        
        CodeNamespace CodeNamespace { get; }
        
        string CodeNamespaceString { get; set; }
        string CodeProvider { get; set; }
        string CodeString { get; }
        
        CompilerParameters CompilerParameters { get; set; }
        BaseCodeGeneratorOptions CodeGeneratorOptions { get; set; }
        CompilerErrorCollection Errors { get; }
        StringCollection ReferencedAssemblies { get; }

        bool HasErrors { get; }
        bool IsCompiled { get; }
        bool ThrowExceptions { get; set; }

        XmlDocument SourceDocument { get; set; }
    }
}
