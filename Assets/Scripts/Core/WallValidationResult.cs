namespace Quoridor.Core
{
    /// <summary>
    /// Result returned by a wall validation step.
    /// </summary>
    public readonly struct WallValidationResult
    {
        private WallValidationResult(bool isValid, string reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        /// <summary>
        /// True when the validation step passed.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Human-readable reason when validation failed.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Shared successful validation result.
        /// </summary>
        public static WallValidationResult Valid { get; } = new WallValidationResult(true, string.Empty);

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static WallValidationResult Invalid(string reason)
        {
            return new WallValidationResult(false, reason);
        }
    }
}
