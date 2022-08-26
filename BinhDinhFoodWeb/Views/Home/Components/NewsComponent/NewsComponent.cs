﻿using BinhDinhFoodWeb.Intefaces;
using Microsoft.AspNetCore.Mvc;

namespace BinhDinhFoodWeb.Views.Home.Components.NewsComponent
{
    public class NewsComponent: ViewComponent
    {
        private readonly IProductRepository _repo;
        public NewsComponent(IProductRepository repo)
        {
            _repo = repo;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var obj = await _repo.GetAllProductsAsync();
            var obj1= obj.Take(2).ToList();
            return View("NewComponent", obj1);
        }
    }
}