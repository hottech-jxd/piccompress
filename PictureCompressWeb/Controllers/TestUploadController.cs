using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing.Imaging;
using PictureCompressWeb.Models;

namespace PictureCompressWeb.Controllers
{
    public class TestUploadController : Controller
    {
        public ActionResult Index()
        {      
            string appPath = Request.ApplicationPath;
            ViewBag.appPath = appPath;

            return View();
        }


        public JsonResult Upload()
        {
             JsonResult json =  new JsonResult();
             CompressModel msg = new CompressModel();

            int count = Request.Files.Count;
            if (count < 1)
            {
                msg.status = Constant.status_upload_fail;
                msg.message = "";
                json.Data = msg;
                json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return json;
            }

            String fileDir = Server.MapPath("~/Temps/");

            String sid = Request.QueryString["sid"];
                       

            string sDir = fileDir + "\\" +sid;

            if (!Directory.Exists( sDir ))
            {
                Directory.CreateDirectory(sDir );
            }
            string desPath = ""; String path="";
            //for (int i = 0; i < count; i++)
            //{
                HttpPostedFileBase file = Request.Files.Get(0);
             
                //if (file == null) continue;
                String fileName = file.FileName;                


                path = sDir +"\\"+ fileName;

                file.SaveAs(path);

                int quantity = 0;

               desPath = compress(path , out quantity);

             //  break;
            //}


               transforpic(desPath, sid , msg);


            if (System.IO.File.Exists( desPath))
            {
                msg.status = Constant.status_compress_sucess;

                getFileInfo( sid , path , desPath , msg , quantity );

            }
            else
            {
                msg.status = Constant.status_compress_fail;
            }


            //msg.status = Constant.status_upload_success;
            //CompressModel status = new CompressModel();
            //status.status = Constant.status_upload_success;
            //msg.message="";
            json.Data = msg;
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return json;
        }

        protected void getFileInfo(string sid , string path , string desPath , CompressModel msg,int quantity)
        {
            long size1 = getFileSize(path);
            long size2 = getFileSize(desPath);
            msg.rate = getRate(size1, size2);
            msg.originSize = formatFileSize(size1);
            msg.size = formatFileSize(size2);
            string rootPath = Request.ApplicationPath;
            msg.orginpath =( rootPath.EndsWith("/")? rootPath : rootPath+"/") + "Temps/" + sid + "/" + Path.GetFileName(path);
            msg.minpath = ( rootPath.EndsWith("/")? rootPath : rootPath+"/")  + "Temps/" + sid + "/" + Path.GetFileName(desPath);
            msg.filename = Path.GetFileName(path);
            msg.quantity = quantity;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                System.Drawing.Image bitmap = new System.Drawing.Bitmap(fs);
                msg.orginwidth = bitmap.Width;
                msg.orginheight = bitmap.Height;

                if (bitmap.RawFormat.Guid.Equals(ImageFormat.Png.Guid))
                {
                    msg.minQuantity = 1;
                    msg.maxQuantity = 256;
                }
                else if (bitmap.RawFormat.Guid.Equals(ImageFormat.Jpeg.Guid))
                {
                    msg.minQuantity = 10;
                    msg.maxQuantity = 100;
                }
                bitmap.Dispose();
            }

        }

        private string formatFileSize(long size)
        {
            if (size > 1024 * 1024)
            {
                long s = size / (1024 * 1024);
                long ss = size % (1024 * 1024);
                return s + "." + ss + "M";
            }
            else
            {
                long s = (size / 1024);
                long ss = size % (1024);
                return s + "." + ss + "K";
            }
        }

        private long getFileSize(string path)
        {
            System.IO.FileInfo fileInfo = new FileInfo(path);
            long l = fileInfo.Length;
            return l;
        }

        private string getRate(long size1, long size2)
        {
            double  rate = (size1 - size2) * 100.00 / size1;
            return (-Math.Floor(rate)).ToString()+"%";
        }


