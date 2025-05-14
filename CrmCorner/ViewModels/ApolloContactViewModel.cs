namespace CrmCorner.ViewModels
{
    using Newtonsoft.Json;

    public class ApolloContactViewModel
    {
        [JsonProperty("person_id")]
        public string PersonId { get; set; }  // ✅ Eklendi

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("organization_name")]
        public string CompanyName { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("headline")]
        public string Headline { get; set; }

        [JsonProperty("present_raw_address")]
        public string Location { get; set; }

        [JsonProperty("linkedin_url")]
        public string LinkedinUrl { get; set; }
    }


    public class ApolloOrganization
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
