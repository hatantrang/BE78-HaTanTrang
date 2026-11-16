using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models
{
    public partial class Product
    {

        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }

        [NotMapped]
        public int OriginalStock { get; set; }
    }
}