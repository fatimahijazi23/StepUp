using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace aspteamWeb.Pages.Company
{
    public class ProfileModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;
        private readonly string connectionString = "Server=DESKTOP-81J6GVU\\SQLEXPRESS;Database=SetUp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        public ProfileModel(HttpClient httpClient, IWebHostEnvironment environment)
        {
            _httpClient = httpClient;
            _environment = environment;
        }

        [BindProperty]
        public CompanyViewModel Company { get; set; } = new();

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        public int FollowersCount { get; set; } = 0;
        public bool IsEditMode { get; set; } = false;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        // GET request
        public async Task<IActionResult> OnGetAsync(bool edit = false)
        {
            IsEditMode = edit;

            var userIdInt = HttpContext.Session.GetInt32("UserId");
            var companyIdInt = HttpContext.Session.GetInt32("CompanyId");
            var token = HttpContext.Session.GetString("Token");

            if (!userIdInt.HasValue)
            {
                return RedirectToPage("/Company/Login");
            }

            int userId = userIdInt.Value;
            int companyId = 0;

            if (!companyIdInt.HasValue)
            {
                companyId = GetCompanyIdFromDatabase(userId);

                if (companyId > 0)
                {
                    HttpContext.Session.SetInt32("CompanyId", companyId);
                }
                else
                {
                    ErrorMessage = "Company account not found.";
                    return Page();
                }
            }
            else
            {
                companyId = companyIdInt.Value;
            }

            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var profileUrl = $"https://localhost:7289/api/CompanyProfiles/{companyId}";
                var profileResponse = await _httpClient.GetAsync(profileUrl);

                if (profileResponse.IsSuccessStatusCode)
                {
                    var jsonString = await profileResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var profileData = JsonSerializer.Deserialize<CompanyProfileDTO>(jsonString, options);

                    if (profileData != null)
                    {
                        Company = new CompanyViewModel
                        {
                            Id = profileData.Id,
                            Name = profileData.Name ?? string.Empty,
                            Email = profileData.Email ?? string.Empty,
                            Industry = ConvertIndustryEnumToString(profileData.Industry),
                            CompanySize = ConvertCompanySizeIntToString(profileData.CompanySize),
                            About = profileData.About ?? string.Empty,
                            LogoUrl = profileData.LogoUrl ?? string.Empty
                        };
                    }
                }
                else
                {
                    var companyData = GetCompanyAccountFromDatabase(companyId);

                    if (companyData != null)
                    {
                        Company = new CompanyViewModel
                        {
                            Id = companyData.Id,
                            Name = companyData.CompanyName ?? string.Empty,
                            Email = companyData.Email ?? string.Empty,
                            Industry = ConvertIndustryEnumToString(companyData.Industry),
                            CompanySize = ConvertCompanySizeIntToString(companyData.CompanySize),
                            About = string.Empty,
                            LogoUrl = string.Empty
                        };

                        IsEditMode = true;
                    }
                }

                FollowersCount = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading profile: {ex.Message}";
            }

            return Page();
        }

        // POST request
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                IsEditMode = true;
                return Page();
            }

            var token = HttpContext.Session.GetString("Token");
            var companyIdInt = HttpContext.Session.GetInt32("CompanyId");

            if (!companyIdInt.HasValue)
            {
                var userIdInt = HttpContext.Session.GetInt32("UserId");
                if (userIdInt.HasValue)
                {
                    int companyId = GetCompanyIdFromDatabase(userIdInt.Value);
                    if (companyId > 0)
                    {
                        HttpContext.Session.SetInt32("CompanyId", companyId);
                        companyIdInt = companyId;
                    }
                }
            }

            if (!companyIdInt.HasValue)
            {
                ErrorMessage = "Company ID not found. Please logout and login again.";
                IsEditMode = true;
                return Page();
            }

            try
            {
                // ✅ Handle image upload FIRST
                string? logoUrl = Company.LogoUrl;
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    logoUrl = await SaveProfileImage(ProfileImage, companyIdInt.Value);

                    if (string.IsNullOrEmpty(logoUrl))
                    {
                        ErrorMessage = "Failed to upload image. Please try again.";
                        IsEditMode = true;
                        return Page();
                    }
                }

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var apiUrl = $"https://localhost:7289/api/CompanyProfiles/{companyIdInt.Value}";

                // ✅ FIX: Use correct property names that match your DTO
                var updateDto = new
                {
                    Id = companyIdInt.Value,
                    Name = Company.Name?.Trim(),           // ✅ Changed from CompanyName
                    Email = Company.Email?.Trim(),
                    Industry = ConvertIndustryToEnum(Company.Industry),
                    CompanySize = ConvertCompanySizeToInt(Company.CompanySize),
                    About = Company.About?.Trim(),
                    LogoUrl = logoUrl                      // ✅ Changed from ProfilePictureUrl
                };

                // ✅ Add logging to see what's being sent
                var jsonPayload = JsonSerializer.Serialize(updateDto);
                Console.WriteLine($"Sending to API: {jsonPayload}");

                var response = await _httpClient.PatchAsJsonAsync(apiUrl, updateDto);

                if (response.IsSuccessStatusCode)
                {
                    // Update CompanyName in session
                    HttpContext.Session.SetString("CompanyName", Company.Name?.Trim() ?? "");

                    SuccessMessage = "Company profile updated successfully!";
                    return RedirectToPage(new { edit = false });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update profile: {errorContent}";
                    Console.WriteLine($"API Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating profile: {ex.Message}";
                Console.WriteLine($"Exception: {ex.Message}\n{ex.StackTrace}");
            }

            IsEditMode = true;
            return Page();
        }

        // ✅ IMPROVED: Save uploaded image with better error handling
        private async Task<string> SaveProfileImage(IFormFile image, int companyId)
        {
            try
            {
                // Validate file size (5MB max)
                if (image.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image size must be less than 5MB.";
                    return string.Empty;
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ErrorMessage = "Only JPG, PNG, and GIF images are allowed.";
                    return string.Empty;
                }

                // Create uploads folder
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "company-logos");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = $"company_{companyId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Return relative URL
                var relativeUrl = $"/uploads/company-logos/{fileName}";
                Console.WriteLine($"Image saved successfully: {relativeUrl}");
                return relativeUrl;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error uploading image: {ex.Message}";
                Console.WriteLine($"Upload error: {ex.Message}\n{ex.StackTrace}");
                return string.Empty;
            }
        }

        private int GetCompanyIdFromDatabase(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT Id FROM CompanyAccounts WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            return 0;
        }

        private CompanyAccountData? GetCompanyAccountFromDatabase(int companyId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // ✅ Add Email to query (from Users table via JOIN)
                    string sql = @"
                        SELECT ca.Id, ca.CompanyName, u.Email, ca.Industry, ca.CompanySize 
                        FROM CompanyAccounts ca
                        INNER JOIN Users u ON ca.UserId = u.Id
                        WHERE ca.Id = @CompanyId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CompanyId", companyId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new CompanyAccountData
                                {
                                    Id = (int)reader["Id"],
                                    CompanyName = reader["CompanyName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Industry = (int)reader["Industry"],
                                    CompanySize = (int)reader["CompanySize"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            return null;
        }

        private string ConvertIndustryEnumToString(int industry)
        {
            return industry switch
            {
                0 => "Technology",
                1 => "Healthcare",
                2 => "Finance",
                3 => "Education",
                4 => "Manufacturing",
                5 => "Retail",
                6 => "Consulting",
                7 => "RealEstate",
                8 => "Other",
                _ => "Other"
            };
        }

        private int ConvertIndustryToEnum(string industry)
        {
            return industry switch
            {
                "Technology" => 0,
                "Healthcare" => 1,
                "Finance" => 2,
                "Education" => 3,
                "Manufacturing" => 4,
                "Retail" => 5,
                "Consulting" => 6,
                "RealEstate" => 7,
                "Other" => 8,
                _ => 8
            };
        }

        private string ConvertCompanySizeIntToString(int companySize)
        {
            return companySize switch
            {
                10 => "1-10",
                50 => "11-50",
                200 => "51-200",
                500 => "201-500",
                1000 => "500+",
                _ => "1-10"
            };
        }

        private int ConvertCompanySizeToInt(string companySize)
        {
            return companySize switch
            {
                "1-10" => 10,
                "11-50" => 50,
                "51-200" => 200,
                "201-500" => 500,
                "500+" => 1000,
                _ => 10
            };
        }

        public class CompanyAccountData
        {
            public int Id { get; set; }
            public string? CompanyName { get; set; }
            public string? Email { get; set; }
            public int Industry { get; set; }
            public int CompanySize { get; set; }
        }

        public class CompanyProfileDTO
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }

            [JsonConverter(typeof(FlexibleIntConverter))]
            public int Industry { get; set; }

            [JsonConverter(typeof(FlexibleIntConverter))]
            public int CompanySize { get; set; }

            public string? About { get; set; }
            public string? LogoUrl { get; set; }
        }

        public class FlexibleIntConverter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    if (int.TryParse(stringValue, out int result))
                        return result;

                    return stringValue switch
                    {
                        "Technology" => 0,
                        "Healthcare" => 1,
                        "Finance" => 2,
                        "Education" => 3,
                        "Manufacturing" => 4,
                        "Retail" => 5,
                        "Consulting" => 6,
                        "RealEstate" => 7,
                        "Other" => 8,
                        _ => 8
                    };
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt32();
                }
                return 0;
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        public class CompanyViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Company name is required.")]
            [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Industry is required.")]
            public string Industry { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Company size is required.")]
            public string CompanySize { get; set; } = string.Empty;

            [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
            public string About { get; set; } = string.Empty;

            public string LogoUrl { get; set; } = string.Empty;
        }
    }
}