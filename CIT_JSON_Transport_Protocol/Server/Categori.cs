﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public class Categori
{

    private Dictionary<int, string> CategoriesAPI =
    new Dictionary<int, string>()
    {
        { 1, "Beverages" },
        { 2, "Condiments" },
        { 3, "Confections" }
    };

    public Dictionary<int, string> GetCategories()
    {
        return CategoriesAPI;
    }

}