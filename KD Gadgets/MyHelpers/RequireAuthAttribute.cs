using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KD_Gadgets.MyHelpers
{
    public class RequireAuthAttribute : Attribute, IPageFilter
    {

        public string RequiredRole { get; set; } = "";

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            string? role = context.HttpContext.Session.GetString("role");
            if (role == null)
            {
                //the user is not authenticated so we have to redirect the user to the home pahe
                context.Result = new RedirectResult("/");
            }

            else
            {
                if (RequiredRole.Length > 0 && !RequiredRole.Equals(role))
                {
                    // the user is authenticated but the role in not authorized(meaning not admin)
                    context.Result = new RedirectResult("/");
                }
            }
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {

        }
    }
}
