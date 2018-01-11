using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("XsdToClasses")]
[assembly: AssemblyDescription("Visual Studio extension to convert XSDs to classes")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("BlueToque")]
[assembly: AssemblyProduct("BlueToque.XsdToClasses")]
[assembly: AssemblyCopyright("Copyright © Blue Toque 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("93ce2880-712f-40fe-a6d8-05469eeb05fa")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.2.23.0")]
[assembly: AssemblyFileVersion("1.2.23.0")]

internal class ThisAssembly
{
    private static ThisAssembly m_thisAssembly = new ThisAssembly();
    public static ThisAssembly P { get  { return m_thisAssembly; } }

    public string Name { get { return this.GetType().Assembly.GetName().Name; } }
    public string Description { get { return GetAssemblyDescription(this.GetType().Assembly); } }
    public string Author { get { return GetAssemblyAuthor(this.GetType().Assembly); } }
    public string Version { get { return this.GetType().Assembly.GetName().Version.ToString(); } }

    public static string GetAssemblyName(Assembly asm) { return (asm == null) ? string.Empty : asm.GetName().Name; }
    public static string GetAssemblyVersion(Assembly asm) { return (asm==null)?string.Empty:asm.GetName().Version.ToString(); }
    public static string GetAssemblyAuthor(Assembly asm)
    {
        if (asm==null) return string.Empty;

        if (AssemblyCompanyAttribute.IsDefined(asm, typeof(AssemblyCompanyAttribute)))
        {
            AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)
                AssemblyCompanyAttribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute));
            return company.Company;
        }
        return string.Empty;
    }
    public static string GetAssemblyDescription(Assembly asm)
    {
        if (asm == null) return string.Empty;
        if (AssemblyDescriptionAttribute.IsDefined(asm, typeof(AssemblyDescriptionAttribute)))
        {
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute)
                AssemblyDescriptionAttribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
            return description.Description;
        }
        return string.Empty;
    }

}