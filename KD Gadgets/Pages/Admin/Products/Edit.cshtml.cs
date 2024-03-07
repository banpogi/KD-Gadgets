    using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace KD_Gadgets.Pages.Admin.Products
{
	[BindProperties]
    public class EditModel : PageModel
    {
		



		public int Id { get; set; }


		[Required(ErrorMessage = "The product name is required")]
		public string ProductName { get; set; } = "";

		[Required(ErrorMessage = "The price is required")]
		public decimal Price { get; set; }


		[Required(ErrorMessage = "The category  is required")]
		public string Category { get; set; } = "";

		public string? Description { get; set; } = "";


		public string ImageFileName { get; set; } = "";

		public IFormFile? ImageFile { get; set; }

		public string errorMessage = "";
		public string successMessage = "";

		private readonly string connectionString;

		private IWebHostEnvironment webHostEnvironment;
		public EditModel (IWebHostEnvironment env, IConfiguration configuration)
		{
			webHostEnvironment = env;
			connectionString = configuration.GetConnectionString("DefaultConnection");

        }
		public void OnGet()
        {
			string requestId = Request.Query["id"];


			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM products where id=@id";

					using(SqlCommand command = new SqlCommand(sql, connection)) 
					{
						command.Parameters.AddWithValue("id", requestId);
						using(SqlDataReader reader = command.ExecuteReader())
						{
							if(reader.Read())
							{
								Id = reader.GetInt32(0);
								ProductName = reader.GetString(1);
								Price = reader.GetDecimal(2);
								Category = reader.GetString(3);
								Description = reader.GetString(4);
								ImageFileName = reader.GetString(5);

							}
							else
							{
								Response.Redirect("/Admin/Products/Index");

							}
						}
					
					}


				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				Response.Redirect("/Admin/Products/Index");
			}

			
        }

		public void OnPost() 
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Data validation failed";
				return;

			}

			if (Description == null) Description = "";

			string newFileName = ImageFileName;
			if (ImageFile != null)
			{
				newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
				newFileName += Path.GetExtension(ImageFile.FileName);

				string imageFolder = webHostEnvironment.WebRootPath + "/images";
				string imageFullPath = Path.Combine(imageFolder, newFileName);

				using (var stream = System.IO.File.Create(imageFullPath))
				{
					ImageFile.CopyTo(stream);
				}

				//delete old image
				string oldImageFullPath = Path.Combine(imageFolder, ImageFileName);
				System.IO.File.Delete(oldImageFullPath);
				Console.WriteLine("Delete image " + oldImageFullPath);
			}


			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "UPDATE products SET product_name=@product_name, price=@price, category=@category, " +
						"description=@description, image_filename=@image_filename WHERE id=@id";
					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@product_name",	ProductName);
						command.Parameters.AddWithValue("@price", Price);
						command.Parameters.AddWithValue("@category", Category);
						command.Parameters.AddWithValue("@description", Description);
						command.Parameters.AddWithValue("@image_filename", newFileName);
						command.Parameters.AddWithValue("@id", Id);

						command.ExecuteNonQuery();	

					}
				}

			}
			catch (Exception ex) 
			{
				errorMessage = ex.Message;
				return;

			}
			successMessage = "Update successful";
			Response.Redirect("/Admin/Products/Index");
		}
		
    }
}
