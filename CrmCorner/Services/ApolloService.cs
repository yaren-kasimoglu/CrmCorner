using CrmCorner.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CrmCorner.Services
{
    public class ApolloService
    {
        private readonly HttpClient _httpClient;

        public ApolloService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ApolloUserDto>> GetContactsAsync(string apiKey) // direkt kullanıcının bilgisini çekiyo örn : yaren kasimoglu
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.apollo.io/v1/users/search"),
                Headers =
        {
            { "accept", "application/json" },
            { "Cache-Control", "no-cache" },
            { "x-api-key", apiKey },
        },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(body);

                var users = json.GetProperty("users").EnumerateArray().Select(user => new ApolloUserDto
                {
                    FirstName = user.GetProperty("first_name").GetString(),
                    LastName = user.GetProperty("last_name").GetString(),
                    Email = user.GetProperty("email").GetString(),
                    Title = user.GetProperty("title").GetString(),
                    CreatedAt = user.TryGetProperty("created_at", out var created)
                        ? DateTime.Parse(created.GetString())
                        : DateTime.MinValue
                }).ToList();

                return users;
            }
        }


        public async Task<List<ApolloListDto>> GetContactListsAsync(string token) //liste methodu - çalışmadı
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/contact_lists/search"),
                Content = new StringContent("{\"page\":1,\"per_page\":10}", System.Text.Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = new List<ApolloListDto>();

            foreach (var item in doc.RootElement.GetProperty("contact_lists").EnumerateArray())
            {
                result.Add(new ApolloListDto
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    ContactCount = item.GetProperty("contact_count").GetInt32(),
                    CreatedAt = item.GetProperty("created_at").GetDateTime()
                });
            }

            return result;
        }

        public async Task<ApolloPersonDto> GetPersonMatchAsync(string token, string firstName, string lastName, string company)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/people/match"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new
            {
                first_name = firstName,
                last_name = lastName,
                organization_name = company
            };

            string json = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(responseBody).RootElement;

            var person = root.GetProperty("person");

            return new ApolloPersonDto
            {
                FullName = person.GetProperty("name").GetString(),
                Title = person.GetProperty("title").GetString(),
                Email = person.GetProperty("email").GetString(),
                CreatedAt = person.TryGetProperty("created_at", out var created) ? DateTime.Parse(created.GetString()) : DateTime.MinValue
            };
        }

        public async Task<ApolloMatchedPersonDto> MatchPersonAsync(string token, string firstName, string lastName, string email)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/people/match"),
                Headers =
        {
            { "x-api-key", token },
            { "accept", "application/json" },
            { "Cache-Control", "no-cache" }
        },
                Content = new StringContent($@"
        {{
            ""first_name"": ""{firstName}"",
            ""last_name"": ""{lastName}"",
            ""email"": ""{email}""
        }}", System.Text.Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var person = json.RootElement.GetProperty("person");

            return new ApolloMatchedPersonDto
            {
                FirstName = person.GetProperty("first_name").GetString(),
                LastName = person.GetProperty("last_name").GetString(),
                Email = person.GetProperty("email").GetString(),
                Title = person.GetProperty("title").GetString(),
                LinkedinUrl = person.TryGetProperty("linkedin_url", out var l) ? l.GetString() : null,
                PhoneNumber = person.TryGetProperty("phone_number", out var p) ? p.GetString() : null,
                OrganizationName = person.TryGetProperty("organization_name", out var o) ? o.GetString() : null,
                Location = person.TryGetProperty("location_name", out var loc) ? loc.GetString() : null
            };

        }

        public async Task<List<ApolloPersonDto1>> SearchPeopleAsync(string apiKey) // belli şirkete göre çalışan getirme, çalışmadı
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/people/search"),
                Content = new StringContent(@"
        {
            ""q_organization_domains"": [""microsoft.com""],
            ""page"": 1,
            ""per_page"": 5
        }", System.Text.Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("accept", "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = new List<ApolloPersonDto1>();

            foreach (var item in doc.RootElement.GetProperty("people").EnumerateArray())
            {
                result.Add(new ApolloPersonDto1
                {
                    FirstName = item.GetProperty("first_name").GetString(),
                    LastName = item.GetProperty("last_name").GetString(),
                    Email = item.GetProperty("email").GetString(),
                    Title = item.GetProperty("title").GetString()
                });
            }

            return result;
        }

        public async Task<List<ApolloCompanyDto>> SearchAccountsAsync(string token, AccountSearchModel model)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/mixed_company/search"),
                Headers =
        {
            { "Authorization", $"Bearer {token}" },
            { "accept", "application/json" },
        },
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    q_organization_name = model.OrganizationName,
                    q_location = model.Location,
                    q_keywords = model.Industry,
                    page = 1,
                    per_page = 10
                }), System.Text.Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = new List<ApolloCompanyDto>();

            foreach (var item in doc.RootElement.GetProperty("organizations").EnumerateArray())
            {
                result.Add(new ApolloCompanyDto
                {
                    Name = item.GetProperty("name").GetString(),
                    Website = item.TryGetProperty("website_url", out var site) ? site.GetString() : "-",
                    Location = item.TryGetProperty("location_name", out var loc) ? loc.GetString() : "-",
                    Industry = item.TryGetProperty("industry", out var ind) ? ind.GetString() : "-"
                });
            }

            return result;
        }

        public async Task<List<ApolloCompanyResultDto>> SearchCompaniesAsync(string token, ApolloCompanySearchDto dto)
        {
            using var client = new HttpClient();

            var body = new
            {
                q_organization_names = string.IsNullOrWhiteSpace(dto.OrganizationName) ? null : new[] { dto.OrganizationName },
                q_organization_locations = string.IsNullOrWhiteSpace(dto.Location) ? null : new[] { dto.Location },
                q_organization_industry_tag_ids = string.IsNullOrWhiteSpace(dto.IndustryTagIds) ? null : new[] { dto.IndustryTagIds },
                page = 1,
                per_page = 10
            };

            var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.apollo.io/v1/mixed_company/search"),
                Headers =
        {
            { "Accept", "application/json" },
            { "Cache-Control", "no-cache" }
        },
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("organizations", out var orgArray))
            {
                throw new Exception("Apollo yanıtında 'organizations' alanı bulunamadı.");
            }

            var result = new List<ApolloCompanyResultDto>();

            foreach (var item in orgArray.EnumerateArray())
            {
                result.Add(new ApolloCompanyResultDto
                {
                    Name = item.GetProperty("name").GetString(),
                    WebsiteUrl = item.GetProperty("website_url").GetString(),
                    Location = item.GetProperty("location_name").GetString(),
                    Industry = item.GetProperty("industry_tag_name").GetString(),
                    EmployeeCount = item.TryGetProperty("estimated_num_employees", out var emp) ? emp.GetInt32() : 0,
                    CreatedAt = item.TryGetProperty("created_at", out var date) ? date.GetDateTime() : null
                });
            }

            return result;
        }


        public async Task<string> SearchPeopleWithSessionTokenAsync()
        {
            var token = "eyJfcmFpbHMiOnsibWVzc2FnZSI6IklqWTNZelJrTmpOa05UWTVZV1EzTURBeFpEYzNPREUxWTE5aE5XUTJNekU0Tm1RME9HWTFPREkyTnpWbE5qTTRNREV4WXpOa1pUbGxaQ0k9IiwiZXhwIjoiMjAyNS0wNS0xNlQxMzo1NjowNy4yNTVaIiwicHVyIjoiY29va2llLnJlbWVtYmVyX3Rva2VuX2xlYWRnZW5pZV92MiJ9fQ==";

            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://app.apollo.io/api/v1/people/search"),
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    page = 1,
                    per_page = 5,
                    q_keywords = "Marketing Manager"
                }), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-csrf-token", token);
            request.Headers.Add("Cookie", $"remember_token_leadgenie_v2={token}");
            request.Headers.Add("Accept", "*/*");

            var response = await client.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            return $"Status: {response.StatusCode}\n\n{responseText}";
        }



    }

    public class ApolloCompanySearchDto
    {
        public string OrganizationName { get; set; }
        public string Location { get; set; }
        public string IndustryTagIds { get; set; } // bu optional olabilir
    }
    public class ApolloCompanyResultDto
    {
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string Location { get; set; }
        public string Industry { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


    public class ApolloCompanyDto
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public string Industry { get; set; }
    }

    public class ApolloPersonDto
    {
        public string FullName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApolloPersonDto1
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
    }



    public class ApolloUserResponse
    {
        public List<ApolloUser> Users { get; set; }
    }

    public class ApolloUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ApolloUser
    {
        public string Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        // İhtiyacınıza göre diğer alanları da ekleyebilirsiniz
    }

    public class ApolloListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int ContactCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApolloMatchedPersonDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }

        public string LinkedinUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Seniority { get; set; }
        public string OrganizationName { get; set; }
        public string Location { get; set; }
    }


}
