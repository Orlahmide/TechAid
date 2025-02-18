using System.ComponentModel.DataAnnotations;

namespace TechAid.Validation
{
    public class EmailDomainAttribute : ValidationAttribute
    {
        private readonly string _requiredDomain;

        public EmailDomainAttribute(string requiredDomain)
        {
            _requiredDomain = requiredDomain;
        }

        public override bool IsValid(object? value)
        {
            if (value is string email)
            {
                return email.EndsWith(_requiredDomain, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
