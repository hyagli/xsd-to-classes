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
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XsdToClasses
{
	/// <summary>
	/// Registers the project with COM.
    /// Can do the same thing with 
    /// regasm.exe xsdtoclasses.dll
	/// </summary>
	[RunInstaller(true)]
	[System.ComponentModel.DesignerCategory("Code")]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);
			new RegistrationServices().RegisterAssembly(Assembly.GetExecutingAssembly(), AssemblyRegistrationFlags.SetCodeBase);
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback (savedState);
			new RegistrationServices().UnregisterAssembly(Assembly.GetExecutingAssembly());
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall (savedState);
			new RegistrationServices().UnregisterAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
