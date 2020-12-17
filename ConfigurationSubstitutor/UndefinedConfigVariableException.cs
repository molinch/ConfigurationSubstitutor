namespace ConfigurationSubstitution
{
    using System;

    public class UndefinedConfigVariableException : Exception
    {
        public UndefinedConfigVariableException(string variableName)
            : base($"No value found for configuration variable {variableName}")
        {
        }
    }
}