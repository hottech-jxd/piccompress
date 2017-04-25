using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PictureCompressWeb.Models
{
    public class Constant
    {
        public static int status_init = 0;
        public static int status_upload_success = 1;
        public static int status_upload_fail = 2;
        public static int status_compress_sucess = 8;
        public static int status_compress_fail = 9;
        public static int status_dir_notexist = 20;
        public static int status_file_notexist = 21;
        public static int status_parameter_error = 1000;
        public static int status_success = 200;

        public static int thumbnail_width = 150;
        public static int thumbnail_height = 100;

        public static string message_file_notexist = "文件不存在";
    }
}