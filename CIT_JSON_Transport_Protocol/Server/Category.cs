using System.Text.Json;
using System.Text.Json.Serialization;

public class Category
{
    [JsonPropertyName("cid")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class CategoryList
{
    List<Category> CategoriesAPI = new List<Category>
    {
        new Category { Id = 1, Name = "Beverages" },
        new Category { Id = 2, Name = "Condiments" },
        new Category { Id = 3, Name = "Confections" }
    };

    public bool CreateCategory(Category category)
    {
        try
        {
            category.Id = Random.Shared.Next(100, int.MaxValue);
            CategoriesAPI.Add(category);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public bool DeleteCategory(int id)
    {
        try
        {
            CategoriesAPI.RemoveAt(CategoriesAPI.FindIndex(x => x.Id == id));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetCategories()
    {
        return JsonSerializer.Serialize(CategoriesAPI, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string GetCategoryByID(int id)
    {
        return JsonSerializer.Serialize(CategoriesAPI[id - 1], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public int GetCategoryCount()
    {
        return CategoriesAPI.Count;
    }

    public bool UpdateCategoryById(int id, Category category)
    {
        try
        {
            CategoriesAPI[id - 1] = category;
            Console.WriteLine("We updating the category name: " + CategoriesAPI[id - 1].Name);
            return true;
        }
        catch
        {
            return false;
        }
    }
}