using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RemoteSignal
{
    public static class HttpClient
    {
        #region client
        static System.Net.Http.HttpClient client(List<(string name, string val)> addHeaders)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var client = new System.Net.Http.HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(8);        // Сервер ждет ответ 10 секунд, ждать ответ источника дольше 8 секунд нет смысла
            client.MaxResponseContentBufferSize = 1_000_000; // 1MB

            if (addHeaders != null)
            {
                foreach (var item in addHeaders)
                    client.DefaultRequestHeaders.Add(item.name, item.val);
            }

            return client;
        }
        #endregion

        #region Get
        async public static ValueTask<byte[]> Get(string url, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                using (HttpResponseMessage response = await client(addHeaders).GetAsync(url))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region Post
        async public static ValueTask<byte[]> Post(string url, HttpContent data, List<(string name, string val)> addHeaders)
        {
            try
            {
                using (HttpResponseMessage response = await client(addHeaders).PostAsync(url, data))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch { }

            return null;
        }
        #endregion
    }
}
