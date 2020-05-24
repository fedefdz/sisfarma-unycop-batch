using System;
using System.Net.Http;

namespace Sisfarma.RestClient.WebClient.Extensions
{
    public static class RestRequestExtension
    {
        public static Request ToRestClientRequest(this HttpRequestMessage @this, Uri baseAddress)
        {
            string body = null;
            if(@this.Content != null)
                body = @this.Content.ReadAsStringAsync().Result;

            string url = $"{@this.RequestUri}";
            return new Request(url, @this.Method.ToString(), body);
        }
    }
}