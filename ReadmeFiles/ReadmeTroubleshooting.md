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