using System;
using System.Collections.Generic;

namespace CodeGeneration.CodeModifiers
{
    /// <summary>
    /// A strongly typed  collection of ICodeModifers
    /// </summary>
    public class CodeModifierCollection : List<ICodeModifier>
    {
        /// <summary>
        /// Remove the code modifier with the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True if successfull, false otherwise</returns>
        public bool Remove(Type type)
        {
            foreach (ICodeModifier modifier in this)
            {
                if (modifier.GetType() == type)
                    return Remove(modifier);
            }
            return false;
        }

        /// <summary>
        /// Don't add the same code modifier twice
        /// </summary>
        /// <param name="modifier"></param>
        public new void Add(ICodeModifier modifier)
        {
            if (this.Contains(modifier))
                return;
            base.Add(modifier);
        }
    }
}
