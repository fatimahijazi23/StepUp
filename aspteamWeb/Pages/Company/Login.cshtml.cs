using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace aspteamWeb.Pages.Company
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        private readonly string connectionString = "Server=DESKTOP-81J6GVU\\SQLEXPRESS;Database=SetUp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and Password are required.";
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Role = 1 for Company (0 = JobSeeker, 1 = Company)
                    string sql = "SELECT Id, PasswordHash FROM Users WHERE Email = @Email AND Role = 1";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", Email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString();

                                // Verify password using the SAME method as your API
                                if (VerifyPassword(Password, storedHash))
                                {
                                    HttpContext.Session.SetString("UserEmail", Email);
                                    HttpContext.Session.SetInt32("UserId", (int)reader["Id"]);
                                    return RedirectToPage("/Company/CompanyDashboard");
                                }
                                else
                                {
                                    ErrorMessage = "Invalid email or password. Please check your credentials.";
                                    return Page();
                                }
                            }
                            else
                            {
                                ErrorMessage = "Invalid email or password. Please check your credentials.";
                                return Page();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        // EXACT SAME verification method from your API
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Split the stored hash to get salt and hash
                var parts = storedHash.Split(':');
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var hash = parts[1];

                // Hash the provided password with the same salt
                string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                ));

                // Compare the hashes
                return hash == computedHash;
            }
            catch
            {
                return false;
            }
        }
    }
}