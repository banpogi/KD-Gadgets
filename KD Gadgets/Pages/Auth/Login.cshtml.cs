using KD_Gadgets.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace KD_Gadgets.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class LoginModel : PageModel
    {

        private readonly string connectionString;
        public LoginModel(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        [Required(ErrorMessage = "The email is required"), EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "The password is required")]
        public string Password { get; set; } = "";


        public string errorMessage = "";
        public string successMessage = "";

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


            //connect to the database and check the user credentials if matched
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string firstname = reader.GetString(1);
                                string lastname = reader.GetString(2);
                                string email = reader.GetString(3);
                                string phone = reader.GetString(4);
                                string address = reader.GetString(5);
                                string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_at = reader.GetDateTime(8).ToString("MM/dd/yyyy");

                                //verify the password
                                var passwordHasher = new PasswordHasher<IdentityUser>();
                                var result = passwordHasher.VerifyHashedPassword(new IdentityUser(), hashedPassword,
                                    Password);

                                //if password is correct, we need to create the session
                                if (result == PasswordVerificationResult.Success ||
                                     result == PasswordVerificationResult.SuccessRehashNeeded)
                                {

                                    HttpContext.Session.SetInt32("id", id);
                                    HttpContext.Session.SetString("firstname", firstname);
                                    HttpContext.Session.SetString("lastname", lastname);
                                    HttpContext.Session.SetString("email", email);
                                    HttpContext.Session.SetString("phone", phone);
                                    HttpContext.Session.SetString("address", address);
                                    HttpContext.Session.SetString("role", role);
                                    HttpContext.Session.SetString("created_at", created_at);



                                    //once the user is authenticated redirect to home page
                                    Response.Redirect("/");
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;

            }

            //Wrong Email or password
            errorMessage = "Wrong Email or Password";

        }

    }
}
