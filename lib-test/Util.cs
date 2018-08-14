using hashstream.bitcoin_lib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace lib_test
{
#if WITH_LIVE_TEST_TEST
    static class Util
    {
        public static byte[] GetBlock(string hash)
        {
            return ((string)RpcQuery("http://10.10.0.1:8332", "user", "password", "getblock", new object[] { hash, 0 }).result).FromHex();
        }

        public static string GetBestBlockHash()
        {
            return RpcQuery("http://10.10.0.1:8332", "user", "password", "getbestblockhash", new object[0]).result;
        }

        internal class JsonRPC
        {
            [JsonProperty(PropertyName = "jsonrpc")]
            public string JsonRpc { get; set; } = "1.0";

            [JsonProperty(PropertyName = "method")]
            public string Method { get; set; }

            [JsonProperty(PropertyName = "params")]
            public object Params { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; } = 1;
        }

        public static dynamic RpcQuery(string url, string username, string password, string method, object[] param)
        {
            dynamic ret = null;

            var req_payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new JsonRPC()
            {
                Method = method,
                Params = param
            }));

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers.Add(HttpRequestHeader.Authorization, $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}");
            req.Method = "POST";
            req.ContentLength = req_payload.Length;
            req.ContentType = "text/plain";
            req.GetRequestStream().Write(req_payload, 0, req_payload.Length);

            var rsp = req.GetResponse();
            using(var sr = new StreamReader(rsp.GetResponseStream()))
            {
                ret = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
            }

            return ret;
        }
    }
#endif
}
