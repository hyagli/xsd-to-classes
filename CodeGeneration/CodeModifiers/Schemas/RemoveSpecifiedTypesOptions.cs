using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGeneration.CodeModifiers
{

    partial class RemoveSpecifiedTypesOptions
    {
        public RemoveSpecifiedTypesOptions()
        {
            this.Type = new ClassTypeCollection();
        }
    }

    partial class ClassTypeCollection
    {
        internal bool ContainsName(string name)
        {
            foreach (ClassType classType in this)
                if (classType.Name == name)
                    return true;
            return false;
        }
    }
}
