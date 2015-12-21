using Microsoft.SharePoint.WebPartPages;
using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace UploadFileToFTP.wpCopyFiletoFTP
{
    [ToolboxItemAttribute(false)]
    public partial class wpCopyFiletoFTP : System.Web.UI.WebControls.WebParts.WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public wpCopyFiletoFTP()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string urlLink = string.Empty;
            string basePath = "/_layouts/UploadFileToFTP";
            string version = "?v01";

            //urlLink += "<link rel='stylesheet' type='text/css' href='/_layouts/15/PPFA.AED.HCA/css/main.css' />";
            urlLink += string.Format("<script src='{0}/js/app/app-setting.js{1}'></script>", basePath, version);
            urlLink += string.Format("<script src='{0}/js/app/config.js{1}'></script>", basePath, version);
            urlLink += string.Format("<script src='{0}/js/app/routes.js{1}'></script>", basePath, version);
            urlLink += string.Format("<script src='{0}/js/app/services/global.js{1}'></script>", basePath, version);
            urlLink += string.Format("<script src='{0}/js/app/services/sharepoint.jsom.js{1}'></script>", basePath, version);
            //urlLink += string.Format("<script src='{0}/js/services/start.js'></script>", basePath);

            urlLink += string.Format("<script src='{0}/js/app/controllers/start.js{1}'></script>", basePath, version);
            urlLink += string.Format("<script src='{0}/js/app/app.js{1}'></script>", basePath, version);
            ltrApp.Text = urlLink;
            hdSecureStoreID.Value = SecureStoreID;
        }

        [WebBrowsable(true),
         WebDisplayName("Target Application ID"),
         WebDescription("Please Input Secure Store ID"),
         Personalizable(PersonalizationScope.Shared),
         Category("FTP Info")]
        public string SecureStoreID { get; set; }
      
    }
    //https://rmanimaran.wordpress.com/2011/02/27/sharepoint-webpart-custom-properties-password-field-as-property/
    class PasswordToolPart : ToolPart
    {

        public TextBox txtPassword = null;

        private void PasswordToolPart_Init(object sender, EventArgs e)
        {

            txtPassword = new TextBox();

            txtPassword.TextMode = TextBoxMode.Password;

            txtPassword.ID = "txtPassword";

            SyncChanges();

        }

        public PasswordToolPart(string strTitle)
        {

            this.Title = strTitle;

            this.Init += new EventHandler(PasswordToolPart_Init);

        }

        public override void ApplyChanges()
        {

            base.ApplyChanges();

        }

        public override void SyncChanges()
        {

            EnsureChildControls();

        }

        protected override void CreateChildControls()
        {

            base.CreateChildControls();

            Controls.Add(txtPassword);

        }

    }
}
