using Newtonsoft.Json.Linq;

namespace PhiZone
{
    public class Response
    {
        public readonly long statusCode;
        public readonly JObject responsedData;

        public Response(long code, JObject data)
        {
            statusCode = code;
            responsedData = data;
        }
    }
}