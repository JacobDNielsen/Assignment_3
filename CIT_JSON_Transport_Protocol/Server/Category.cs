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


        category.Id = Random.Shared.Next(100, int.MaxValue); // Creates random ID, Random is generated with same seed throughout threads
        CategoriesAPI.Add(category);
        return true;

    }
    public bool DeleteCategory(int id)
    {
        try
        {
            CategoriesAPI.RemoveAt(GetCategoryIndex(id));
            return true;
        }
        catch //No current tests where catch happens, but still nice to have!
        {
            return false;

        }
    }

    //Returns all categories as JSON string
    public string GetCategories()
    {
        return JsonSerializer.Serialize(CategoriesAPI, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    //Returns specific category by ID as JSON string.
    public string GetCategoryByID(int id)
    {
        return JsonSerializer.Serialize(CategoriesAPI[GetCategoryIndex(id)], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public int GetCategoryCount()
    {
        return CategoriesAPI.Count;
    }

    //Updating category by replacing existing category object with new category object from parameter.
    public bool UpdateCategoryById(int id, Category category)
    {
        try
        {
            CategoriesAPI[GetCategoryIndex(id)] = category;
            return true;
        }
        catch
        {
            return false;
        }
    }

    int GetCategoryIndex(int id)
    {
        return CategoriesAPI.FindIndex(x => x.Id == id); //FindIndex returns the index of element in CategoriesAPI that has the Id equal to the provided id
    }

}