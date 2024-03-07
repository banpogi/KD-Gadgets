using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Net;
using static KD_Gadgets.Pages.Admin.Products.IndexModel;

namespace KD_Gadgets.Pages
{
    [BindProperties]
    public class CartModel : PageModel

    {

		private readonly string connectionString;
		public CartModel(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = "";
        [Required]
        public string PaymentMethod { get; set; } = "";


		public List<OrderItem> listOrderItems = new List<OrderItem>();

        public decimal subTotal = 0;
		public decimal shippingFee = 5;
		public decimal total = 0;



		private Dictionary<String, int> getProductDictionary()
        {
            var productDictionary = new Dictionary<String, int>();

            //read the shopping cart item from cookie
            string cookieValue = Request.Cookies["shopping_cart"] ?? "";

            if (cookieValue.Length > 0)
            {
                string[] productIdArray = cookieValue.Split('-');
                
                for(int i = 0; i < productIdArray.Length; i++)
                {
                    string productId = productIdArray[i];
                    if(productDictionary.ContainsKey(productId))
                    {
                        productDictionary[productId] += 1;
                    }
                    else
                    {
                        productDictionary.Add(productId, 1);
                    }

                }

            }

            return productDictionary;
        }
		public void OnGet()
        {
            var productDictionary = getProductDictionary();

            //add, sub and delete item in the cart. ? means that the action can be null
            string? action = Request.Query["action"];
            string? id = Request.Query["id"];

            if (action != null && id != null && productDictionary.ContainsKey(id))
            {
                if (action.Equals("add"))
                {
                    productDictionary[id] += 1;
                }
                else if (action.Equals("sub"))
                {
                    if (productDictionary[id] > 1) productDictionary[id] -= 1;
                }
                else if (action.Equals("delete"))
                {
                    productDictionary.Remove(id);

                }

                //build the new cookie
                string newCookieValue = "";
                foreach (var keyValuePair in productDictionary)
                {
                    for (int i = 0; i < keyValuePair.Value; i++)
                    {
                        newCookieValue += "-" + keyValuePair.Key;
                    }
                }
                if (newCookieValue.Length > 0) newCookieValue = newCookieValue.Substring(1);

                var cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(365);
                cookieOptions.Path = "/";

                Response.Cookies.Append("shopping_cart", newCookieValue, cookieOptions);

                Response.Redirect(Request.Path.ToString());
                return;
			}

            try
            {

                using(SqlConnection connection = new SqlConnection(connectionString)) 
                {
                    connection.Open();

                    string sql = "SELECT * FROM products WHERE id=@id";
                    foreach(var keyValuePair in  productDictionary)
                    {
                        string productId = keyValuePair.Key;
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", productId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    OrderItem item = new OrderItem();

                                    item.productInfo.Id = reader.GetInt32(0);
                                    item.productInfo.ProductName = reader.GetString(1);
                                    item.productInfo.Price = reader.GetDecimal(2);
                                    item.productInfo.Category = reader.GetString(3);
                                    item.productInfo.Description = reader.GetString(4);
                                    item.productInfo.ImageFileName = reader.GetString(5);
                                    item.productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                    item.numCopies = keyValuePair.Value;
                                    item.totalPrice = item.numCopies * item.productInfo.Price;

                                    listOrderItems.Add(item);

                                    subTotal += item.totalPrice;
                                    total = subTotal + shippingFee;

                                }
                            }
                        }
                    }
                
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            Address = HttpContext.Session.GetString("address") ?? "";
        }

        public string successMessage = "";
        public string errorMessage = "";
        public void OnPost()
        {
            int client_id = HttpContext.Session.GetInt32("id") ?? 0;

            //check if the client id is valid
            if(client_id < 1) 
            {
                Response.Redirect("/Auth/Login");            
            }

            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }
            //Reading shopping cart items from cookie
            var productDictionary = getProductDictionary();

            if (productDictionary.Count < 1) 
            {
                errorMessage = "Your cart is empty";
                return;
                
            }

            //save the order to the database
            try
            {

                using(SqlConnection connection =  new SqlConnection(connectionString))           
                {
                    connection.Open();

                    //create a new order in the orders table
                    int newOrderId = 0;
                    string sqlOrder = "INSERT INTO orders (client_id, order_date, shipping_fee, " +
						"delivery_address, payment_method, payment_status, order_status) " +
						"OUTPUT INSERTED.id " +
						"VALUES (@client_id, CURRENT_TIMESTAMP, @shipping_fee, " +
						"@delivery_address, @payment_method, 'pending', 'created')";


                    using(SqlCommand command = new SqlCommand(sqlOrder, connection))
                    {
						command.Parameters.AddWithValue("@client_id", client_id);
						command.Parameters.AddWithValue("@shipping_fee", shippingFee);
						command.Parameters.AddWithValue("@delivery_address", Address);
						command.Parameters.AddWithValue("@payment_method", PaymentMethod);

                        newOrderId = (int)command.ExecuteScalar();

					}

					string sqlItem = "INSERT INTO order_items (order_id, product_id, quantity, unit_price) " +
					  "VALUES (@order_id, @product_id, @quantity, @unit_price)";


					foreach (var keyValuePair in productDictionary)
					{
						string productId = keyValuePair.Key;
						int quantity = keyValuePair.Value;
						decimal unitPrice = getProductPrice(productId);


						using (SqlCommand command = new SqlCommand(sqlItem, connection))
						{
							command.Parameters.AddWithValue("@order_id", newOrderId);
							command.Parameters.AddWithValue("@product_id", productId);
							command.Parameters.AddWithValue("@quantity", quantity);
							command.Parameters.AddWithValue("@unit_price", unitPrice);


							command.ExecuteNonQuery();
						}

					}

				}
			
			}
            catch(Exception ex) 
            {
                errorMessage = ex.Message;
                return;
            }

            //Delete the cookie "shopping_cart" from browser
            Response.Cookies.Delete("shopping_cart");

            successMessage = "Order created successfully";
        }


        private decimal getProductPrice(string productId)
        {
            decimal price = 0;

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT price FROM products where id=@id";

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
						command.Parameters.AddWithValue("@id", productId);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{

								price = reader.GetDecimal(0);
							}
						}
					}
				}   
            }
            catch(Exception ex)
            {

            }

           

            return price;
        }
    }

    public class OrderItem
    {
        public ProductInfo productInfo = new ProductInfo();
        public int numCopies = 0;
        public decimal totalPrice = 0;
    }    
}
