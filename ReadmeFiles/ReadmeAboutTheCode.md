## About the code

<details>
 <summary>Expand the section</summary>

For general information about how the project is organized, refer to the [tutorial](https://learn.microsoft.com/windows/apps/winui/winui3/create-your-first-winui3-app)

The constructor of `MainWindow` class was modified by adding code to read configuration, and initialize MSAL and MS Graph helper classes :

```csharp
   
    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    // Read configuration
    AzureADConfig azureADConfig = configuration.GetSection("AzureAD").Get<AzureADConfig>();
    this.MSALClientHelper = new MSALClientHelper(azureADConfig);

    MSGraphApiConfig graphApiConfig = configuration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();
    this.MSGraphHelper = new MSGraphHelper(graphApiConfig, this.MSALClientHelper);
```

The following code in *MSALClientHelper.cs* initializes the MSAL's [PublicClientApplication](https://learn.microsoft.com/azure/active-directory/develop/msal-client-applications) from the various configuration settings

```csharp
    private void InitializePublicClientApplicationBuilder()
    {
        this.PublicClientApplicationBuilder = PublicClientApplicationBuilder.Create(AzureADConfig.ClientId)
            .WithAuthority(string.Format(AzureADConfig.Authority, AzureADConfig.TenantId))
            .WithRedirectUri(string.Format(AzureADConfig.RedirectURI, AzureADConfig.ClientId))      // Skipping this will make MSAL fall back to older Uri: urn:ietf:wg:oauth:2.0:oob
            .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)        // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
            .WithClientCapabilities(new string[] { "cp1" });                                        // declare this client app capable of receiving CAE events- https://aka.ms/clientcae
    }
```

Additionally the *MSALClientHelper.cs* has two methods that further prepare the MSAL's PublicClient instance with a [token cache](https://learn.microsoft.com/azure/active-directory/develop/msal-net-token-cache-serialization) to sign-in using the standard authentication flow.

```csharp

    public async Task<IAccount> InitializePublicClientAppAsync()
    {
        // Initialize the MSAL library by building a public client application
        this.PublicClientApplication = this.PublicClientApplicationBuilder.Build();

        await AttachTokenCache();
        return await FetchSignedInUserFromCache().ConfigureAwait(false);
    }
```

or using the WAM broker. This method is called when `SignInWithBrokerButton_Click` is pressed

```csharp
    public async Task<IAccount> InitializePublicClientAppForWAMBrokerAsync(IntPtr? handle)
    {
        // Initialize the MSAL library by building a public client application for authenticating using WAM
        this.PublicClientApplication = this.PublicClientApplicationBuilder
                .WithBrokerPreview(true)
                .WithParentActivityOrWindow(() => { return handle.Value; }) // Specify Window handle - (required for WAM).
                .Build();

        this.IsBrokerInitialized = true;

        await AttachTokenCache();
        return await FetchSignedInUserFromCache().ConfigureAwait(false);
    }        
```

finally the method `SignInTheUser()` takes care of signing-in the user and obtaining an Access Token for Microsoft Graph. if there is no tokens cached, an interactive authentication session takes place, where a user has to provide credentials and may also be asked to consent.

```csharp
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
```

When the `CallGraphButton_Click` function is called, new `MSGraphHelper` client is used to call the various Graph API. Then a call to Graph API is done.

The access token is obtained inside `SignInUserAndGetTokenUsingMSAL` method,

```csharp
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
```

To understand more how the buttons are linked to the callback functions, open `MainWindow.xaml` file and learn the below lines, notice Click properties:

```xml
    <Button x:Name="CallGraphButton" Content="Call Microsoft Graph API" HorizontalAlignment="Right" Padding="5" Click="CallGraphButton_Click" Margin="5" Visibility="Collapsed" FontFamily="Segoe Ui"/>
    <Button x:Name="SignInWithDefaultButton" Content="Sign-In" HorizontalAlignment="Left" Padding="10" Click="SignInWithDefaultButton_Click" Margin="5" FontFamily="Segoe Ui"/>
    <Button x:Name="SignInWithBrokerButton" Content="Sign-In with WAM Broker" HorizontalAlignment="Right" Padding="10" Click="SignInWithBrokerButton_Click" Margin="5" FontFamily="Segoe Ui"/>
    <Button x:Name="SignOutButton" Content="Sign-Out" HorizontalAlignment="Right" Padding="5" Click="SignOutButton_Click" Margin="5" Visibility="Collapsed" FontFamily="Segoe Ui"/>

```

### Using the Broker (WAM)

MSAL is also able to call [Web Account Manager](https://learn.microsoft.com/windows/uwp/security/web-account-manager), a Windows 10 component that ships with the OS. This component acts as an authentication broker and users of your app benefit from integration with accounts known from Windows, such as the account you signed-in with in your Windows session.

> Setting up a machine and its environment for WAM is a fairly involved task and beyond the scope of this code sample. We advise you work with your tenant administrators to ascertain of your organization's Azure AD tenant is set up for WAM and the device is joined to that Azure AD tenant. 

The methods `MSALClientHelper` class can be referenced to lean about WAM initialization:

```csharp
   
public async Task<IAccount> InitializePublicClientAppForWAMBrokerAsync(IntPtr? handle)
{
    // Initialize the MSAL library by building a public client application for authenticating using WAM
    this.PublicClientApplication = this.PublicClientApplicationBuilder
            .WithBrokerPreview(true)
            .WithParentActivityOrWindow(() => { return handle.Value; })// Specify Window handle - (required for WAM).
            .Build();

    this.IsBrokerInitialized = true;

    await AttachTokenCache();
    return await FetchSignedInUserFromCache().ConfigureAwait(false);
}
```

```csharp
    if (this.IsBrokerInitialized)
    {
        Console.WriteLine("No accounts found in the cache. Trying Window's default account.");

        this.AuthResult = await this.PublicClientApplication
            .AcquireTokenSilent(scopes, Microsoft.Identity.Client.PublicClientApplication.OperatingSystemAccount)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }
    else
    {
        this.AuthResult = await SignInUserInteractivelyAsync(scopes);
    }
```                    

Refer to [MSAL WAM](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/wam#to-enable-wam-preview) for more details on how to write code for this.

### Process the CAE challenge from Microsoft Graph

To process the CAE challenge from Microsoft Graph, the controller actions need to extract it from the `wwwAuthenticate` header. It is returned when MS Graph rejects a seemingly valid Access tokens for MS Graph. For this you need to:

1. To tell AAD that your app is ready to handle claim challenges, use `.WithClientCapabilities` method when creating Public Client app

   ```csharp
    .WithClientCapabilities(new string[] { "cp1" }) //client capabilities for CAE - https://learn.microsoft.com/azure/active-directory/develop/app-resilience-continuous-access-evaluation?tabs=dotnet
   ```

2. Catch `ServiceException` thrown by Graph API, try to obtain new token interactively, create Graph API client with the new token:

   ```csharp
     catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
        {
            //**************************************************************
            // Handle a claims challenge produced by CAE by requesting a new access token with more claims
            //**************************************************************

            // Get challenge from response of Graph API
            var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

            // Use the challenge to obtain fresh token
            _authResult = await _publicClientApp.AcquireTokenInteractive(scopes).WithClaims(claimChallenge).ExecuteAsync();

            // Sign-in user using MSAL and fresh token for MS Graph
            graphClient = await SignInAndInitializeGraphServiceClient(scopes, _authResult.AccessToken);
        }
   ```

</details>
