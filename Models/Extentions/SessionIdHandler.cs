using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WebDtt.Models.Extentions
{
    public class SessionIdHandler : DelegatingHandler
    {
        public static string SessionIdToken = "studentExamId";
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Try to get the session ID from the request; otherwise create a new ID.
            var cookie = _getCookieValue(request);
            /*if (!string.IsNullOrEmpty(cookie))
            {
                request.Properties[SessionIdToken] = cookie;
            }*/
            

            

            // Continue processing the HTTP request.
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Set the session ID as a cookie in the response message.
            if (!string.IsNullOrEmpty(cookie))
            {
                response.Headers.AddCookies(new[]{
                    new CookieHeaderValue(SessionIdToken, cookie)
                });
            }
            return response;
        }

        private string _getCookieValue(HttpRequestMessage request)
        {
            var cookiez = request.Headers.GetValues("cookie").First();
            if (string.IsNullOrEmpty(cookiez)) return "";
            var arrCookies = cookiez.Split(';').FirstOrDefault(x => x.Contains(SessionIdToken));
            if (string.IsNullOrEmpty(arrCookies)) return "";
            var val = arrCookies.Remove(0, SessionIdToken.Length + 2);
            //Request.Headers.GetCookies(name).FirstOrDefault();
            //if (cookie == null) return string.Empty;
            return val;
        }
    }
}