using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using NetSuiteRestletConnector.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using OAuth;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NetSuiteRestletConnector.Activities
{
    [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_DisplayName))]
    [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_Description))]
    public class NetSuiteProcessRestlet : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_ConsumerKey_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_ConsumerKey_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ConsumerKey { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_ConsumerSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_ConsumerSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ConsumerSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_TokenID_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_TokenID_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> TokenID { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_TokenSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_TokenSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> TokenSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_AccountID_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_AccountID_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> AccountID { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_RestletURL_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_RestletURL_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> RestletURL { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_RequestType_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_RequestType_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> RequestType { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_RequestData_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_RequestData_Description))]
        [LocalizedCategory(nameof(Resources.Options_Category))]
        public InArgument<string> RequestData { get; set; }

        [LocalizedDisplayName(nameof(Resources.NetSuiteProcessRestlet_ResponseData_DisplayName))]
        [LocalizedDescription(nameof(Resources.NetSuiteProcessRestlet_ResponseData_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> ResponseData { get; set; }

        #endregion


        #region Constructors

        public NetSuiteProcessRestlet()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ConsumerKey == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ConsumerKey)));
            if (ConsumerSecret == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ConsumerSecret)));
            if (TokenID == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(TokenID)));
            if (TokenSecret == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(TokenSecret)));
            if (AccountID == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(AccountID)));
            if (RestletURL == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RestletURL)));
            if (RequestType == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RequestType)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            IList<String> BODY_REQUEST_TYPES = new List<String> { "POST", "PATCH", "PUT" };

            // Inputs
            var consumerKey = ConsumerKey.Get(context);
            var consumerSecret = ConsumerSecret.Get(context);
            var tokenID = TokenID.Get(context);
            var tokenSecret = TokenSecret.Get(context);
            var accountID = AccountID.Get(context);
            var restletURL = RestletURL.Get(context);
            var requestType = RequestType.Get(context);
            var requestData = new JObject();
            if (BODY_REQUEST_TYPES.Contains(requestType))
            {
                requestData = JObject.Parse(RequestData.Get(context));
            } 
                

            String responseString = "";
            Uri requestUrl = new Uri(restletURL);
            OAuthBase req = new OAuthBase();
            String timestamp = req.GenerateTimeStamp();
            String nonce = req.GenerateNonce();
            String norm1 = "";
            String norm2 = "";

            String signature = req.GenerateSignature(requestUrl, consumerKey, consumerSecret,
                tokenID, tokenSecret, requestType, timestamp, nonce, out norm1, out norm2);

            if (signature.Contains("+"))
            {
                signature = signature.Replace("+", "%2B");
            }

            String header = "Authorization: OAuth " +
            "oauth_signature=\"" + signature + "\"," +
            "oauth_version=\"1.0\"," +
            "oauth_nonce=\"" + nonce + "\"," +
            "oauth_signature_method=\"HMAC-SHA256\"," +
            "oauth_consumer_key=\"" + consumerKey + "\"," +
            "oauth_token=\"" + tokenID + "\"," +
            "oauth_timestamp=\"" + timestamp + "\"," +
            "realm=\"" + accountID + "\"";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.ContentType = "application/json";
            request.Method = requestType;
            request.Headers.Add(header);
            if (BODY_REQUEST_TYPES.Contains(requestType)) {
                //request.ContentLength = requestData.Length;
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestData);
                }
            }
            HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
            {
                responseString = streamReader.ReadToEnd();
            }

            httpResponse.Close();

            // Outputs
            return (ctx) => {
                ResponseData.Set(ctx, responseString);
            };
        }

        #endregion
    }
}

