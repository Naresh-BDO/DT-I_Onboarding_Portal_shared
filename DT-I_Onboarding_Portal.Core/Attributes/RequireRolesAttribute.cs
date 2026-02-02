using Microsoft.AspNetCore.Identity;

namespace DT_I_Onboarding_Portal.Core.Attributes
{
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class RequireRolesAttribute : Attribute
        {
            public string[] Roles { get; }

            public RequireRolesAttribute(params string[] roles) => Roles = roles;
        }
}