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
	///     Determines which VS.NET generator categories are supported by the custom tool.
	///     This class also contains constants for C# and VB.NET category guids.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CategorySupportAttribute : Attribute
	{
		Guid _category;

		/// <summary> VS Generator Category for C# Language. </summary>
		public const string CSharpCategory = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";

		/// <summary> VS Generator Category for VB Language. </summary>
		public const string VBCategory = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";

		/// <summary> Initializes the attribute. </summary>
		/// <param name="categoryGuid">
		/// Either <see cref="CSharpCategory"/> or <see cref="VBCategory"/>.
		/// </param>
		public CategorySupportAttribute(string categoryGuid)
		{
			_category = new Guid(categoryGuid);
		}

		/// <summary> The identifier of the supported category. </summary>
		public Guid Guid { get { return _category; } } 

	}
}
