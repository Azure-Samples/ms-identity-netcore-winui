## About the code

<details>
 <summary>Expand the section</summary>
For general information about how the project is organized, refer to the [tutorial](https://learn.microsoft.com/windows/apps/winui/winui3/create-your-first-winui3-app)

The constructor of `MainWindow` class was modified by adding a configuration, MSAL Authentication and token caching capability:

```csharp
   
    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    _winUiSettings = configuration.GetSection("AzureAAD").Get<WinUISettings>();

    
    _PublicClientApp = PublicClientApplicationBuilder.Create(_winUiSettings.ClientId)
        .WithAuthority(string.Format(_winUiSettings.Authority, _winUiSettings.TenantId))
        .WithRedirectUri(string.Format(_winUiSettings.RedirectURL, _winUiSettings.ClientId)) 
        .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false) 
        .Build();

    var storageProperties = new StorageCreationPropertiesBuilder(_winUiSettings.CacheFileName, _winUiSettings.CacheDir).Build();
    Task.Run(async () => await MsalCacheHelper.CreateAsync(storageProperties)).Result.RegisterCache(_PublicClientApp.UserTokenCache);
```

Every time a sign-in button is clicked and `CallGraphButton_Click` callback function is called, new `MsGraph` client is created by obtaining access token silently or interactively. Then a call to Graph API is done.

The access token is obtained inside `SignInUserAndGetTokenUsingMSAL` method, if there is no token cache available, then exception thrown and interactive session takes place, where a user should provide credentials and may also be asked to consent.

```csharp
    private async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            _currentUserAccount = _currentUserAccount ?? (await _PublicClientApp.GetAccountsAsync()).FirstOrDefault();
            try
            {
                _authResult = await _PublicClientApp.AcquireTokenSilent(scopes, _currentUserAccount).ExecuteAsync();
                DispatcherQueue.TryEnqueue(() =>
                {
                    this.CallGraphButton.Content = _buttonTextAuthorized;
                });
            }
            catch (MsalUiRequiredException ex)
            {
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                _authResult = await _PublicClientApp.AcquireTokenInteractive(scopes)
                                                  .ExecuteAsync();
            }
            return _authResult.AccessToken;
        }
```

To understand more how the buttons are linked to the callback functions, open `MainWindow.xaml` file and learn the below lines, notice Click properties:

```xml
    <Button x:Name="CallGraphButton" Content="Sign-In and Call Microsoft Graph API" HorizontalAlignment="Right" Padding="5" Click="CallGraphButton_Click" Margin="5" FontFamily="Segoe Ui"/>
    <Button x:Name="SignOutButton" Content="Sign-Out" HorizontalAlignment="Right" Padding="5" Click="SignOutButton_Click" Margin="5" Visibility="Collapsed" FontFamily="Segoe Ui"/>

```

### Using the Broker (WAM)

MSAL is also able to call [Web Account Manager](https://learn.microsoft.com/windows/uwp/security/web-account-manager), a Windows 10 component that ships with the OS. This component acts as an authentication broker and users of your app benefit from integration with accounts known from Windows, such as the account you signed-in with in your Windows session.

The constructor of `MainWindow` class can be modified further to utilize WAM for authentication by making the following changes to the code:

```csharp
   
    _publicClientApp = PublicClientApplicationBuilder.Create(_winUiSettings.ClientId)
        .WithAuthority(string.Format(_winUiSettings.Authority, _winUiSettings.TenantId))
        //if not using this, it will fall back to older Uri: urn:ietf:wg:oauth:2.0:oob
        .WithRedirectUri(string.Format(_winUiSettings.RedirectURL, _winUiSettings.ClientId))

        //Using WAM - https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/wam#to-enable-wam-preview
        .WithBrokerPreview(true)
        .WithParentActivityOrWindow(() => { return WinRT.Interop.WindowNative.GetWindowHandle(this); })
        
        //this is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging
        .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false) //set Identity Logging level to Warning which is a middle ground
        .Build();
```

Refer to [MSAL WAM](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/wam#to-enable-wam-preview) for more details on how to write code for this.
</details>
