namespace DT_I_Onboarding_Portal.Core.enums
{
    public enum EmailErrorType
    {
        None,
        AuthenticationFailed,
        SmtpConnectionFailed,
        SmtpSendFailed,
        RecipientRejected,
        InvalidRecipientAddress,
        ConfigurationError,
        Timeout,
        NetworkError,
        Unknown
    }
}
