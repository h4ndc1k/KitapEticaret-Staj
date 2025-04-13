using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    public class addmodel
    {
        public int? AddressID { get; set; } // Mevcut adres varsa ID'si, yoksa null olabilir
        public string City { get; set; }
        public string District { get; set; }
        public string Nbhood { get; set; }
        public string Street { get; set; }
    }
}