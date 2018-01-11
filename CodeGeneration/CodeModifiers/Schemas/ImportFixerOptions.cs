using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGeneration.CodeModifiers
{

    partial class ImportFixerOptions
    {
        public ImportFixerOptions()
        {
            this.Namespace=new NamespaceTypeCollection();
        }
    }

    partial class NamespaceTypeCollection
    {
        internal bool ContainsXmlName(string name)
        {
            foreach (NamespaceType ns in this)
                if (ns.XmlNamespace == name)
                    return true;
            return false;
        }
    }
}
