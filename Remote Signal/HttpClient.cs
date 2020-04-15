using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Signal
{
    public static class HttpClient
    {
        #region Get
        async public static Task<byte[]> Get(string url, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new System.Net.Http.HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    client.MaxResponseContentBufferSize = 2_000_000; // 2MB

                    foreach (var item in addHeaders)
                        client.DefaultRequestHeaders.Add(item.name, item.val);

                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                            return null;

                        using (HttpContent content = response.Content)
                        {
                            var res = await content.ReadAsByteArrayAsync();
                            if (10 > res.Length)
                                return null;

                            return res;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Post
        async public static ValueTask<string> Post(string url, HttpContent data, Encoding encoding, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new System.Net.Http.HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    client.MaxResponseContentBufferSize = 2_000_000; // 2MB

                    if (addHeaders != null)
                    {
                        foreach (var item in addHeaders)
                            client.DefaultRequestHeaders.Add(item.name, item.val);
                    }

                    using (HttpResponseMessage response = await client.PostAsync(url, data))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                            return null;

                        using (HttpContent content = response.Content)
                        {
                            string res = encoding.GetString(await content.ReadAsByteArrayAsync());
                            if (string.IsNullOrWhiteSpace(res))
                                return null;

                            return res;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}
