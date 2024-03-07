using KD_Gadgets.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Windows.Input;

namespace KD_Gadgets.Pages.Admin.Products
{

    [RequireAuth(RequiredRole ="admin")]
    public class IndexModel : PageModel
    {   
        private readonly string connectionString;
        public IndexModel(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<ProductInfo> productLists = new List<ProductInfo>();


        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 5; //orders per page
        public string search = "";


        public void OnGet()
        {
            search = Request.Query["search"];
            if (search == null) search = "";

            page = 1;
            string requestPage = Request.Query["page"];


            if(requestPage != null) {
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
                    if (search.Length > 0)
                    {
                        sql += " WHERE product_name LIKE @search OR category LIKE @search";

                    }

                    sql += " ORDER BY id DESC";
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
                                productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                productLists.Add(productInfo);


                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
         
        }

        public class ProductInfo
        {
            public int Id { get; set; }
            public string ProductName { get; set; } = "";
            public decimal Price { get; set; }
            public string Category { get; set; } = "";
            public string Description { get; set; } = "";
            public string ImageFileName { get; set; } = "";
            public string CreatedAt { get; set; } = "";

        }
    }
}