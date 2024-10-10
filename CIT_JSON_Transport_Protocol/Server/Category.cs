using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;


public class Category
{


    List<object> CategoriesAPI = new List<object>
            {
                new {cid = 1, name = "Beverages"},
                new {cid = 2, name = "Condiments"},
                new {cid = 3, name = "Confections"}
            };

    public string GetCategories()
    {
        //return GetCategoriesToResponse(CategoriesAPI);
        return JsonSerializer.Serialize(CategoriesAPI, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }



    public string GetById(int id)
    {
        return "No one";
    }

}