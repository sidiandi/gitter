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
            var parts = path.Lineage.Select(_ => new Link { Name = _.GetDisplayName("Home"), Href = _.VirtualPath});
            return View(parts);
        }
    }
}
