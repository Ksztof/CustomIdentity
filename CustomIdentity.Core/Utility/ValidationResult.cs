namespace CustomIdentity.Core.Utility
{
    public class ValidationResult
    {
        public List<string> ErrorMessages { get; } = new List<string>();

        public bool IsValid => ErrorMessages.Count == 0;
    }
}
