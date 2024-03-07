using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static KD_Gadgets.Pages.Admin.Products.IndexModel;

namespace KD_Gadgets.Pages
{
    public class IndexModel : PageModel
	{
		public List<ProductInfo> productLists = new List<ProductInfo>();

        private readonly string connectionString;

        public IndexModel(IConfiguration configuration) 
        {

            connectionString = configuration.GetConnectionString("DefaultConnection");
        }



		public string search = "";
        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 6; //product displayed per page
        public void OnGet()
		{

            page = 1;

            search = Request.Query["search"];
            if (search == null) search = "";
            string requestPage = Request.Query["page"];


            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception ex)
                {
                    page = 1;
                }

            }


            try
            {
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
                    string sqlCount = "SELECT COUNT(*) FROM products";
                    if (search.Length > 0)
                    {
                        sqlCount += " WHERE product_name LIKE @search OR category LIKE @search";

                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");

                        decimal count = (int)command.ExecuteScalar();//storing the total numbers of order
                        totalPages = (int)Math.Ceiling(count / pageSize);

                    }


                    string sql = "SELECT * FROM products";
					if(search.Length > 0)
					{
						sql += " WHERE product_name LIKE @search OR category LIKE @search";

                    }

					sql += " ORDER by id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";


                    using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@search", "%" + search + "%");
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ProductInfo productInfo = new ProductInfo();

								productInfo.Id = reader.GetInt32(0);
								productInfo.ProductName = reader.GetString(1);
								productInfo.Price = reader.GetDecimal(2);
								productInfo.Category = reader.GetString(3);
								productInfo.Description = reader.GetString(4);
								productInfo.ImageFileName = reader.GetString(5);
								productInfo.CreatedAt=reader.GetDateTime(6).ToString("MM/dd/yyyy");

								productLists.Add(productInfo);
							}
						}
					}
				}
			}
			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message);			
			}
		}
	}
}