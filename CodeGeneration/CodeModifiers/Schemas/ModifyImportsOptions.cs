using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGeneration.CodeModifiers
{

    partial class ModifyImportsOptions
    {
        public ModifyImportsOptions()
        {
            this.AddImport = new ImportTypeCollection();
            this.RemoveImport = new ImportTypeCollection();
        }
    }
    
}
