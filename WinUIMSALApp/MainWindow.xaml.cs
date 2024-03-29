﻿using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;
using WinUIMSALApp.MSAL;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIMSALApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MSALClientHelper MSALClientHelper;

        private MSGraphHelper MSGraphHelper;

        public MainWindow()
        {
            InitializeComponent();

            // Using appsettings.json as our configuration settings and utilizing IOptions pattern - https://learn.microsoft.com/dotnet/core/extensions/options
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Read configuration
            AzureADConfig azureADConfig = configuration.GetSection("AzureAD").Get<AzureADConfig>();
            this.MSALClientHelper = new MSALClientHelper(azureADConfig);

            MSGraphApiConfig graphApiConfig = configuration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();
            this.MSGraphHelper = new MSGraphHelper(graphApiConfig, this.MSALClientHelper);
        }

        private async void SignInWithDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            await MSALClientHelper.InitializePublicClientAppAsync();

            await SignInTheUser();

        }

        private async void SignInWithBrokerButton_Click(object sender, RoutedEventArgs e)
        {
            IntPtr? _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            await MSALClientHelper.InitializePublicClientAppForWAMBrokerAsync(_windowHandle);

            await SignInTheUser();
        }

        private async Task SignInTheUser()
        {
            try
            {
                // Trigger sign-in and token acquisition flow
                await MSGraphHelper.SignInAndInitializeGraphServiceClient();

                DispatcherQueue.TryEnqueue(() =>
                {
                    ResultText.Text = "User has signed-in successfully";
                    TokenInfoText.Text = "Call Graph API";

                    SetButtonsVisibilityWhenSignedIn();
                });
            }
            catch (Exception ex)
            {
                ResultText.Text = ex.Message;
            }
        }

        /// <summary>
        /// Call MS Graph on behalf of the signed-in user and display content
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            // Call the /me endpoint of Graph
            User graphUser = await this.MSGraphHelper.GetMeAsync();

            // Go back to the UI thread to make changes to the UI
            DispatcherQueue.TryEnqueue(() =>
            {
                ResultText.Text = $"Current time: {DateTime.Now.ToString("HH:mm:ss")}" + "\nDisplay Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                                  + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                                  + "\nUser Principal Name: " + graphUser.UserPrincipalName;

                DisplayBasicTokenInfo(this.MSALClientHelper.AuthResult);

                this.SignOutButton.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = await this.MSALClientHelper.FetchSignedInUserFromCache();
                this.MSALClientHelper.SignOutUser(t);

                DispatcherQueue.TryEnqueue(() =>
                {
                    ResultText.Text = "User has signed-out";
                    TokenInfoText.Text = string.Empty;

                    SetButtonsVisibilityWhenSignedOut();
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing-out user: {ex.Message}";
            }
        }

        /// <summary>
        /// Display basic information contained in the token. Needs to be called from the UI thread.
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

        private void SetButtonsVisibilityWhenSignedIn()
        {
            this.CallGraphButton.Visibility = Visibility.Visible;
            this.SignOutButton.Visibility = Visibility.Visible;
            this.SignInWithBrokerButton.Visibility = Visibility.Collapsed;
            this.SignInWithDefaultButton.Visibility = Visibility.Collapsed;
        }

        private void SetButtonsVisibilityWhenSignedOut()
        {
            this.CallGraphButton.Visibility = Visibility.Collapsed;
            this.SignOutButton.Visibility = Visibility.Collapsed;
            this.SignInWithBrokerButton.Visibility = Visibility.Visible;
            this.SignInWithDefaultButton.Visibility = Visibility.Visible;
        }
    }
}