using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartz.Util;

namespace Servershot.Framework.Extentions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj)
        {
            var js = JsonSerializer.Create(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            var jw = new StringWriter();
            js.Serialize(jw, obj);
            return jw.ToString();
        }

    }
}
