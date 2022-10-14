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

</details>
