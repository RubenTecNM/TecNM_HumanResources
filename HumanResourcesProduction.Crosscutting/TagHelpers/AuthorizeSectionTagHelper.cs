using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HumanResourcesProduction.TagHelpers
{
    [HtmlTargetElement("authorize-section")]
    public class AuthorizeSectionTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizeSectionTagHelper(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HtmlAttributeName("policy")]
        public string Policy { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // Don't render the tag helper tag itself

            var user = ViewContext.HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            if (!string.IsNullOrEmpty(Policy))
            {
                bool negate = Policy.StartsWith("!");
                string actualPolicy = negate ? Policy.Substring(1) : Policy;

                var authorizeResult = await _authorizationService.AuthorizeAsync(user, actualPolicy);
                
                if (negate)
                {
                    if (authorizeResult.Succeeded)
                    {
                        output.SuppressOutput();
                        return;
                    }
                }
                else
                {
                    if (!authorizeResult.Succeeded)
                    {
                        output.SuppressOutput();
                        return;
                    }
                }
            }
        }
    }
}
