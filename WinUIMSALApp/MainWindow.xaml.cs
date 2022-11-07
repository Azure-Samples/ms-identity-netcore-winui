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
        //Constant resources
        private static readonly string _buttonTextAuthorized = "Call Microsoft Graph API";

        private MSALClientHelper MSALClientHelper;

        private MSGraphHelper MSGraphHelper;

        public MainWindow()
        {
            InitializeComponent();

            this.MSALClientHelper = new MSALClientHelper();
            this.MSGraphHelper = new MSGraphHelper(this.MSALClientHelper);

            var cachedUserAccount = Task.Run(async () => await MSALClientHelper.InitializePublicClientAppAsync()).Result;

            if (cachedUserAccount != null)
            {
                this.CallGraphButton.Content = _buttonTextAuthorized;
                this.SignOutButton.Visibility = Visibility.Visible;
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
                this.CallGraphButton.Content = _buttonTextAuthorized;

                ResultText.Text = "Display Name: " + graphUser.DisplayName + "\nBusiness Phone: " + graphUser.BusinessPhones.FirstOrDefault()
                                  + "\nGiven Name: " + graphUser.GivenName + "\nid: " + graphUser.Id
                                  + "\nUser Principal Name: " + graphUser.UserPrincipalName;

                DisplayBasicTokenInfo(this.MSALClientHelper.AuthResult);

                this.SignOutButton.Visibility = Visibility.Visible;
                this.CallGraphButton.Content = _buttonTextAuthorized;
            });
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MSALClientHelper.SignOutUser(await this.MSALClientHelper.FetchSignedInUserFromCache());

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
    }
}