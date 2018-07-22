using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public interface IPlantumlRenderer
    {
        Task<string> GetId(string content);
        Task<Stream> GetPng(string id);
        Task<Stream> GetPlantuml(string id);
    }
}
