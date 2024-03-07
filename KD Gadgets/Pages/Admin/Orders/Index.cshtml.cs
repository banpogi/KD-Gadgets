using KD_Gadgets.MyHelpers;
using KD_Gadgets.Pages.Admin.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static KD_Gadgets.Pages.Admin.Products.IndexModel;


namespace KD_Gadgets.Pages.Admin.Orders
{
    [RequireAuth(RequiredRole = "admin") ]
    public class IndexModel : PageModel
    {
		private readonly string connectionString;

		public IndexModel(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}
        
        public List<OrderInfo> listOrders = new List<OrderInfo>();

        public int page = 1; //current html page
        public int totalPages = 0;
        private readonly int pageSize = 3; // number of orders per page

		public void OnGet()
        {
            try
            {
                string requestPage = Request.Query["page"];
                page = int.Parse(requestPage);
            }
            catch  (Exception ex)
            {
                page = 1;
            }     

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlCountOrders = "SELECT COUNT(*) FROM orders";
                    using (SqlCommand command = new SqlCommand(sqlCountOrders, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }
                    

                    string sql = "SELECT * FROM orders ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                OrderInfo orderInfo = new OrderInfo();

								orderInfo.id = reader.GetInt32(0);
								orderInfo.clientId = reader.GetInt32(1);
								orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
								orderInfo.shippingFee = reader.GetDecimal(3);
								orderInfo.deliveryAddress = reader.GetString(4);
								orderInfo.paymentMethod = reader.GetString(5);
								orderInfo.paymentStatus = reader.GetString(6);
								orderInfo.orderStatus = reader.GetString(7);

								orderInfo.items = OrderInfo.getOrderItems(orderInfo.id);

								listOrders.Add(orderInfo);

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

    public class OrderItemInfo
    {
        public int id;
        public int orderId;
        public int productId;
        public int quantity;
        public decimal unitPrice;

        public ProductInfo productInfo = new ProductInfo();
    }

    public class OrderInfo
    {

		


		public int id;
		public int clientId;
		public string orderDate;
		public decimal shippingFee;
		public string deliveryAddress;
		public string paymentMethod;
		public string paymentStatus;
		public string orderStatus;

        public List<OrderItemInfo> items = new List<OrderItemInfo>();



		public static List<OrderItemInfo> getOrderItems(int orderId)
        {
			List<OrderItemInfo> items = new List<OrderItemInfo>();

            try
            {

                string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=kdgadgets;Integrated Security=True";
				using (SqlConnection connection = new SqlConnection(connectionString)) 
                {
                    connection.Open();
                    string sql = "SELECT order_items.*, products.* FROM order_items, products " +
						"WHERE order_items.order_id=@order_id AND order_items.product_id = products.id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@order_id", orderId); 

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderItemInfo item = new OrderItemInfo();


								item.id = reader.GetInt32(0);
								item.orderId = reader.GetInt32(1);
								item.productId = reader.GetInt32(2);
								item.quantity = reader.GetInt32(3);
								item.unitPrice = reader.GetDecimal(4);

                                
                                item.productInfo.Id = reader.GetInt32(5);
                                item.productInfo.ProductName = reader.GetString(6);
                                item.productInfo.Price = reader.GetDecimal(7);
                                item.productInfo.Category   = reader.GetString(8);
                                item.productInfo.Description = reader.GetString(9);
                                item.productInfo.ImageFileName = reader.GetString(10);
                                item.productInfo.CreatedAt = reader.GetDateTime(11).ToString("MM/dd/yyyy");

                                items.Add(item);
							}
                        }
                    }


				}
            }
            catch(Exception ex) 
            {   
                Console.WriteLine(ex.Message);
            }


            return items;

		}

	}

}
