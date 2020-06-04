using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RS.Shared
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
            client.MaxResponseContentBufferSize = 5_000_000; // 5MB

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

        #region TakeLogin
        async public static Task<string> TakeLogin(string url, HttpContent data, List<(string name, string val)> addHeaders)
        {
            try
            {
                var clientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new System.Net.Http.HttpClient(clientHandler))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    foreach (var item in addHeaders)
                        client.DefaultRequestHeaders.Add(item.name, item.val);

                    using (HttpResponseMessage response = await client.PostAsync(url, data))
                    {
                        if (response.Headers.TryGetValues("Set-Cookie", out var cook))
                        {
                            StringBuilder result = new StringBuilder();
                            foreach (string line in cook)
                            {
                                if (string.IsNullOrWhiteSpace(line))
                                    continue;

                                result.Append($"{line} ");
                            }

                            if (result.Length > 0)
                                return result.ToString();
                        }
                    }
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region Location
        async public static ValueTask<string> Location(string url, bool encodeArgs, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                using (HttpResponseMessage response = await client(addHeaders).GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    string location = response.Headers?.Location?.ToString() ?? response.RequestMessage?.RequestUri?.ToString();
                    location = Uri.EscapeUriString(HttpUtility.UrlDecode(location ?? string.Empty));

                    if (encodeArgs)
                    {
                        var match = Regex.Match(location, "(\\?|&)([^=]+)=([^\\?&\n\r]+)");
                        while (match.Success)
                        {
                            string val = HttpUtility.UrlEncode(match.Groups[3].Value);
                            location = location.Replace($"={match.Groups[3].Value}", $"={val}");

                            match = match.NextMatch();
                        }
                    }

                    return string.IsNullOrWhiteSpace(location) ? null : location;
                }
            }
            catch { }

            return null;
        }
        #endregion
    }
}
