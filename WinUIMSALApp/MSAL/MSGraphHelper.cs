﻿using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WinUIMSALApp.MSAL
{
    /// <summary>
    /// Contains methods to initialize and call the various MS Graph SDK methods
    /// </summary>
    /// <autogeneratedoc />
    public class MSGraphHelper
    {
        public readonly MSGraphApiConfig MSGraphApiConfig;

        public MSALClientHelper MSALClient { get; }
        private GraphServiceClient _graphServiceClient;

        private string[] GraphScopes;
        private string MSGraphBaseUrl = "https://graph.microsoft.com/v1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphHelper"/> class.
        /// </summary>
        /// <param name="msalClientHelper">The MSALClientHelper helper instance.</param>
        /// <exception cref="System.ArgumentNullException">msalClientHelper</exception>
        /// <autogeneratedoc />
        public MSGraphHelper(MSALClientHelper msalClientHelper)
        {
            if (msalClientHelper == null)
            {
                throw new ArgumentNullException(nameof(msalClientHelper));
            }

            // Load configuration
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            this.MSGraphApiConfig = configuration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();

            this.MSALClient = msalClientHelper;
            this.GraphScopes = this.MSGraphApiConfig.ScopesArray;
            this.MSGraphBaseUrl = this.MSGraphApiConfig.MSGraphBaseUrl;
        }

        /// <summary>
        /// Calls the MS Graph /me endpoint
        /// </summary>
        /// <returns></returns>

        public async Task<User> GetMeAsync()
        {
            if (this._graphServiceClient == null)
            {
                await SignInAndInitializeGraphServiceClient();
            }

            User graphUser = null;

            // Call /me Api
            Debug.WriteLine(ConsoleColor.Yellow, $"GET {_graphServiceClient.Me.Request().RequestUrl}");
            try
            {
                graphUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                this._graphServiceClient = await SignInAndInitializeGraphServiceClientPostCAE(ex);

                // Call the /me endpoint of Graph again with a fresh token
                graphUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            return await _graphServiceClient.Me.Request().GetAsync();
        }

        /// <summary>
        /// Sign in user using MSAL and obtain a token for MS Graph
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            string token = await this.MSALClient.SignInUserAndAcquireAccessToken(this.GraphScopes);
            return await InitializeGraphServiceClientAsync(token);
        }

        /// <summary>
        /// Signs the in and initialize graph service client post a CAE event exception.
        /// </summary>
        /// <param name="ex">The Graph Service exception. Contains the header required to properly process a CAE event</param>
        /// <returns></returns>

        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClientPostCAE(ServiceException ex)
        {
            // Get challenge from response of Graph API
            var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

            string token = await this.MSALClient.SignInUserAndAcquireAccessToken(this.GraphScopes, claimChallenge);
            return await InitializeGraphServiceClientAsync(token);
        }

        /// <summary>
        /// Bootstraps the MS Graph SDK with the provided token and returns it for use
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A GraphServiceClient (MS Graph SDK) instance
        /// </returns>
        private async Task<GraphServiceClient> InitializeGraphServiceClientAsync(string token)
        {
            this._graphServiceClient = new GraphServiceClient(this.MSGraphBaseUrl,
                            new DelegateAuthenticationProvider(async (requestMessage) =>
                            {
                                // Don't try to sign-in if token was supplied as an input parameter
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await Task.FromResult(token));
                            }));

            return await Task.FromResult(this._graphServiceClient);
        }
    }
}