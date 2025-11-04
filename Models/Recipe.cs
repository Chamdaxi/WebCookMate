namespace demo.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Ingredients { get; set; } = new List<string>();
        public string? Category { get; set; }
    }
}

