## How the code was created

<details>

<summary>Expand the section</summary>

 The current sample is based on [UWP sample](https://github.com/Azure-Samples/active-directory-dotnet-native-uwp-v2) in terms of UI and logical structure - it is similar for both.

 To build an initial project, you can use [WinUI 3 Templates for Visual Studio](https://learn.microsoft.com/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)

 It might be helpful to read [Initial project creation instructions](https://learn.microsoft.com/windows/apps/winui/winui3/create-your-first-winui3-app) as well.

### Adding configuration

To be able to use `appsettings.json` file similar to ASP.NET applications, install `Microsoft.Extensions.Configuration.Binder` and `Microsoft.Extensions.Configuration.Json`.
Create new file inside your client project - `appsettings.json`, with the following contents

```json
 {
  "AzureAAD": {
    "ClientId": "[Enter the Client Id (Application ID obtained from the Azure portal), e.g. ba74781c2-53c2-442a-97c2-3d60re42f403]",
    "TenantId": "[Enter 'common', or 'organizations' or the Tenant Id (Obtained from the Azure portal. Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs), e.g. da41245a5-11b3-996c-00a8-4d99re19f292]",
    "Authority": "https://login.microsoftonline.com/{0}",
    "MSGraphURL": "https://graph.microsoft.com/v1.0",
    "RedirectURL": "ms-appx-web://microsoft.aad.brokerplugin/{0}",
    "CacheFileName": "netcore_winui_cache.txt",
    "CacheDir": "C:/Temp",
    "Scopes": "user.read" //write multiple scopes separated by space, Ex: scope1 scope2 ...
  }
 }
```

The file will configure Authentication and Caching for the application. `ClientId` and `TenantId` keys will be updated either by you during manual App Registration setup or by `AppCreationScripts\Configure.ps1` if you choose automated setup.

After creating the configuration file add the below lines after call to `InitializeComponent();` inside `MainWindow.xaml.cs` file, `MainWindow()` method:

```csharp
 var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
 _winUiSettings = configuration.GetSection("AzureAAD").Get<WinUISettings>();
```

### Adding MSAL support and logging

[MSAL.NET for public clients](https://learn.microsoft.com/azure/active-directory/develop/msal-net-initializing-client-applications) is used to Authenticate with Azure AD to gain access token to MsGraph API. 
Install `Microsoft.Identity.Client.Extensions.Msal` package and create a Public Client application by adding the below code immediately after configuration lines:

```csharp
 _PublicClientApp = PublicClientApplicationBuilder.Create(_winUiSettings.ClientId)
  .WithAuthority(string.Format(_winUiSettings.Authority, _winUiSettings.TenantId))
  .WithRedirectUri(string.Format(_winUiSettings.RedirectURL, _winUiSettings.ClientId))
  .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false) 
  .Build();
```

Notice the `.WithLogging()` method is being called. You will have to implement `IIdentityLogger` interface in similar to how it was [implemented](https://github.com/Azure-Samples/ms-identity-netcore-winui/blob/main/WinUIMSALApp/Logging/IdentityLogger.cs) inside the current sample.

Refer to the [sample source code](https://github.com/Azure-Samples/ms-identity-netcore-winui/blob/f98f3170b3759e812fdd320cead851e2c73e15d5/WinUIMSALApp/MainWindow.xaml.cs#L56) for more information about the lines you've just added.

### Adding Token Cache

[Token Cache](https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache#configuring-the-token-cache) issued to enhance user experience by skipping authentication part if the user is logging-in within Access Token expiry period. Add the below code immediately after Public Client creation code:

```csharp
var storageProperties = new StorageCreationPropertiesBuilder(_winUiSettings.CacheFileName, _winUiSettings.CacheDir).Build();
Task.Run(async () => await MsalCacheHelper.CreateAsync(storageProperties)).Result.RegisterCache(_PublicClientApp.UserTokenCache);
```

At the end of [MainWindow()](https://github.com/Azure-Samples/ms-identity-netcore-winui/blob/f98f3170b3759e812fdd320cead851e2c73e15d5/WinUIMSALApp/MainWindow.xaml.cs#L47) method the application tries to obtain current user account and set Sign-in button text accordingly.

### User sign-In process

Before calling MsGraph API, user should authenticate. The process is started by calling `SignInAndInitializeGraphServiceClient()` method that will attempt to authenticate and create MSGraph client object. There are 2 ways to obtain the token:

- `AcquireTokenSilent()` - where the application tries to obtain the access token from token cache
- `AcquireTokenInteractive()` - in case of `AcquireTokenSilent()` failed with [MsalUiRequiredException](https://learn.microsoft.com/dotnet/api/microsoft.identity.client.msaluirequiredexception?view=azure-dotnet). In this case user will be offered to type their credentials into a standard authentication UI dialog box.

### Calling MsGraph API

To be able to call for the [MsGraph API](https://learn.microsoft.com/graph/use-the-api), the `Microsoft.Graph` package must be installed. Then the below code inside `CallGraphButton_Click()` method is called:

```csharp
 GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(_winUiSettings.Scopes.Split(' '));
 User graphUser = await graphClient.Me.Request().GetAsync();
```

### Additional code

Take a look into [MainWindow.xaml.cs](https://github.com/Azure-Samples/ms-identity-netcore-winui/blob/main/WinUIMSALApp/MainWindow.xaml.cs) and learn how Sign-in/Sign-out buttons callback functions are being used to call MsGraph API and manage user authentication state.

</details>
