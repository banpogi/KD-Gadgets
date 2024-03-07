using KD_Gadgets.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KD_Gadgets.Pages.Admin.Users
{
	[RequireAuth(RequiredRole ="admin")]
    public class IndexModel : PageModel
    {

		public List<UserInfo> usersList = new List<UserInfo>();

		private readonly string connectionString;
		public IndexModel(IConfiguration configuration) 
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

        public void OnGet()
        {
			try
			{

				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM users";

					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								UserInfo userInfo = new UserInfo();
								userInfo.id = reader.GetInt32(0);
								userInfo.firstname = reader.GetString(1);
								userInfo.lastname = reader.GetString(2);
								userInfo.email = reader.GetString(3);
								userInfo.phone = reader.GetString(4);
								userInfo.address = reader.GetString(5);
								userInfo.password = reader.GetString(6);
								userInfo.role = reader.GetString(7);
								userInfo.createdAt = reader.GetDateTime(8).ToString("MM/dd/yyyy");

								usersList.Add(userInfo);
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
    }

    public class UserInfo
    {
		public int id;
		public string firstname;
		public string lastname;
		public string email;
		public string phone;
		public string address;
		public string password;
		public string role;
		public string createdAt;
	}
}
