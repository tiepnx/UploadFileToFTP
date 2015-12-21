using Microsoft.BusinessData.Infrastructure.SecureStore;
using Microsoft.Office.SecureStoreService.Server;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Client.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel.Activation;


namespace UploadFileToFTP
{
    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class UploadService : IUploadService
    {
        //public string HelloWorld()
        //{
        //    //ReadInfoFtp("test");
            
        //    return string.Format("Path: {0}, UserName: {1}, Pass: {2}", Path, UserName, Password);
        //}
        public void ReadFileOnDocument(string serverRelativeUrl)
        {
            SPWeb web = SPContext.Current.Web;
            SPFile spfile = web.GetFile(serverRelativeUrl);
            if (spfile.Exists)
            {
                using (FileStream fs = File.OpenWrite(@"E:\files" + "\\" + spfile.Name))
                {
                    byte[] b = spfile.OpenBinary();
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(b);
                    bw.Close();
                }
            }
        }

        public ResultRespone Upload(string secureID, string serverRelativeUrl)
        {
            /*get info FTP*/
            ResultRespone result = new ResultRespone { isSuccess = true, Message = string.Format("Upload File: {0} is successful!", serverRelativeUrl) };
            try {
                string[] str = GetCredentialsFromSecureStoreService(secureID);//"FTP1"
                string Path = str[0];
                string UserName = str[1];
                string Password = str[2];
                SPWeb web = SPContext.Current.Web;
                SPFile spfile = web.GetFile(serverRelativeUrl);
                if (spfile.Exists)
                {
                    //string uri = "ftp://112.78.2.146/Test/" + spfile.Name;
                    string uri = Path +"/" + spfile.Name;
                    // Create FtpWebRequest object from the Uri provided
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                    // Provide the WebPermission Credintials
                    
                    //reqFTP.Credentials = new NetworkCredential("sha45749", "33e4e899A@!");
                    reqFTP.Credentials = new NetworkCredential(UserName, Password);
                    // By default KeepAlive is true, where the control connection is not closed
                    // after a command is executed.
                    reqFTP.KeepAlive = false;
                    // Specify the command to be executed.
                    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                    // Specify the data transfer type.
                    reqFTP.UseBinary = true;
                    using (Stream fs = spfile.OpenBinaryStream())
                    {
                        byte[] b = spfile.OpenBinary();
                        // The buffer size is set to 2kb
                        int buffLength = b.Length;
                        byte[] buff = new byte[buffLength];
                        try
                        {
                            // Stream to which the file to be upload is written
                            Stream strm = reqFTP.GetRequestStream();
                            strm.Write(b, 0, buffLength);
                            // Close the file stream and the Request Stream
                            strm.Close();
                            fs.Close();
                        }
                        catch (Exception ex)
                        {
                            result.isSuccess = false;
                            result.Message = string.Format("Can not Upload file {0} to FTP: {1}", serverRelativeUrl,  ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex) {
                result.isSuccess = false;
                result.Message = string.Format("Can not Upload file {0} to FTP: {1}", serverRelativeUrl, ex.Message);
            }
            
            return result;
            
            
        }

        private string GetStringFromSecureString(SecureString secStr)
        {
            if (secStr == null)
            {
                return null;
            }

            IntPtr pPlainText = IntPtr.Zero;
            try
            {
                pPlainText = Marshal.SecureStringToBSTR(secStr);
                return Marshal.PtrToStringBSTR(pPlainText);
            }
            finally
            {
                if (pPlainText != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(pPlainText);
                }
            }
        }
        public SPSite GetCentralAdminSite()
        {
            SPSite adminSite = null;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                SPAdministrationWebApplication adminWebApp = SPAdministrationWebApplication.Local;
                if (adminWebApp == null)
                {
                    throw new InvalidProgramException("Unable to get the admin web app");
                }
                Uri adminSiteUri = adminWebApp.GetResponseUri(SPUrlZone.Default);
                if (adminSiteUri != null)
                {
                    adminSite = adminWebApp.Sites[adminSiteUri.AbsoluteUri];
                }
                else
                {
                    throw new InvalidProgramException("Unable to get Central Admin Site.");
                }

            });
            return adminSite;
        }

        private void ReadInfoFtp(string appId)
        {
            // Get the default Secure Store Service provider.
            ISecureStoreProvider provider = SecureStoreProviderFactory.Create();
            if (provider == null)
            {
                throw new InvalidOperationException("Unable to get an ISecureStoreProvider");
            }

            ISecureStoreServiceContext providerContext = provider as ISecureStoreServiceContext;
            providerContext.Context = SPServiceContext.GetContext(GetCentralAdminSite());

            // Create the variables to hold the credentials.
            string userName = null;
            string password = null;
            string pin = null;
            // Specify a valid target application ID for the Secure Store.
            appId = "FTP";

            try
            {
                // Because we are getting the credentials in the using block, all the credentials that we get 
                // will be disposed after the using block. If you need to cache the credentials, do not 
                // use the using block, and dispose the credentials when you are finished.
                //
                // In the following block, we are looking for the first user name, password, and pin
                // credentials in the collection.
                using (SecureStoreCredentialCollection creds = provider.GetCredentials(appId))
                {
                    // Secure Store Service will not return null. It may throw a SecureStoreServiceException,
                    // but this may not be true for other providers.
                    Debug.Assert(creds != null);

                    if (creds != null)
                    {
                        foreach (SecureStoreCredential cred in creds)
                        {
                            if (cred == null)
                            {
                                // Secure Store Service will not return null credentials, but this may not be true for other providers.
                                continue;
                            }

                            switch (cred.CredentialType)
                            {
                                case SecureStoreCredentialType.UserName:
                                    if (userName == null)
                                    {
                                        userName = GetStringFromSecureString(cred.Credential);
                                    }
                                    break;

                                case SecureStoreCredentialType.Password:
                                    if (password == null)
                                    {
                                        password = GetStringFromSecureString(cred.Credential);
                                    }
                                    break;

                                case SecureStoreCredentialType.Pin:
                                    if (pin == null)
                                    {
                                        pin = GetStringFromSecureString(cred.Credential);
                                    }
                                    break;
                                case SecureStoreCredentialType.Generic:
                                    break;
                            }
                        }
                    }
                }

                if (userName == null || password == null || pin == null)
                {
                    throw new InvalidOperationException("Unable to get the credentials");
                }

                // Use the credentials.
                //
                // Note that it is not a secure programming practice to print credential information, but this code example 
                // prints the credentials to the console for testing purposes.
                Console.WriteLine("User Name: " + userName);
                Console.WriteLine("Password : " + password);
                Console.WriteLine("Pin      : " + pin);
            }
            catch (SecureStoreException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public string[] GetCredentialsFromSecureStoreService(string AppId)//SPSite Site, 
        {
            string[] Credentials = new String[3];
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPServiceContextScope scope = new Microsoft.SharePoint.SPServiceContextScope(SPServiceContext.GetContext(GetCentralAdminSite())))//Site
                {
                    string Provider = "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
                    Type ProviderType = Type.GetType(Provider);
                    ISecureStoreProvider provider = (ISecureStoreProvider)Activator.CreateInstance(ProviderType);
                    string appTargetName = AppId;
                    SecureStoreCredentialCollection credentials = provider.GetCredentials(appTargetName);
                    foreach (ISecureStoreCredential cred in credentials)
                    {
                        if (cred.CredentialType == SecureStoreCredentialType.UserName)
                        {
                            Credentials[0] = ParseString(cred.Credential);
                        }
                        else if (cred.CredentialType == SecureStoreCredentialType.Password)
                        {
                            Credentials[2] = ParseString(cred.Credential);
                        }
                        else if (cred.CredentialType == SecureStoreCredentialType.Generic)
                        {
                            if (Credentials[0] == null)
                                Credentials[0] = ParseString(cred.Credential);//Path
                            else
                                Credentials[1] = ParseString(cred.Credential);//Path
                        }
                    }
                }
            });
            return Credentials;
        }

        private static string ParseString(System.Security.SecureString secureString)
        {
            string outStr = null;
            IntPtr intPtr = IntPtr.Zero;

            try
            {
                intPtr = Marshal.SecureStringToBSTR(secureString);
                outStr = Marshal.PtrToStringBSTR(intPtr);
            }
            finally
            {
                Marshal.FreeBSTR(intPtr);
            }

            return outStr;
        }
    }
    public class ResultRespone
    {
        public string Message { get; set; }
        public bool isSuccess { get; set; }
    }
}
