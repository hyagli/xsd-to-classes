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

namespace BlueToque.XsdToClasses
{
	/// <summary>
	/// Determines which versions of VS.NET are supported by the custom tool.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class VersionSupportAttribute : Attribute
	{
		Version _version;

		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		/// <param name="version">Version supported by the tool.</param>
		public VersionSupportAttribute(string version)
		{
			_version = new Version(version);
		}

		/// <summary>Version supported by the tool.</summary>
		public Version Version { get { return _version; } } 
	}
}
