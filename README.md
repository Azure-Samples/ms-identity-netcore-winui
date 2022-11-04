---
page_type: sample
name: Authenticate users with MSAL.NET in a WinUI desktop application 
description: This sample demonstrates how to use the [Microsoft Authentication Library (MSAL) for .NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to get an access token and call the Microsoft Graph using the MS Graph SDK from a WinUI application.
languages:
 -  csharp
products:
 - azure-active-directory
 - msal-net
 - Windows
 - WinUI
urlFragment: ms-identity-netcore-winui
extensions:
- services: ms-identity
- platform: WinUI
- endpoint: AAD v2.0
- level: 100
- client: WinUI Desktop app
- service: 
---

# Authenticate users with MSAL.NET in a WinUI desktop application 

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=XXX)

* [Overview](#overview)
* [Scenario](#scenario)
* [Prerequisites](#prerequisites)
* [Setup the sample](#setup-the-sample)
* [Explore the sample](#explore-the-sample)
* [Using Web Account Manager (WAM)](#using-web-account-manager-(wam))
* [Troubleshooting](#troubleshooting)
* [About the code](#about-the-code)
* [How the code was created](#how-the-code-was-created)
* [Contributing](#contributing)
* [Learn more](#learn-more)

## Overview

This sample demonstrates a WinUI Desktop app that authenticates users against Azure AD.

## Scenario

This sample demonstrates a WinUI Desktop app that authenticates users against Azure AD.

1. The client WinUI Desktop app uses the [MSAL.NET](https://aka.ms/msal-net) to sign-in a user and obtain a JWT [ID Token](https://aka.ms/id-tokens) from **Azure AD**.
1. The **ID Token** proves that the user has successfully authenticated against **Azure AD**.

![Scenario Image](./ReadmeFiles/topology.png)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**. If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.
* [Windows App SDK C# VS2022 Templates](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)


## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/ms-identity-netcore-winui.git
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.


### Step 3: Register the sample application(s) in your tenant

There is one project in this sample. To register it, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process -in PowerShell, run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
       ```

    > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md). The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

#### Choose the Azure AD tenant where you want to create your applications

To manually register the apps, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the client app (WinUI-App-Calling-MSGraph)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WinUI-App-Calling-MSGraph`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Authentication** blade to the left.
1. If you don't have a platform added, select **Add a platform** and select the **Public client (mobile & desktop)** option.
    1. In the **Redirect URIs** section, add **ms-appx-web://microsoft.aad.brokerplugin/{ClientId}**.
        The **ClientId** is the Id of the App Registration and can be found under **Overview/Application (client) ID**
    1. Click **Save** to save your changes.
1. Since this app signs-in users, we will now proceed to select **delegated permissions**, which is is required by apps signing-in users.
    1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs:
    1. Select the **Add a permission** button and then:
    1. Ensure that the **Microsoft APIs** tab is selected.
    1. In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
      * Since this app signs-in users, we will now proceed to select **delegated permissions**, which is requested by apps that signs-in users.
      * In the **Delegated permissions** section, select **User.Read** in the list. Use the search box if necessary.
    1. Select the **Add permissions** button at the bottom.

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **ID**.
    1. Select the optional claim **acct**.
    > Provides user's account status in tenant. If the user is a **member** of the tenant, the value is *0*. If they're a **guest**, the value is *1*.
    1. Select **Add** to save your changes.

##### Configure the client app (WinUI-App-Calling-MSGraph) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `WinUIMSALApp\appsettings.json` file.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant/directory ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WinUI-App-Calling-MSGraph` app copied from the Azure portal.

### Step 4: Running the sample

    Open the solution in Visual Studio and start it by pressing F5 to debug or Ctrl+F5 without debug.

## Explore the sample

<details>
 <summary>Expand the section</summary>

  Start running the sample by pressing `WinUIMSALApp (Package)` button on Visual Studio menu bar.

  No information is displayed because you're not logged in.
  Click `Sign-In and Call Microsoft Graph API` button.

  The UI similar to Web Browser will be displayed and give you a chance to select a user and login. You might be asked to consent to access your data on Graph API.

  Immediately after the UI will display basic user information as it is inside Graph API and some Token Info

  Click `Sign-Out` button -> all the information is deleted and `User has signed-out` message is shown.

  Look at Token Cache folder configured inside `appsettings.json` and find a file with binary information.

  If you sign-out and exit the application, then you will have to sign-in again when starting the UI.

  In case you were signed-in and just closed the UI, next time you will start the UI, you will be already signed-in - this is because token cache file is present and it will let you in until token expiration time is passed...
  The token expiration date/time is shown in `Token Info` in the UI.

  **So make sure to sign-out every time before closing the UI.**

  [Azure AD code sample survey - A .NET Core WinUI application that signs-in users and calls Microsoft Graph](https://forms.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUN0Q5NkFVUFBDVTZTNkhSUkEzUk9aM0szQiQlQCN0PWcu)

## Using Web Account Manager (WAM)

MSAL is able to call [Web Account Manager](https://learn.microsoft.com/windows/uwp/security/web-account-manager), a Windows 10 component that ships with the OS. This component acts as an authentication broker and users of your app benefit from integration with accounts known from Windows, such as the account you signed-in with in your Windows session.

### WAM value proposition

Using an authentication broker such as WAM has numerous benefits.

* Enhanced security (your app doesn't have to manage the powerful refresh token)
* Better support for Windows Hello, Conditional Access and FIDO keys
* Integration with Windows' "Email and Accounts" view
* Better Single Sign-On (users don't have to reenter passwords)
* Most bug fixes and enhancements will be shipped with Windows

### WAM limitations

* B2C and ADFS authorities aren't supported. MSAL will fall back to a browser.
* Available on Win10+ and Win Server 2019+. On Mac, Linux, and earlier versions of Windows, MSAL will fall back to a browser.
* Not available on Xbox.

</details>

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>
 
### "Either the user canceled the authentication or the WAM Account Picker crashed because the app is running in an elevated process" error message

When an app that uses MSAL is run as an elevated process, some of these calls within WAM may fail due to different process security levels. Internally MSAL.NET uses native Windows methods ([COM](/windows/win32/com/the-component-object-model)) to integrate with WAM. Starting with version 4.32.0, MSAL will display a descriptive error message when it detects that the app process is elevated and WAM returned no accounts.

One solution is to not run the app as elevated, if possible. Another solution is for the app developer to call `WindowsNativeUtils.InitializeProcessSecurity` method when the app starts up. This will set the security of the processes used by WAM to the same levels. See [this sample app](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/tests/devapps/WAM/NetCoreWinFormsWam/Program.cs#L18-L21) for an example. However, note, that this solution isn't guaranteed to succeed to due external factors like the underlying CLR behavior. In that case, an `MsalClientException` will be thrown. For more information, see issue [#2560](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/2560).

### "WAM Account Picker did not return an account" error message

This message indicates that either the application user closed the dialog that displays accounts, or the dialog itself crashed. A crash might occur if AccountsControl, a Windows control, is registered incorrectly in Windows. To resolve this issue:

1. In the taskbar, right-click **Start**, and then select **Windows PowerShell (Admin)**.
1. If you're prompted by a User Account Control (UAC) dialog, select **Yes** to start PowerShell.
1. Copy and then run the following script:

   ```powershell
   if (-not (Get-AppxPackage Microsoft.AccountsControl)) { Add-AppxPackage -Register "$env:windir\SystemApps\Microsoft.AccountsControl_cw5n1h2txyewy\AppxManifest.xml" -DisableDevelopmentMode -ForceApplicationShutdown } Get-AppxPackage Microsoft.AccountsControl

> * Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `winui` `ms-identity` `adal` `msal`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../../../issues).
</details>

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
        // If not using this, it will fall back to older Uri: urn:ietf:wg:oauth:2.0:oob
        .WithRedirectUri(string.Format(_winUiSettings.RedirectURL, _winUiSettings.ClientId))

        // Using WAM - https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/wam#to-enable-wam-preview
        .WithBrokerPreview(true)
        
        // Specify a Window handle - required
        .WithParentActivityOrWindow(() => { return WinRT.Interop.WindowNative.GetWindowHandle(this); })
        
        // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging
        .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false) //set Identity Logging level to Warning which is a middle ground
        .Build();
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

[MSAL.NET for public clients](https://learn.microsoft.com/azure/active-directory/develop/msal-net-initializing-client-applications) is used to Authenticate with Azure AD to gain access token to MSGraph API. 
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

Before calling MSGraph API, user should authenticate. The process is started by calling `SignInAndInitializeGraphServiceClient()` method that will attempt to authenticate and create MSGraph client object. There are 2 ways to obtain the token:

- `AcquireTokenSilent()` - where the application tries to obtain the access token from token cache
- `AcquireTokenInteractive()` - in case of `AcquireTokenSilent()` failed with [MsalUiRequiredException](https://learn.microsoft.com/dotnet/api/microsoft.identity.client.msaluirequiredexception?view=azure-dotnet). In this case user will be offered to type their credentials into a standard authentication UI dialog box.

### Calling MSGraph API

To be able to call for the [MSGraph API](https://learn.microsoft.com/graph/use-the-api), the `Microsoft.Graph` package must be installed. Then the below code inside `CallGraphButton_Click()` method is called:

```csharp
 GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(_winUiSettings.Scopes.Split(' '));
 User graphUser = await graphClient.Me.Request().GetAsync();
```

### Additional code

Take a look into [MainWindow.xaml.cs](https://github.com/Azure-Samples/ms-identity-netcore-winui/blob/main/WinUIMSALApp/MainWindow.xaml.cs) and learn how Sign-in/Sign-out buttons callback functions are being used to call MSGraph API and manage user authentication state.

</details>




## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn more

* [WinUi open source project](https://github.com/microsoft/microsoft-ui-xaml)
* [ILogging interface](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging)
* [MSGraph API](https://learn.microsoft.com/graph/use-the-api)
* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  
* To learn more about the code, visit:
  * [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  * [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  * [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)
  
  * [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  *  [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
  * [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
  * [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals)
  * [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
  * [Building Zero Trust ready apps](https://aka.ms/ztdevsession)
