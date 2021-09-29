namespace ConfigurationSubstitution
{
    using System;

    public class RecursiveConfigVariableException : Exception
    {
        public RecursiveConfigVariableException(string variableName)
            : base($"Variable {variableName} is recursive")
        {
        }
    }
}