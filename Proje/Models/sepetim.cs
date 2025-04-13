using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    public class sepetim
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalPrice { get; set; }
        public string PublisherName { get; set; } // Yayıncı adı
        public string AuthorName { get; set; }    // Yazar adı
        public string CoverURL { get; set; }
        public int PubID { get; set; }

        
        public int? Stock { get; set; }
        public int? PageNum { get; set; }   






    }
}