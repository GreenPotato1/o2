using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Com.O2Bionics.Chat.App.Tests.Models;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web;
using HtmlAgilityPack;
using JetBrains.Annotations;
using log4net;
using NUnit.Framework;

//TODO: p1. task-121. Use HttpHelper from dev.
//TODO: p3. task-121. HttpClient must have 1 instance.

namespace Com.O2Bionics.Chat.App.Tests.Utilities
{
    public static class ControllerClient
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ControllerClient));
        private const string RefererKey = "referer";
        private const string Schema = "https://";

        /// <summary>
        ///     Return two cookies and a token to call the controllers from the web application.
        /// </summary>
        [ItemNotNull]
        public static async Task<CookiesAndToken> Login(
            [NotNull] string server,
            [NotNull] string email,
            [NotNull] string password,
            bool shallSendToken = true)
        {
            var result = await Run(
                nameof(Login),
                async () =>
                    {
                        NameValidator.ValidateServerName(server);
                        if (string.IsNullOrEmpty(email))
                            throw new ArgumentNullException(nameof(email));
                        if (string.IsNullOrEmpty(password))
                            throw new ArgumentNullException(nameof(password));

                        var url = Schema + server;
                        var uri = new Uri(url);
                        var referer = url + LoginConstants.LoginPath;

                        var cookieAndToken = await FetchCookieAndToken(referer, uri);
                        var result1 = await CallLogin(referer, uri, cookieAndToken, email, password, shallSendToken);
                        return result1;
                    });
            return result;
        }

        private static async Task<KeyValuePair<string, string>> FetchCookieAndToken(
            [NotNull] string referer,
            [NotNull] Uri uri)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Add(RefererKey, referer);

                var response = await client.GetAsync(LoginConstants.LoginPath);
                var data = await response.Content.ReadAsStringAsync();
                if (HttpStatusCode.OK != response.StatusCode)
                    throw new Exception($"{referer} returned StatusCode={response.StatusCode}, data='{data}'.");
                if (string.IsNullOrEmpty(data))
                    throw new Exception($"{referer} returned an empty response, data='{data}'.");

                if (!response.Headers.TryGetValues("Set-Cookie", out var values) || null == values)
                    throw new Exception($"{referer} must have returned the cookie, data='{data}'.");

                var headerCookies = values.ToList();
                if (1 != headerCookies.Count)
                    throw new Exception($"{referer} returned {headerCookies.Count} cookies while expecting one, data='{data}'.");

                var cookie = ParseCookie(referer, headerCookies[0]);
                var formToken = FindToken(referer, data, "form");
                return new KeyValuePair<string, string>(cookie, formToken);
            }
        }

        [NotNull]
        private static string ParseCookie([NotNull] string referer, [NotNull] string raw)
        {
            if (string.IsNullOrEmpty(raw))
                throw new Exception($"{referer} returned an empty cookie.");

            var values = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (null != value && LoginConstants.TokenKey.Length + 1 < value.Length && value.StartsWith(LoginConstants.TokenKey))
                {
                    var result = value.Substring(LoginConstants.TokenKey.Length + 1);
                    if (string.IsNullOrEmpty(result))
                        throw new Exception($"{referer} returned an empty value cookie named '{LoginConstants.TokenKey}'.");

                    return result;
                }
            }

            throw new Exception($"{referer} must have returned a cookie named '{LoginConstants.TokenKey}'.");
        }

        [NotNull]
        private static string FindToken([NotNull] string referer, [NotNull] string html, [NotNull] string elementName)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentNullException(nameof(html));
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode.SelectSingleNode($".//{elementName}");
            if (null == node)
                throw new Exception($"{referer} must have returned the document with an element '{elementName}' tag. HTML='{html}'.");

            const string nameKey = "name";
            const string valueKey = "value";

            var inputs = node.Elements("input");
            var tokenNode = inputs.FirstOrDefault(
                e => null != e.Attributes && e.Attributes.Contains(nameKey) && LoginConstants.TokenKey == e.Attributes[nameKey].Value
                     && e.Attributes.Contains(valueKey));
            if (null == tokenNode)
                throw new Exception($"{referer} must have returned the document with an input named '{LoginConstants.TokenKey}'. HTML='{html}'.");

            var result = tokenNode.Attributes[valueKey].Value;
            if (string.IsNullOrEmpty(result))
                throw new Exception(
                    $"{referer} must have returned an element '{elementName}' with not empty '{LoginConstants.TokenKey}' value. HTML='{html}'.");

            return result;
        }

        [ItemNotNull]
        private static async Task<CookiesAndToken> CallLogin(
            [NotNull] string referer,
            [NotNull] Uri uri,
            KeyValuePair<string, string> cookieAndToken,
            [NotNull] string email,
            [NotNull] string password,
            bool shallSendToken = true)
        {
            var localDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            var form = new LoginViewModelWithToken
                {
                    __RequestVerificationToken = shallSendToken ? cookieAndToken.Value : null,
                    LocalDate = localDate,
                    Email = email,
                    Password = password,
                    RememberMe = false
                };
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(uri, new Cookie(LoginConstants.TokenKey, cookieAndToken.Key));

            using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler))
            using (var objectContent = form.CreateContent())
            {
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Add("origin", referer);
                client.DefaultRequestHeaders.Add(RefererKey, referer);

                var response = await client.PostAsync(LoginConstants.LoginPath, objectContent);
                var html = await response.Content.ReadAsStringAsync();
                if (HttpStatusCode.OK != response.StatusCode)
                    throw new PostException((int)response.StatusCode, $"{referer} returned HTML='{html}'.");

                var cookies = cookieContainer.GetCookies(uri);
                var appCookie = GetCookie(referer, cookies, LoginConstants.CookieName, html);
                var validationCookie = GetCookie(referer, cookies, LoginConstants.TokenKey, html);
                var validationToken = FindToken(referer, html, "body");
                var result = new CookiesAndToken(appCookie, validationCookie, validationToken);
                return result;
            }
        }

        [NotNull]
        private static string GetCookie(string referer, CookieCollection cookies, [NotNull] string cookieName, string html)
        {
            if (null == cookies)
                throw new ArgumentNullException(nameof(cookies));
            if (string.IsNullOrEmpty(cookieName))
                throw new ArgumentNullException(nameof(cookieName));

            var cookie = cookies[cookieName];
            if (null == cookie)
                throw new LoginFailedException($"{referer} must have returned a cookie named '{cookieName}', HTML='{html}'.");

            var result = cookie.Value;
            if (string.IsNullOrEmpty(result))
                throw new LoginFailedException($"{referer} must have returned not empty cookie named '{cookieName}', HTML='{html}'.");
            return result;
        }

        [ItemNotNull]
        public static async Task<GetUsersResult> GetUsers([NotNull] string server, [NotNull] CookiesAndToken cookiesAndToken)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentNullException(nameof(server));
            if (null == cookiesAndToken)
                throw new ArgumentNullException(nameof(cookiesAndToken));

            var result = await Run(
                nameof(GetUsers),
                async () =>
                    {
                        var url = Schema + server;
                        var uri = new Uri(url);
                        var cookieContainer = CreateCookieContainer(cookiesAndToken, uri);

                        using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                        using (var client = new HttpClient(handler))
                        {
                            client.BaseAddress = uri;

                            const string path = "/User/GetAll";
                            client.DefaultRequestHeaders.Add(RefererKey, path);

                            var response = await client.GetAsync(path);
                            var data = await response.Content.ReadAsStringAsync();
                            if (HttpStatusCode.OK != response.StatusCode)
                                throw new Exception($"{path} returned StatusCode={response.StatusCode}, data='{data}'.");

                            if (string.IsNullOrEmpty(data))
                                throw new Exception($"{path} returned an empty response.");

                            var result1 = data.JsonUnstringify2<GetUsersResult>();
                            if (null == result1)
                                throw new Exception($"{path} returned null, data='{data}'.");

                            return result1;
                        }
                    });
            return result;
        }

        [ItemNotNull]
        public static async Task<UpdateUserResult> UpdateUser(
            [NotNull] string server,
            [NotNull] CookiesAndToken cookiesAndToken,
            [NotNull] UserInfo user)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentNullException(nameof(server));
            if (null == cookiesAndToken)
                throw new ArgumentNullException(nameof(cookiesAndToken));
            if (null == user)
                throw new ArgumentNullException(nameof(user));

            var result = await Run(
                nameof(UpdateUser),
                async () =>
                    {
                        var url = Schema + server;
                        var uri = new Uri(url);
                        var cookieContainer = CreateCookieContainer(cookiesAndToken, uri);

                        using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                        using (var client = new HttpClient(handler))
                        using (var objectContent = user.CreateContent())
                        {
                            client.BaseAddress = uri;

                            const string path = "/User/Update";
                            client.DefaultRequestHeaders.Add(RefererKey, path);
                            client.DefaultRequestHeaders.Add(LoginConstants.TokenKey, cookiesAndToken.VerificationToken);

                            var response = await client.PostAsync(path, objectContent);
                            var data = await response.Content.ReadAsStringAsync();
                            if (HttpStatusCode.OK != response.StatusCode)
                                throw new Exception($"{path} returned StatusCode={response.StatusCode}, data='{data}'.");

                            if (string.IsNullOrEmpty(data))
                                throw new Exception($"{path} returned an empty response.");

                            var result1 = data.JsonUnstringify2<UpdateUserResult>();
                            if (null == result1)
                                throw new Exception($"{path} returned null, data='{data}'.");

                            return result1;
                        }
                    });
            return result;
        }

        [ItemCanBeNull]
        public static async Task<List<WidgetViewStatisticsEntry>> GetWidgetLoads(
            [NotNull] string server,
            [NotNull] CookiesAndToken cookiesAndToken,
            [NotNull] WidgetLoadRequest request,
            bool usePost = true,
            bool setToken = true)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentNullException(nameof(server));
            if (null == cookiesAndToken)
                throw new ArgumentNullException(nameof(cookiesAndToken));
            if (null == request)
                throw new ArgumentNullException(nameof(request));

            Assert.IsNotEmpty(request.BeginDateStr, nameof(request.BeginDateStr));
            Assert.IsNotEmpty(request.EndDateStr, nameof(request.EndDateStr));

            var result = await Run(
                nameof(GetWidgetLoads),
                async () =>
                    {
                        var uri = new Uri(Schema + server);
                        var cookieContainer = CreateCookieContainer(cookiesAndToken, uri);

                        using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                        using (var client = new HttpClient(handler))
                        using (var objectContent = request.CreateContent())
                        {
                            const string path = "/Widget/Loads";
                            client.BaseAddress = uri;
                            client.DefaultRequestHeaders.Add(RefererKey, path);
                            if (setToken)
                                client.DefaultRequestHeaders.Add(LoginConstants.TokenKey, cookiesAndToken.VerificationToken);

                            var query = path
                                        + "?" + nameof(request.BeginDateStr) + "=" + request.BeginDateStr
                                        + "&" + nameof(request.EndDateStr) + "=" + request.EndDateStr;

                            var response = usePost ? await client.PostAsync(path, objectContent) : await client.GetAsync(query);
                            var data = await response.Content.ReadAsStringAsync();
                            if (HttpStatusCode.OK != response.StatusCode)
                                throw new PostException(
                                    (int)response.StatusCode,
                                    $"{path} returned StatusCode={response.StatusCode}, data='{data}'.");

                            if (string.IsNullOrEmpty(data))
                                return null;

                            var result1 = data.JsonUnstringify2<List<WidgetViewStatisticsEntry>>();
                            return result1;
                        }
                    });
            return result;
        }

        /// <summary>
        /// Get the widget.
        /// </summary>
        [ItemNotNull]
        public static async Task<string> GetWidgetChatFrame(
            [NotNull] Uri widgetUri,
            [NotNull] string refererHost,
            [NotNull] string customerId,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            bool isDemoMode = false)
        {
            if (null == widgetUri)
                throw new ArgumentNullException(nameof(widgetUri));
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));
            if (string.IsNullOrEmpty(refererHost))
                throw new ArgumentNullException(nameof(refererHost));

            using (var client = new HttpClient())
            {
                client.BaseAddress = widgetUri;
                var path = TestConstants.ChatFramePathQuery + customerId;
                if (isDemoMode)
                    path += "&m=demo";

                client.DefaultRequestHeaders.Add("authority", refererHost);
                const string protocol = "https://";
                client.DefaultRequestHeaders.Add("referer", protocol + refererHost);

                using (var response = await client.GetAsync(path))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(
                        statusCode,
                        response.StatusCode,
                        $"{path} returned '{result}'.");

                    if (string.IsNullOrEmpty(result))
                        throw new Exception($"{path} returned an empty response.");
                    return result;
                }
            }
        }

        [NotNull]
        private static CookieContainer CreateCookieContainer(CookiesAndToken cookiesAndToken, Uri uri)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(uri, new Cookie(LoginConstants.TokenKey, cookiesAndToken.VerificationCookie));
            cookieContainer.Add(uri, new Cookie(LoginConstants.CookieName, cookiesAndToken.AppCookie));
            return cookieContainer;
        }

        private static async Task<T> Run<T>([NotNull] string name, [NotNull] Func<Task<T>> func)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (null == func)
                throw new ArgumentNullException(nameof(func));

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await func();
                return result;
            }
            finally
            {
                stopwatch.Stop();
                var report = $"{nameof(ControllerClient)}.{name} took {stopwatch.ElapsedMilliseconds} ms.";
                m_log.Info(report);
            }
        }
    }
}