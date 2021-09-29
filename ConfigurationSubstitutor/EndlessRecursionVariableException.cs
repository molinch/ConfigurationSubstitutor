namespace ConfigurationSubstitution
{
    using System;

    public class EndlessRecursionVariableException : Exception
    {
        public EndlessRecursionVariableException(string variableName)
            : base($"Variable {variableName} is causing an endless recursion")
        {
        }
    }
}