using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PictureCompressWeb.Models
{
    public class CompressModel
    {
        public int status{get;set;}
        public string message { get; set; }

        public string originSize { get; set; }

        public string size { get; set; }

        public string rate { get; set; }

        public string orginpath { get; set; }

        public string minpath { get; set; }

        public string filename { get; set; }

        public int quantity { get; set; }

        public int minQuantity { get; set; }
        public int maxQuantity { get; set; }

        public int orginwidth { get; set; }
        public int orginheight { get; set; }

        public string thumbnailpath { get; set; }

        public int thumbnailwidth { get;set; }
        public int thumbnailheight { get; set; }

    }
}