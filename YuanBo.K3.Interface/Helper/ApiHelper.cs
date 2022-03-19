using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Helper
{
    public static class ApiHelper
    {

        /// <summary>
        /// 通用请求方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="datas"></param>
        /// <param name="method">POST GET PUT DELETE</param>
        /// <param name="contentType">"POST application/x-www-form-urlencoded; charset=UTF-8"</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpRequest(
            string url, 
            string data, 
            string method = "PUT", 
            string contentType = "application/json", 
            Encoding encoding = null)
        {
            byte[] datas = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);//data可以直接传字节类型 byte[] data,然后这一段就可以去掉
            if (encoding == null)
                encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            request.Timeout = 150000;
            request.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            Stream requestStream = null;
            string responseStr = null;
            try
            {
                if (datas != null)
                {
                    request.ContentLength = datas.Length;
                    requestStream = request.GetRequestStream();
                    requestStream.Write(datas, 0, datas.Length);
                    requestStream.Close();
                }
                else
                {
                    request.ContentLength = 0;
                }
                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    Stream getStream = webResponse.GetResponseStream();
                    byte[] outBytes = ReadFully(getStream);
                    getStream.Close();
                    responseStr = Encoding.UTF8.GetString(outBytes);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                requestStream = null;
            }
            return responseStr;
        }

        //带HTTP安全协议BasicAuth验证的post请求方法
        public static string Post_BasicAuthAsync(string url, string user, string secret)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                var postContent = new MultipartFormDataContent();
                

                // 创建身份认证
                AuthenticationHeaderValue authentication = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{secret}")
                    ));

                _httpClient.DefaultRequestHeaders.Authorization = authentication;

                var values = new[]
                {
                    new KeyValuePair<string, string>("grant_type","client_credentials"),
                    new KeyValuePair<string, string>("scope","write")
                 };

                postContent.Headers.Add("ContentType", $"multipart/form-data");
                postContent.Add(new StringContent("client_credentials"), "grant_type");
                postContent.Add(new StringContent("write"), "scope");

                HttpResponseMessage response
                    = _httpClient.PostAsync(url, postContent).Result;

               return response.Content.ReadAsStringAsync().Result;
 
            }
            catch (Exception s)
            {
                //返回错误信息
                return s.Message;
            }
        }


        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[512];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        public static string HttpPost(string url, string body)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string HttpPut(string url, string body)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 发送Get请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="dic">请求参数定义</param>
        /// <returns></returns>
        public static string HttpGet(string url, Dictionary<string, object> dic,out int count)
        {
            string result = "";
            StringBuilder builder = new StringBuilder();
            builder.Append(url);
            if (dic.Count > 0)
            {
                builder.Append("?");
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
            }
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
            //添加参数
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var heads = resp.Headers;
            count = Convert.ToInt32(heads["X-Total-Count"]);
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }
            return result;

        }
            /// <summary>
                    /// 秘钥：加密、解密（秘钥相同,注意保存）
                    /// </summary>
                    /// <returns></returns>
            private static CspParameters GetCspKey()
        {
            CspParameters param = new CspParameters
            {
                KeyContainerName = "chait"//密匙容器的名称，保持加密解密一致才能解密成功
            };
            return param;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="palinData">明文</param>
        /// <param name="encodingType">编码方式</param>
        /// <returns>密文</returns>
        public static string Encrypt(string palinData)
        {
            if (string.IsNullOrWhiteSpace(palinData)) return null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetCspKey()))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(palinData); //将要加密的字符串转换为指定编码的字节数组
                byte[] encryptData = rsa.Encrypt(bytes, false);//将加密后的字节数据转换为新的加密字节数组
                return Convert.ToBase64String(encryptData);//将加密后的字节数组转换为字符串
            }
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// 获取新的密码盐码
        /// </summary>
        /// <returns></returns>
        public static string GetPasswordSalt()
        {
            var salt = new byte[128 / 8];
            using (var saltnum = RandomNumberGenerator.Create())
            {
                saltnum.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }
    }
}
