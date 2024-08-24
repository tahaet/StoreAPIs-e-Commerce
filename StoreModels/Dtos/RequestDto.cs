using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static StoreUtility.SD;


namespace StoreModels.Dtos
{
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
        public string AccesssToken { get; set; }
        public ContentType ContentType { get; set; } = ContentType.Json;
    }
}
