using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService.Models
{
    public class ClienteABC
    {
        public string created_at { get; set; }
        public string no_cliente { get; set; }
        public string RFC { get; set; }
        public string nombre { get; set; }
        public string password { get; set; }
    }

    public class ClientePocket
    {
        public string RFC { get; set; }
        public string contratos { get; set; }
        public string telefonos { get; set; }  
        public string email { get; set; }
    }

    public class ClientABC
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string scope { get; set; }
    }
}