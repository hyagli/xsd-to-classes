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
	///     Specifies custom tool registration information.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomToolAttribute : Attribute
    {
        /// <summary>
		///     Assigns custom tool information to the class.
		/// </summary>
		/// <param name="name">Name of the custom tool.</param>
		/// <param name="description">A description of the tool.</param>
		/// <param name="generatesDesignTimeCode">
		///     If <see langword="true" />, the IDE will try to compile on the fly the 
		///     dependent the file associated with this tool, and make it available 
		///     through intellisense to the rest of the project.
		/// </param>
		public CustomToolAttribute(string name, string description, bool generatesDesignTimeCode)
		{
			m_name           = name;
			m_description    = description;
			m_code           = generatesDesignTimeCode;
        }

        #region fields
        string m_name;
        string m_description;
        bool m_code;
        #endregion

        #region properties
        /// <summary> Name of the custom tool. </summary>
		public string Name { get { return m_name; } } 

		/// <summary> Friendly description of the tool. </summary>
		public string Description { get { return m_description; } }

		/// <summary>
		///     Specifies whether the tool generates design time code to compile on the fly.
		/// </summary>
		public bool GeneratesDesignTimeCode { get { return m_code; } }
        #endregion
    }
}
