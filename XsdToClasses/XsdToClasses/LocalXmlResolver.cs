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
using System.Xml;

namespace BlueToque.XsdToClasses
{
    class LocalXmlResolver : XmlUrlResolver
    {
        string m_schemaFolder;
        public LocalXmlResolver(string folder)
        {
            m_schemaFolder = folder;
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {

            string newPath = Path.Combine(m_schemaFolder, Path.GetFileName(absoluteUri.AbsolutePath));
            FileStream fs = File.OpenRead(newPath) ;
            return fs;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}
