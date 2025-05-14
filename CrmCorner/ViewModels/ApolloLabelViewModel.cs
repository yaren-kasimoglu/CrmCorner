namespace CrmCorner.ViewModels
{
    using Newtonsoft.Json;

    public class ApolloLabelViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
