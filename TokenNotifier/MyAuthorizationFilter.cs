using Hangfire.Dashboard;

namespace TokenNotifier
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {

        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}