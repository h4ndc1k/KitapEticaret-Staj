using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    public class kitaplar
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string CoverURL { get; set; }
        public decimal? Price { get; set; }
        public int PubID { get; set; }

        public int? Stock { get; set; }
        public int? PageNum { get; set; }
        public string PublisherName { get; set; }
        public string AuthorName { get; set; }
        public List<string> Genres { get; set; }
    }
}