        public JsonResult apply()
        {
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            string sid = Request.QueryString["sid"];
            string filename = Request.QueryString["filename"];
            string dir = Server.MapPath("~/temps/");
            dir +=sid;
            string path = dir + "\\" + filename;
            string filenamenoext = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path).ToLower();
            string desPath = dir + "\\" + filenamenoext + "-min" + ext;
            if (!System.IO.File.Exists(path))
            {
                CompressModel model = new CompressModel();
                model.status = Constant.status_file_notexist;
                model.message = Constant.message_file_notexist;
                json.Data = model;
                return json;
            }
            if (!System.IO.File.Exists(desPath))
            {
                CompressModel model = new CompressModel();
                model.status = Constant.status_file_notexist;
                model.message = Constant.message_file_notexist;
                json.Data = model;
                return json;
            }
            string quanlitystr = Request.QueryString["quantity"];
            int quanlity = 0;
            if( !int.TryParse(quanlitystr, out quanlity)){
                CompressModel model = new CompressModel();
                model.status = Constant.status_parameter_error;
                json.Data = model;
                return json;
            }
          
            compress(path , quanlity);
          

            CompressModel result = new CompressModel();
            if (System.IO.File.Exists(desPath))
            {
                result.status = Constant.status_compress_sucess;
                getFileInfo(sid, path, desPath, result,quanlity);

            }
            else
            {
                result.status = Constant.status_compress_fail;
            }

