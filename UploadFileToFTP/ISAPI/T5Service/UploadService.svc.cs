using Microsoft.BusinessData.Infrastructure.SecureStore;
using Microsoft.Office.SecureStoreService.Server;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Client.Services;
using Microsoft.SharePoint.Utilities;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
       

        public ResultRespone Upload(string secureID, string serverRelativeUrl)
        {
            /*get info FTP*/
            ResultRespone result = new ResultRespone { isSuccess = true, Message = string.Format("Upload File: {0} is successful!", serverRelativeUrl) };

            try
            {
                //string[] str = GetCredentialsFromSecureStoreService(secureID);//"FTP1"
                Dictionary<string, string> sssCredentials = GetCredentials(secureID);
                string Path = FindValue(sssCredentials, "PATH");// str[0];
                string UserName = FindValue(sssCredentials, "USER"); //str[1];
                string Password = FindValue(sssCredentials, "PASS"); //str[2];
                SPWeb web = SPContext.Current.Web;
                SPFile spfile = web.GetFile(serverRelativeUrl);
                if (spfile.Exists)
                {
                    //string uri = "ftp://112.78.2.146/Test/" + spfile.Name;
                    string uri = Path + "/" + spfile.Name;
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
                            result.Message = string.Format("Can not Upload file {0} to FTP: {1}", serverRelativeUrl, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.Message = string.Format("Can not Upload file {0} to FTP: {1}", serverRelativeUrl, ex.Message);
            }

            return result;


        }
        private Dictionary<string, string> GetCredentials(string applicationID)
        {
            var credentialMap = new Dictionary<string, string>();
            SPSite site = SPContext.Current.Site;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite oSite = new SPSite(site.Url))
                {
                    SPServiceContext serviceContext = SPServiceContext.GetContext(oSite);
                    var secureStoreProvider = new SecureStoreProvider { Context = serviceContext };
                    using (var credentials = secureStoreProvider.GetCredentials(applicationID))
                    {
                        var fields = secureStoreProvider.GetTargetApplicationFields(applicationID);
                        for (var i = 0; i < fields.Count; i++)
                        {
                            var field = fields[i];
                            var credential = credentials[i];
                            var decryptedCredential = ToClrString(credential.Credential);

                            credentialMap.Add(field.Name, decryptedCredential);
                        }
                    }
                }
            });
            return credentialMap;
        }
        private string FindValue(Dictionary<string, string> credentials, string key)
        {
            string value = string.Empty;
            int n = credentials.Count;
            if (credentials.ContainsKey(key))
            {
                value = credentials[key];
            }
            return value;
        }
        private string ToClrString(SecureString secureString)
        {
            var ptr = Marshal.SecureStringToBSTR(secureString);

            try
            {
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.FreeBSTR(ptr);
            }
        }
        
        //private SPSite GetCentralAdminSite()
        //{
        //    SPSite adminSite = null;
        //    SPSecurity.RunWithElevatedPrivileges(delegate
        //    {
        //        SPAdministrationWebApplication adminWebApp = SPAdministrationWebApplication.Local;
        //        if (adminWebApp == null)
        //        {
        //            throw new InvalidProgramException("Unable to get the admin web app");
        //        }
        //        Uri adminSiteUri = adminWebApp.GetResponseUri(SPUrlZone.Default);
        //        if (adminSiteUri != null)
        //        {
        //            adminSite = adminWebApp.Sites[adminSiteUri.AbsoluteUri];
        //        }
        //        else
        //        {
        //            throw new InvalidProgramException("Unable to get Central Admin Site.");
        //        }

        //    });
        //    return adminSite;
        //}



        //private string[] GetCredentialsFromSecureStoreService(string AppId)//SPSite Site, 
        //{
        //    string[] Credentials = new String[3];
        //    SPSecurity.RunWithElevatedPrivileges(delegate
        //    {
        //        using (SPServiceContextScope scope = new Microsoft.SharePoint.SPServiceContextScope(SPServiceContext.GetContext(GetCentralAdminSite())))//Site
        //        {
        //            string Provider = "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        //            Type ProviderType = Type.GetType(Provider);
        //            ISecureStoreProvider provider = (ISecureStoreProvider)Activator.CreateInstance(ProviderType);
        //            string appTargetName = AppId;
        //            SecureStoreCredentialCollection credentials = provider.GetCredentials(appTargetName);

        //            foreach (ISecureStoreCredential cred in credentials)
        //            {
        //                if (cred.CredentialType == SecureStoreCredentialType.UserName)
        //                {
        //                    Credentials[0] = ParseString(cred.Credential);
        //                }
        //                else if (cred.CredentialType == SecureStoreCredentialType.Password)
        //                {
        //                    Credentials[2] = ParseString(cred.Credential);
        //                }
        //                else if (cred.CredentialType == SecureStoreCredentialType.Generic)
        //                {
        //                    if (Credentials[0] == null)
        //                        Credentials[0] = ParseString(cred.Credential);//Path
        //                    else
        //                        Credentials[1] = ParseString(cred.Credential);//Path
        //                }
        //            }
        //        }
        //    });
        //    return Credentials;
        //}

        //private static string ParseString(System.Security.SecureString secureString)
        //{
        //    string outStr = null;
        //    IntPtr intPtr = IntPtr.Zero;

        //    try
        //    {
        //        intPtr = Marshal.SecureStringToBSTR(secureString);
        //        outStr = Marshal.PtrToStringBSTR(intPtr);
        //    }
        //    finally
        //    {
        //        Marshal.FreeBSTR(intPtr);
        //    }

        //    return outStr;
        //}
        

        public ResultRespone UpdateStatusFile(List<MyFile> myFiles)
        {
            int n = myFiles.Count;
            ExportExcel(myFiles);
            ResultRespone result = new ResultRespone { isSuccess = true, Message = string.Format("Upload File: {0} is successful!", n) };
            return result;
        }
        protected void ExportExcel(List<MyFile> myFiles)
        {
            SPUser u = SPContext.Current.Web.CurrentUser;
            DateTime now = DateTime.Now;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string fileName = string.Format("FTP_{0}.xlsx", DateTime.Now.ToString("yyyyMMdd_HHmmss"));/*FTP_YYYYMMDD_HHMMSS.xls*/
                string hcaTemplate = SPUtility.GetGenericSetupPath(@"TEMPLATE\LAYOUTS\UploadFileToFTP\ftpstatus.xlsx");
                var path = SPUtility.GetGenericSetupPath(@"TEMPLATE\LAYOUTS\UploadFileToFTP\export\") + @"\" + fileName;
                #region write file to physical
                using (FileStream fs = new FileStream(hcaTemplate, FileMode.Open, FileAccess.Read))
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(fs);
                    workbook.SetSheetName(0, fileName);
                    ISheet sheet = workbook.GetSheetAt(0);
                    for (int i = 0; i < myFiles.Count; i++)
                    {
                        int row = i + 2;
                        IRow r = sheet.CreateRow(row);
                        ICell cNo = r.CreateCell(0);
                        ICell cFileName = r.CreateCell(1);
                        ICell cUploadDate = r.CreateCell(2);
                        ICell cUploadBy = r.CreateCell(3);
                        ICell cStatus = r.CreateCell(4);
                        ICell cUrl = r.CreateCell(5);
                        cNo.SetCellValue(i + 1);
                        cFileName.SetCellValue(myFiles[i].fileName);
                        cUrl.SetCellValue(myFiles[i].url);
                        cUploadDate.SetCellValue(now.ToString("dd/MM/yyyyy"));
                        cUploadBy.SetCellValue(u.LoginName);
                        cStatus.SetCellValue(myFiles[i].status);
                    }

                    using (var f = File.Create(string.Format(@"{0}", path)))
                    {
                        workbook.Write(f);
                        f.Close();
                        f.Dispose();

                    }
                }
                #endregion
                SaveFileToDoc(path);
            });
        }
        protected void SaveFileToDoc(string fileToUpload)
        {
            using (SPSite oSite = new SPSite(SPContext.Current.Web.Url))
            {
                using (SPWeb oWeb = oSite.OpenWeb())
                {
                    oWeb.AllowUnsafeUpdates = true;
                    if (!System.IO.File.Exists(fileToUpload))
                        throw new FileNotFoundException("File not found.", fileToUpload);
                    string url = oWeb.ServerRelativeUrl == "/" ? string.Empty : oWeb.ServerRelativeUrl;
                    SPList myList = oWeb.GetList(url + "/Lists/UploadFTPReport");
                    //SPList myList = oWeb.Lists["Upload FTP Report"];
                    SPFolder myLibrary = myList.RootFolder;
                    // Prepare to upload
                    Boolean replaceExistingFiles = true;
                    String fileName = System.IO.Path.GetFileName(fileToUpload);
                    FileStream fileStream = File.OpenRead(fileToUpload);
                    // Upload document
                    SPFile spfile = myLibrary.Files.Add(fileName, fileStream, replaceExistingFiles);
                    // Commit 
                    myLibrary.Update();
                    oWeb.AllowUnsafeUpdates = false;
                }
            }
        }
        




    }
    public class ResultRespone
    {
        public string Message { get; set; }
        public bool isSuccess { get; set; }
    }
    public class MyFile
    {
        public string url;
        public string fileName;
        public string status;
    }
}
