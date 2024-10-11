using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;


public class CategoryOB
{
    [JsonPropertyName("cid")]
    public int cid { get; set; }
    [JsonPropertyName("name")]
    public string name { get; set; }
    public CategoryOB(int _cid, string _name)
    {
        cid = _cid;
        name = _name;
    }
}

public class Category
{
    [JsonPropertyName("cid")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }

}


public class CategoryList
{
    List<CategoryOB> CategoriesAPI = new List<CategoryOB>
            {
                new CategoryOB(1, "Beverages"),
                new CategoryOB(2, "Condiments"),
                new CategoryOB(3, "Confections")
            };

    public string GetCategories()
    {
        return JsonSerializer.Serialize(CategoriesAPI, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string GetCategoryByID(int id)
    {
        return JsonSerializer.Serialize(CategoriesAPI[id - 1], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }





    public int GetcategoryListCount()
    {
        return CategoriesAPI.Count;
    }



    public void UpdateCategoryByID(string name, int id)
    {
        CategoriesAPI[id - 1].name = name;
        Console.WriteLine($"cid: {CategoriesAPI[id - 1].cid} , name: {CategoriesAPI[id - 1].name}");

    }
}