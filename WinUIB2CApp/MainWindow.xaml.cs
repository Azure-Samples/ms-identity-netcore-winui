// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using WinUIMSALAppB2C.MSAL;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIMSALAppB2C
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MSALClientHelper _msalClientHelper;
        private DownStreamApiHelper _downStreamApiHelper;

        public MainWindow()
        {
            InitializeComponent();

            // Using appsettings.json as our configuration settings and utilizing IOptions pattern - https://learn.microsoft.com/dotnet/core/extensions/options
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Read configuration
            AzureADB2CConfig azureADConfig = configuration.GetSection("AzureADB2C").Get<AzureADB2CConfig>();
            _msalClientHelper = new MSALClientHelper(azureADConfig);

            DownStreamApiConfig downStreamApiConfig = configuration.GetSection("DownstreamApi").Get<DownStreamApiConfig>();
            _downStreamApiHelper = new DownStreamApiHelper(downStreamApiConfig, _msalClientHelper);

        }
        private async void SignInWithDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            await _msalClientHelper.InitializeB2CTokenCacheAsync();

            await SignInTheUser();

        }

        private async Task SignInTheUser()
        {
            try
            {
                // Trigger sign-in and token acquisition flow
                await _downStreamApiHelper.SignInUserAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    ResultText.Text = "User has signed-in successfully";
                    TokenInfoText.Text = "Call B2C API";

                    SetButtonsVisibilityWhenSignedIn();
                });
            }
            catch (Exception ex)
            {
                ResultText.Text = ex.Message;
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _msalClientHelper.SignOutUserAccountAsync();

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
            SignOutButton.Visibility = Visibility.Visible;
            SignInWithDefaultButton.Visibility = Visibility.Collapsed;
        }

        private void SetButtonsVisibilityWhenSignedOut()
        {
            SignOutButton.Visibility = Visibility.Collapsed;
            SignInWithDefaultButton.Visibility = Visibility.Visible;
        }
    }
}
