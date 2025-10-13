// Models/CreateStoryRequest.cs
namespace NexusGram.Models
{
    public class CreateStoryRequest
    {
        public string? Caption { get; set; }
        public required IFormFile Image { get; set; }
    }
}