            json.Data = result;
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return json;

        }


        public JsonResult getFileStatus()
        {
            String fileDir = Server.MapPath("~/Temps/");
            string sid = Request.QueryString["sid"];
            string sDir = fileDir + "\\" + sid;
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (!Directory.Exists(sDir))
            {
                CompressModel msg = new CompressModel();
                msg.status = Constant.status_dir_notexist;
                msg.message = "文件夹不存在!";
                json.Data = msg;
                return json;
            }

            string filename = Request.QueryString["filename"];
            string path = sDir + "\\" + filename;
            if (!System.IO.File.Exists(path))
            {
                CompressModel msg = new CompressModel();
                msg.status = Constant.status_file_notexist;
                msg.message = Constant.message_file_notexist;
                msg.message = "文件不存在!";
                json.Data = msg;
                return json;
            }

            string ext = Path.GetExtension(path);
            string filenamenotext = Path.GetFileNameWithoutExtension(path);
            string desPath = sDir + "\\" + filenamenotext + "-min" + ext;
            if (!System.IO.File.Exists(desPath))
            {
                CompressModel msg = new CompressModel();
                msg.status = Constant.status_file_notexist;
                msg.message = Constant.message_file_notexist;
                json.Data = msg;
                return json;
            }


            CompressModel result = new CompressModel();
            result.status = Constant.status_compress_sucess;
            result.message = ""; 
            getFileInfo(sid, path, desPath, result, 90 );
            json.Data = result;
            return json;

        }

        private void cmdCompressPng(string path , int quantity , string desPath)
        {
            string toolpath = AppDomain.CurrentDomain.BaseDirectory + "bin\\";
            string tooldisk = Path.GetPathRoot(toolpath);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            string cmdPath = toolpath + "pngquant.exe";
            process.StartInfo.WorkingDirectory = toolpath;
            process.StartInfo.FileName = cmdPath;
            process.StartInfo.Arguments = "--force "+ quantity +" --verbose " + path + " -o " + desPath;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.ErrorDataReceived += process_ErrorDataReceived;
            //process.Exited += process_Exited;
            //process.OutputDataReceived += process_OutputDataReceived;
            //process.EnableRaisingEvents = true;
            process.Start();
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            String s = process.StandardOutput.ReadToEnd();
            String ss = process.StandardError.ReadToEnd();

            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        private string  compress(string path , out int quantity )
        {
            quantity = 0;

            var t = ImageFormat.Png;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                System.Drawing.Bitmap bitm = new System.Drawing.Bitmap(fs );
                t = bitm.RawFormat;
                bitm.Dispose();
                bitm = null;
            }


            if (t.Guid.Equals(System.Drawing.Imaging.ImageFormat.Png.Guid))
            {
                quantity = 256 / 2;
                return compresspng(path, quantity);
            }
            else if ( t.Guid.Equals(System.Drawing.Imaging.ImageFormat.Jpeg.Guid))
            {
                quantity = 50;
                return compressjpg(path , quantity );
            }
            else
            {
            }

            return "";
        }

        private string compress(string path, int quantity)
        {
            string disk = Path.GetPathRoot(path);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileName(path);
            string filenamenoext = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path).ToLower();

            string desPath = dir + "\\" + filenamenoext + "-min" + ext;
            var t = ImageFormat.Png;
            using( FileStream fs =new FileStream(path,FileMode.Open)){            
                System.Drawing.Bitmap bitm = new System.Drawing.Bitmap(fs);
                t = bitm.RawFormat;
                bitm.Dispose();
                bitm = null;
            }
        

            if (t.Guid.Equals(System.Drawing.Imaging.ImageFormat.Png.Guid))
            {
                return compresspng(path, quantity);
            }
            else if (t.Guid.Equals(System.Drawing.Imaging.ImageFormat.Jpeg.Guid))
            {
                return compressjpg(path, quantity);
            }
            else
            {
            }
            return desPath;
        }

        private string compresspng(string path, int quantity)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                string filenamenoext = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path).ToLower();
                string desPath = dir + "\\" + filenamenoext + "-min" + ext;
                cmdCompressPng(path, quantity, desPath);
                return desPath;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private void transforpic(string path , string sid , CompressModel msg  )
        {
            if (!System.IO.File.Exists(path)) return;

            int th_width = 0;
            int th_height = 0;
            string th_path;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                System.Drawing.Image bitmap = new System.Drawing.Bitmap(fs);
                int orginwidth = bitmap.Width;
                int orginheight = bitmap.Height;

                th_width = Constant.thumbnail_width;
                th_height = Constant.thumbnail_height;
                
                double rate1 = th_width*1.0/th_height;
                double rate2 = orginwidth*1.0/orginheight;
                if( rate1 >= rate2 ){
                    th_width = orginwidth* th_height  / orginheight;
                }else{
                    th_height = orginheight*th_width / orginwidth;
                }
                
                System.Drawing.Bitmap th_bitmap = new System.Drawing.Bitmap(th_width, th_height);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(th_bitmap);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                g.Clear(System.Drawing.Color.White);
                //绘制缩略图
                g.DrawImage(bitmap , new System.Drawing.Rectangle(0, 0, th_width , th_height ), new System.Drawing.Rectangle(0, 0, orginwidth , orginheight ), System.Drawing.GraphicsUnit.Pixel);

               
                string th_dir  = Path.GetDirectoryName(path);
                string th_filenamenoext = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                th_path = th_dir + "\\"+ th_filenamenoext+"-thumbnail"+ext;
         
                //保存缩略图
                th_bitmap.Save (th_path);

                g.Dispose();
                th_bitmap.Dispose();
                bitmap.Dispose();

                msg.thumbnailheight = th_height;
                msg.thumbnailwidth = th_width;

               string rootPath = Request.ApplicationPath;
               rootPath = (rootPath.EndsWith("/") ? rootPath : rootPath + "/");

                msg.thumbnailpath = rootPath + "Temps/" + sid + "/" + Path.GetFileName(th_path);

            }
        }

        private string compressjpg(string path , int compress)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                string filenamenoext = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path).ToLower();
                string desPath = dir + "\\" + filenamenoext + "-min" + ext;
                BitMiracle.LibJpeg.JpegImage jpeg=new BitMiracle.LibJpeg.JpegImage (path);
                BitMiracle.LibJpeg.CompressionParameters p = new BitMiracle.LibJpeg.CompressionParameters();
                p.Quality = compress;
                p.SmoothingFactor = 50;
                p.SimpleProgressive = true;
                using (FileStream output = new FileStream(desPath, FileMode.Create)) {
                    jpeg.WriteJpeg(output, p);
            }

               

                //System.Drawing.Image b = System.Drawing.Image.FromFile(path);
                //System.Drawing.Imaging.ImageFlags flags = (System.Drawing.Imaging.ImageFlags) Enum.Parse( typeof( System.Drawing.Imaging.ImageFlags ), b.Flags.ToString());


                //EncoderParameters encoder = new EncoderParameters(1);
                //encoder.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compress);
                //ImageCodecInfo imagecoder = GetEncoderInfo("image/jpeg");
                //System.Drawing.Image bitm = System.Drawing.Image.FromFile(path);
                //  string dir = Path.GetDirectoryName(path);
                //  string filenamenoext = Path.GetFileNameWithoutExtension(path);
                //string ext = Path.GetExtension(path).ToLower();
                // string desPath = dir+"\\"+filenamenoext+"-min"+ ext;

                // bitm.Save(desPath, imagecoder, encoder);

                // bitm.Dispose();
                // bitm = null;

                 return desPath;

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // Return an ImageCodecInfo object for this mime type.
        private ImageCodecInfo GetEncoderInfo(string mime_type)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i <= encoders.Length; i++)
            {
                if (encoders[i].MimeType == mime_type) return encoders[i];
            }
            return null;
        }

        void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e == null || e.Data == null ) return;
            Console.WriteLine( sender.ToString() +"  "+ e.Data);
            //throw new NotImplementedException();
            string ss = "";
        }

        void process_Exited(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            string ss = "";
        }

        void process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e == null || e.Data == null) return;
            //throw new NotImplementedException();
            Console.WriteLine( sender.ToString()+" " + e.Data);
            string dd = ";";
        }


        public ActionResult zip()
        {            
            if (!Request.QueryString.AllKeys.Contains("sid"))
            {
                JsonResult result = new JsonResult();
                MessageModel msg = new MessageModel();
                msg.Code = 500;
                msg.Message = "参数错误";
                result.Data = msg;
                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return result;
            }

            string sid = Request.QueryString["sid"];
            return zipFiles(sid);

        }

        private ActionResult zipFiles(string sid)
        {   
            JsonResult result ;
             MessageModel msg;
            string dirPath = Server.MapPath("~/Temps/"+sid);
            if (!Directory.Exists(dirPath))
            {
                result = new JsonResult();
                msg = new MessageModel();
                msg.Code = 500;
                msg.Message = "文件夹不存在";
                result.Data = msg;
                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return result;
            }


            String ext = "*-min.jpg,*-min.jpeg,*-min.png";

             IEnumerable<string> list = Directory.GetFiles(dirPath).ToList()
                 .Where(item => {  return ext.Contains(Path.GetExtension(item).ToLower());})
                 .Where(item => { return Path.GetFileNameWithoutExtension(item).EndsWith("-min"); });

            using (Ionic.Zip.ZipFile entry = new Ionic.Zip.ZipFile())
            {
                //for(int i = 0;i<list.Length;i++){
                //    String path = list[i];
                //    String filename = Path.GetFileName(path);

                //}

                String zipPath = dirPath + "\\piccompress.zip";

                entry.AddFiles(list, "");

                

                entry.Save(zipPath);
            }
           

            result=new JsonResult();
            msg = new MessageModel();
            msg.Code = 200;
            string rootpath = Request.ApplicationPath;
            rootpath = rootpath.EndsWith("/") ? rootpath : rootpath + "/";
            msg.Message = rootpath +"temps/" + sid + "/piccompress.zip";
            result.Data = msg;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            return result;

        }


        public JsonResult clearFiles()
        {
            JsonResult result = new JsonResult();
            MessageModel msg = new MessageModel();

            if (!Request.QueryString.AllKeys.Contains("sid"))
            {          
                msg.Code = 500;
                msg.Message = "参数错误";
                result.Data = msg;
                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return result;
            }

            string sid = Request.QueryString["sid"];
            string dir = Server.MapPath( "~/temps/"+sid );
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            msg.Code = 200;
            result.Data = msg;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;

        }

        public JsonResult deleteFile()
        {
            string sid = Request.QueryString["sid"];
            string filename = Request.QueryString["filename"];
            string dir = Server.MapPath("~/temps");
            dir = dir + "\\" + sid;
            string path = dir + "\\" + filename;
            string ext = Path.GetExtension(path);
            string filenamenoext = Path.GetFileNameWithoutExtension(path);
            string desPath = dir + "\\" + filenamenoext+"-min"+ ext;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            if (System.IO.File.Exists(desPath))
            {
                System.IO.File.Delete(desPath);
            }

            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            CompressModel msg = new CompressModel();
            msg.status = Constant.status_success;
            return json;

        }

    }
}
