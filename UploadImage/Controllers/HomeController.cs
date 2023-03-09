using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using UploadImage.Models;
using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;

namespace UploadImage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment he;         

        public HomeController(ILogger<HomeController> logger, IHostEnvironment e)
        {
            he = e;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult UploadIMG(string fullName,IFormFile file)
        {
            string fileName=string.Empty;
            string path = string.Empty;
            if (file.Length >0)
            {
                string directoryPath = @"wwwroot/Images/avatar";
                // 2.Khai báo một thể hiện của lớp DirectoryInfo
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                // Kiểm tra thư mục chưa tồn tại mới sử dụng phương thức tạo
                if (!directory.Exists)
                {     
                    // 3.Sử dụng phương thức Create để tạo thư mục.
                        directory.Create();
                }
                fileName =Guid.NewGuid()+Path.GetExtension(file.FileName);
                    path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/avatar"));
                    string fullPath=Path.Combine(path,file.FileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        string newSize = ResizeImage(image, 100, 100);
                        string[] aSize = newSize.Split(',');
                        image.Mutate(h => h.Resize(Convert.ToInt32(aSize[1]), Convert.ToInt32(aSize[0])));   
                        image.Save(fullPath);
                        ViewData["fullName"] = file.FileName.ToString();
                        ViewData["fileLocation"] = "/images/avatar/" + Path.GetFileName(file.FileName);  
                    }   
                }          
                return View();
        }    
        public string ResizeImage(Image img,int maxWidth,int maxHeight)
        {
            if (img.Width > maxWidth || img.Height > maxHeight)
            {
                double widthRatio = (double)img.Width / (double)maxWidth;
                double heightRatio = (double)img.Height / (double)maxHeight;
                double raito = Math.Max(widthRatio, heightRatio);
                int newWidth = (int)(img.Width / raito);
                int newHeight = (int)(img.Height / raito);
                return newHeight.ToString() + "," + newWidth.ToString();
            }
            else { return img.Height.ToString() + "," + img.Width.ToString(); }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
       


    }
}