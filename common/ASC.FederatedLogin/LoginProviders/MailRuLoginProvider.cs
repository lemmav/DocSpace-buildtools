/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System.Security.Cryptography;

namespace ASC.FederatedLogin.LoginProviders
{
    [Scope]
    public class MailRuLoginProvider : BaseLoginProvider<MailRuLoginProvider>
    {
        public override string CodeUrl => "https://connect.mail.ru/oauth/authorize";
        public override string AccessTokenUrl => "https://connect.mail.ru/oauth/token";
        public override string ClientID => this["mailRuClientId"];
        public override string ClientSecret => this["mailRuClientSecret"];
        public override string RedirectUri => this["mailRuRedirectUrl"];

        private const string MailRuApiUrl = "http://www.appsmail.ru/platform/api";

        public MailRuLoginProvider() { }

        public MailRuLoginProvider(
            OAuth20TokenHelper oAuth20TokenHelper,
            TenantManager tenantManager,
            CoreBaseSettings coreBaseSettings,
            CoreSettings coreSettings,
            IConfiguration configuration,
            ICacheNotify<ConsumerCacheItem> cache,
            ConsumerFactory consumerFactory,
            Signature signature,
            InstanceCrypto instanceCrypto,
            string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(oAuth20TokenHelper, tenantManager, coreBaseSettings, coreSettings, configuration, cache, consumerFactory, signature, instanceCrypto, name, order, props, additional)
        {
        }

        public override LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params, IDictionary<string, string> additionalStateArgs)
        {
            try
            {
                var token = Auth(context, Scopes, out var redirect);

                if (redirect)
                {
                    return null;
                }

                if (token == null)
                {
                    throw new Exception("Login failed");
                }

                var uid = GetUid(token);

                return RequestProfile(token.AccessToken, uid);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(_signature, _instanceCrypto, ex);
            }
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }

        private LoginProfile RequestProfile(string accessToken, string uid)
        {
            var queryDictionary = new Dictionary<string, string>
                {
                    { "app_id", ClientID },
                    { "method", "users.getInfo" },
                    { "secure", "1" },
                    { "session_key", accessToken },
                    { "uids", uid },
                };

            var sortedKeys = queryDictionary.Keys.ToList();
            sortedKeys.Sort();

            using var md5 = MD5.Create();
            var mailruParams = string.Join("", sortedKeys.Select(key => key + "=" + queryDictionary[key]).ToList());
            var sig = string.Join("", md5.ComputeHash(Encoding.ASCII.GetBytes(mailruParams + ClientSecret)).Select(b => b.ToString("x2")));

            var mailRuProfile = RequestHelper.PerformRequest(
                MailRuApiUrl
                + "?" + string.Join("&", queryDictionary.Select(pair => pair.Key + "=" + HttpUtility.UrlEncode(pair.Value)))
                + "&sig=" + HttpUtility.UrlEncode(sig));
            var loginProfile = ProfileFromMailRu(mailRuProfile);

            return loginProfile;
        }

        private LoginProfile ProfileFromMailRu(string strProfile)
        {
            var jProfile = JArray.Parse(strProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var mailRuProfiles = jProfile.ToObject<List<MailRuProfile>>();
            if (mailRuProfiles.Count == 0) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile(_signature, _instanceCrypto)
            {
                EMail = mailRuProfiles[0].Email,
                Id = mailRuProfiles[0].Uid,
                FirstName = mailRuProfiles[0].FirstName,
                LastName = mailRuProfiles[0].LastName,

                Provider = ProviderConstants.MailRu,
            };

            return profile;
        }

        private class MailRuProfile
        {
            public string Uid { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        private static string GetUid(OAuth20Token token)
        {
            if (string.IsNullOrEmpty(token.OriginJson)) return null;
            var parser = JObject.Parse(token.OriginJson);

            return
                parser?.Value<string>("x_mailru_vid");
        }
    }
}