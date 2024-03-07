using KD_Gadgets.MyHelpers;
using KD_Gadgets.Pages.Admin.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KD_Gadgets.Pages.Client.Orders
{

    [RequireAuth(RequiredRole = "client")]
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
			int clientId = HttpContext.Session.GetInt32("id") ?? 0;	
			try
			{
				string requestPage = Request.Query["page"];
				page = int.Parse(requestPage);
			}
			catch (Exception ex)
			{
				page = 1;
			}

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sqlCountOrders = "SELECT COUNT(*) FROM orders WHERE client_id=@client_id";
					using (SqlCommand command = new SqlCommand(sqlCountOrders, connection))
					{
						command.Parameters.AddWithValue("@client_id", clientId);

						decimal count = (int)command.ExecuteScalar();
						totalPages = (int)Math.Ceiling(count / pageSize);
					}


					string sql = "SELECT * FROM orders WHERE client_id=@client_id ORDER BY id DESC";
					sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@client_id", clientId);
						command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
						command.Parameters.AddWithValue("@pageSize", pageSize);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
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
}
