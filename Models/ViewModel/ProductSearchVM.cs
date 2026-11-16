using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class ProductSearchVM
    {
       
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? CategoryID { get; set; }

        public string StockFilter { get; set; }
        public const int LowStockLevel = 5;

      
        public string SortOrder { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public IPagedList<Product> Products { get; set; }
    }
}