﻿using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Extensions.Configuration;
using WinUIMSALApp.Configuration;
using WinUIMSALApp.Logging;
using Microsoft.Identity.Client.Broker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIMSALApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Constant resources
        private static readonly string _buttonTextAuthorized = "Call Microsoft Graph API";

        // Inside WinUISettings object are the ClientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - The content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers  

        // As for the Tenant, you can use a name as obtained from the azure portal, e.g. kko365.onmicrosoft.com"
        private readonly WinUISettings _winUiSettings;

        private AuthenticationResult _authResult;
        private IAccount _currentUserAccount;

        // The MSAL Public client app
        private static IPublicClientApplication _publicClientApp;

        public MainWindow()
        {
            InitializeComponent();

            // Using appsettings.json as our configuration settings and utilizing IOptions pattern - https://learn.microsoft.com/dotnet/core/extensions/options
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _winUiSettings = configuration.GetSection("AzureAAD").Get<WinUISettings>();

            _currentUserAccount = Task.Run(async () => await InitializePublicClientAppAsync()).Result;

            if (_currentUserAccount != null)
            {
                this.CallGraphButton.Content = _buttonTextAuthorized;
                this.SignOutButton.Visibility = Visibility.Visible;
            }
        }

        private async Task<IAccount> InitializePublicClientAppAsync()
        {
            // Initialize the MSAL library by building a public client application
            _publicClientApp = PublicClientApplicationBuilder.Create(_winUiSettings.ClientId)
                .WithAuthority(string.Format(_winUiSettings.Authority, _winUiSettings.TenantId))
                // If not using this, it will fall back to older Uri: urn:ietf:wg:oauth:2.0:oob
                .WithRedirectUri(string.Format(_winUiSettings.RedirectURL, _winUiSettings.ClientId))
                // Using WAM - https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/wam#to-enable-wam-preview
                // .WithBrokerPreview(true)
                // .WithParentActivityOrWindow(() => { return WinRT.Interop.WindowNative.GetWindowHandle(this); })
                // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging
                .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false) // Set Identity Logging level to Warning which is a middle ground
                // Specify Window handle - required
                .WithClientCapabilities(new string[] { "cp1" }) // Client capabilities for CAE - https://learn.microsoft.com/azure/active-directory/develop/app-resilience-continuous-access-evaluation?tabs=dotnet
                .Build();

            // Cache configuration and hook-up to public application. Refer to https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache#configuring-the-token-cache
            var storageProperties = new StorageCreationPropertiesBuilder(_winUiSettings.CacheFileName, _winUiSettings.CacheDir).Build();
            var msalcachehelper = await MsalCacheHelper.CreateAsync(storageProperties);
            msalcachehelper.RegisterCache(_publicClientApp.UserTokenCache);

            var accounts = await _publicClientApp.GetAccountsAsync();
            return accounts.FirstOrDefault();
        }

        /// <summary>
        /// Call AcquireTokenAsync - to acquire a token requiring user to sign-in
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            GraphServiceClient graphClient = null;
            var scopes = _winUiSettings.Scopes.Split(' ');

            try
            {
                // Sign-in user using MSAL and obtain an access token for MS Graph
                graphClient = await SignInAndInitializeGraphServiceClient(scopes);
                // Call the /me endpoint of Graph
                User graphUser = await graphClient.Me.Request().GetAsync();

            }
            catch (MsalException msalEx)
            {
                DisplayMessage($"Error Acquiring Token:{Environment.NewLine}{msalEx}");
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                //**************************************************************
                // Handle a claims challenge produced by CAE by requesting a new access token with more claims
                //**************************************************************

                // Get challenge from response of Graph API
                var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

                // Use the challenge to obtain fresh token
                _authResult = await _publicClientApp.AcquireTokenInteractive(scopes).WithAccount(_currentUserAccount).WithClaims(claimChallenge).ExecuteAsync();

                // Sign-in user using MSAL and fresh token for MS Graph
                graphClient = await SignInAndInitializeGraphServiceClient(scopes, _authResult.AccessToken);
            }
            finally
            {
                if (graphClient == null)
                {
                    DisplayMessage($"Error Acquiring Token Silently");
                }
                else
                {
                    // Call the /me endpoint of Graph
                    User graphUser = await graphClient.Me.Request().GetAsync();

                    // Go back to the UI thread to make changes to the UI
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ResultText.Text = "Display Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                                          + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                                          + "\nUser Principal Name: " + graphUser.UserPrincipalName;
                        DisplayBasicTokenInfo(_authResult);
                        this.SignOutButton.Visibility = Visibility.Visible;
                        this.CallGraphButton.Content = _buttonTextAuthorized;
                    });
                }
            }
        }

        /// <summary>
        /// Signs in the user and obtains an Access token for MS Graph
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns> Access Token</returns>
        private async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            _currentUserAccount ??= (await _publicClientApp.GetAccountsAsync()).FirstOrDefault();

            try
            {
                _authResult = await _publicClientApp.AcquireTokenSilent(scopes, _currentUserAccount)
                    .ExecuteAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    this.CallGraphButton.Content = _buttonTextAuthorized;
                });

            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                // Must be called from UI thread
                _authResult = await _publicClientApp.AcquireTokenInteractive(scopes).ExecuteAsync();
            }

            return _authResult.AccessToken;
        }

        /// <summary>
        /// Sign in user using MSAL and obtain a token for MS Graph
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(string[] scopes, string token = null)
        {
            GraphServiceClient graphClient = new GraphServiceClient(_winUiSettings.MsGraphURL,
                            new DelegateAuthenticationProvider(async (requestMessage) =>
                            {
                                // Don't try to sign-in if toekn was supplied as an input parameter
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token ?? await SignInUserAndGetTokenUsingMSAL(scopes));
                            }));

            return await Task.FromResult(graphClient);
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await _publicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await _publicClientApp.RemoveAsync(firstAccount).ConfigureAwait(false);
                DispatcherQueue.TryEnqueue(() =>
                {
                    ResultText.Text = "User has signed-out";
                    TokenInfoText.Text = string.Empty;
                    this.CallGraphButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                    this.CallGraphButton.Content = $"Sign-In and {_buttonTextAuthorized}";
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing-out user: {ex.Message}";
            }
        }

        /// <summary>
        /// Display basic information contained in the token. Needs to be called from the UI thead.
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private void DisplayMessage(string message)
        {
            DispatcherQueue.TryEnqueue(() => { ResultText.Text = message; });
        }
    }
}
