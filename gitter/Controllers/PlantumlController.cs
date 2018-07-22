using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gitter.Models;
using Microsoft.AspNetCore.Mvc;

namespace gitter
{
    public class PlantumlController : Controller
    {
        public async Task<IActionResult> Index([FromServices] IPlantumlRenderer plantumlRenderer, string filename)
        {
            filename = filename.ToLower();
            var ext = Path.GetExtension(filename);
            var id = Path.GetFileNameWithoutExtension(filename);

            if (ext.Equals(".png"))
            {
                var r = await plantumlRenderer.GetPng(id);
                {
                    return File(r, "image/png");
                }
            }
            else if (ext.Equals(".puml"))
            {
                var r = await plantumlRenderer.GetPlantuml(id);
                {
                    return File(r, "text/plain");
                }
            }

            return View();
        }
    }
}
