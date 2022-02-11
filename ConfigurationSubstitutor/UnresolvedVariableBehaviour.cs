namespace ConfigurationSubstitution
{
    public enum UnresolvedVariableBehaviour
    {
        /// <summary>
        /// <see cref="UndefinedConfigVariableException"/> is thrown for unresolved variables
        /// </summary>
        Throw,

        /// <summary>
        /// Unresolved variables are ignored and replaced by empty content
        /// </summary>
        IgnorePattern,

        /// <summary>
        /// The pattern for substitution is kept in case of unresolved variables
        /// </summary>
        KeepPattern
    }
}