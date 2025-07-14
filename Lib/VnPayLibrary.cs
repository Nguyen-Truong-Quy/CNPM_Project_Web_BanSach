using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CNPM_Project_web.Lib
{
    public class VnPayLibrary
    {
        private SortedList<string, string> requestData = new SortedList<string, string>(StringComparer.Ordinal);
        private SortedList<string, string> responseData = new SortedList<string, string>(StringComparer.Ordinal);

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                requestData.Add(key, value);
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                responseData.Add(key, value);
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in requestData)
            {
                data.Append(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
            }
            string queryString = data.ToString().TrimEnd('&');

            string signData = GetSignData(requestData);
            string secureHash = HashSHA256(vnp_HashSecret + signData);

            string fullUrl = baseUrl + "?" + queryString + "&vnp_SecureHash=" + secureHash;
            return fullUrl;
        }

        public bool ValidateSignature(System.Collections.Specialized.NameValueCollection inputData, string vnp_HashSecret)
        {
            foreach (string key in inputData.AllKeys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    AddResponseData(key, inputData[key]);
                }
            }

            string signData = GetSignData(responseData);
            string checksum = HashSHA256(vnp_HashSecret + signData);
            string vnpSecureHash = inputData["vnp_SecureHash"];

            return checksum.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetSignData(SortedList<string, string> data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in data)
            {
                sb.Append(kv.Key + "=" + kv.Value + "&");
            }
            return sb.ToString().TrimEnd('&');
        }

        public string HashSHA256(string input)
        {
            var hash = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(""));
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder result = new StringBuilder();
                foreach (var b in bytes)
                    result.Append(b.ToString("x2"));
                return result.ToString();
            }
        }
    }
}
