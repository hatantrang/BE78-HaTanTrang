using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class ProductListVM
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public IPagedList<Product> Products { get; set; }
    }
}
