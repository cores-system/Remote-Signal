using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        #region Client
        private static System.Net.Http.HttpClient Client(List<(string name, string val)> addHeaders)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var client = new System.Net.Http.HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(8),       // Сервер ждет ответ 10 секунд, ждать ответ источника дольше 8 секунд нет смысла
                MaxResponseContentBufferSize = 5_000_000 // 5MB
            };
            
            if (addHeaders != null)
            {
                foreach (var item in addHeaders)
                    client.DefaultRequestHeaders.Add(item.name, item.val);
            }

            return client;
        }
        #endregion

        #region Get
        public static async ValueTask<byte[]> Get(string url, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                using (HttpResponseMessage response = await Client(addHeaders).GetAsync(url))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return Compression(await response.Content.ReadAsByteArrayAsync());
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region Post
        public static async ValueTask<byte[]> Post(string url, HttpContent data, List<(string name, string val)> addHeaders)
        {
            try
            {
                using (HttpResponseMessage response = await Client(addHeaders).PostAsync(url, data))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return Compression(await response.Content.ReadAsByteArrayAsync());
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region TakeLogin
        public static async Task<string> TakeLogin(string url, HttpContent data, List<(string name, string val)> addHeaders)
        {
            try
            {
                var clientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new System.Net.Http.HttpClient(clientHandler))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    foreach (var (name, val) in addHeaders)
                        client.DefaultRequestHeaders.Add(name, val);

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
        public static async ValueTask<string> Location(string url, bool encodeArgs, List<(string name, string val)> addHeaders = null)
        {
            try
            {
                using (var response = await Client(addHeaders).GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    var location = response.Headers?.Location?.ToString() ?? response.RequestMessage?.RequestUri?.ToString();
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


        #region Compression
        static byte[] Compression(byte[] source)
        {
            using (var sourceStream = new MemoryStream(source))
            {
                using (var targetStream = new MemoryStream())
                {
                    using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                        sourceStream.CopyTo(compressionStream);

                    return targetStream.ToArray();
                }
            }
        }
        #endregion
    }
}
