using System.Web;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SessionSyncAttribute());
        }
    }
}
