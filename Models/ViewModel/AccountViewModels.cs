using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class AccountSearchVM
    {
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<User> Accounts { get; set; }
    }
}
