using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

public class CategoryObj
{
    public int cid { get; set; }
    public string name { get; set; }
    public CategoryObj(int _cid, string _name)
    {
        cid = _cid;
        name = _name;
    }


}

public class Category
{
    public List<CategoryObj> CategoriesAPI = new List<CategoryObj>
            {

                new CategoryObj (1,"Beverages"),
                new CategoryObj (2, "Beverages"),
                new CategoryObj (3, "Beverages"),
            };

    public string GetCategories()
    {
        return JsonSerializer.Serialize(CategoriesAPI, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string GetCategoryByID(int id)
    {
        return JsonSerializer.Serialize(CategoriesAPI[id], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }


    public string GetById(int id)
    {
        return "No one";
    }
}