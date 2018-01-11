using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGeneration.CodeModifiers
{
    public partial class ConvertArraysToCollectionsOptions
    {
        public ConvertArraysToCollectionsOptions()
        {
            this.Exclude = new ExcludeTypeCollection();
            this.Include = new IncludeTypeCollection();
        }
    }
}
