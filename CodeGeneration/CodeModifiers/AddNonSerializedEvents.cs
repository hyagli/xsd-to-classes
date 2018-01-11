using System.CodeDom;

namespace CodeGeneration.CodeModifiers
{
    /// <summary>
    /// When generating code for WCF services, all public properties are by default 
    /// serialized. This code modifier attempts to marks as NonSerialzied all events.
    /// </summary>
    class AddNonSerializedEvents: BaseCodeModifier
    {
        public AddNonSerializedEvents() { }

        #region ICodeModifier Members

        public override void Execute(CodeNamespace codeNamespace)
        {
            // foreach datatype in the codeNamespace
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                if (type.IsEnum) continue;

                foreach (CodeTypeMember member in type.Members)
                {
                    CodeMemberEvent codeEvent = member as CodeMemberEvent;
                    if (codeEvent == null)
                        continue;
                    
                    // add the non serialized attribute
                    CodeAttributeDeclaration attr = new CodeAttributeDeclaration("field: System.NonSerialized");
                    codeEvent.CustomAttributes.Add(attr);
                }

            }
        }

        #endregion
    }
}
