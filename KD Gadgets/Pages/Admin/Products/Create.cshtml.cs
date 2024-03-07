using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace KD_Gadgets.Pages.Admin.Products
{
    [BindProperties]
    public class CreateModel : PageModel

    {
       

        [Required(ErrorMessage ="The product name is required")]
        public string ProductName { get; set; } = "";

        [Required(ErrorMessage = "The price is required")]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "The category  is required")]
        public string Category { get; set; } = "";

        public string? Description { get; set; }= "";

        [Required(ErrorMessage = "The image  is required")]
        public IFormFile ImageFile { get; set; }

        public string errorMessage = "";
        public string successMessage = "";

        private IWebHostEnvironment webHostEnvironment;
        private readonly string connectionString;


        public CreateModel(IWebHostEnvironment env, IConfiguration configuration)
        {
            webHostEnvironment = env;
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }




        public void OnGet()
        {


        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }
            //successful data validation
            if (Description == null) Description = "";

            //saving the image
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(ImageFile.FileName);

            string imageFolder = webHostEnvironment.WebRootPath + "/images";

            string imageFullPath = Path.Combine(imageFolder, newFileName);

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                ImageFile.CopyTo(stream);
            }

            //saving the new product to the db
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO products" +
                        "(product_name, price, category, description, image_filename) VALUES" +
                        "(@product_name, @price, @category, @description, @image_filename);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@product_name", ProductName);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename", newFileName);

                        command.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex) 
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Product created successfully";
			Response.Redirect("/Admin/Products/Index");


		}



	}
}
