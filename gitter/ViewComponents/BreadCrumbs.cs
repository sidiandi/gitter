using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class BreadCrumbs : ViewComponent
    {
        public class Link
        {
            public string Name;
            public string Href;
        }

        public async Task<IViewComponentResult> InvokeAsync(ContentPath path)
        {
            var parts = new[] { new Link { Name = "Home", Href = "/" } }.ToList();
            var h = "";

            foreach (var i in path.Parts)
            {
                h = h + "/" + i;
                parts.Add(new Link { Name = i, Href = h });
            }

            return View(parts);
        }
    }
}
