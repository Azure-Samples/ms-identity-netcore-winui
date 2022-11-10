## Explore the sample

<details>
 <summary>Expand the section</summary>

  Start running the sample by pressing `WinUIMSALApp (Package)` button on Visual Studio menu bar.

  No information is displayed because you're not logged in.

  Two options are provided to you , you can either use the regular sign-in or use the Windows 10 WAM broker to sign-in the user instead.
  
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
