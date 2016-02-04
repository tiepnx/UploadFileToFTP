<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="wpCopyFiletoFTP.ascx.cs" Inherits="UploadFileToFTP.wpCopyFiletoFTP.wpCopyFiletoFTP" %>
<link rel="stylesheet" type="text/css" href="/_layouts/UploadFileToFTP/css/toastr.css" />
<%--<link rel="stylesheet" type="text/css" href="/_layouts/UploadFileToFTP/css/bootstrap.min.css" />--%>
<link rel="stylesheet" type="text/css" href="/_layouts/UploadFileToFTP/css/style.css" />

<script src='/_layouts/UploadFileToFTP/js/lib/jquery-1.11.0.min.js' type="text/javascript"></script>


<script type=”text/ecmascript” src="/_layouts/SP.Core.js" />
<script type=”text/ecmascript” src="/_layouts/SP.Debug.js" />
<script type=”text/ecmascript” src="/_layouts/SP.Runtime.Debug.js" />

<script src='/_layouts/UploadFileToFTP/js/lib/bootstrap.min.js' type="text/javascript"></script>
<script src='/_layouts/UploadFileToFTP/js/lib/angular.js' type="text/javascript"></script>
<script src='/_layouts/UploadFileToFTP/js/lib/angular-ui-router.min.js' type="text/javascript"></script>
<script src='/_layouts/UploadFileToFTP/js/lib/toastr.min.js' type="text/javascript"></script>

<div id="niftApp" style="color">
    <div data-ui-view="header"></div>
    <div data-ui-view="content"></div>
</div>
<asp:Literal ID="ltrApp" runat="server"></asp:Literal>
<asp:HiddenField ID="hdSecureStoreID" runat="server" />
<%--<button type="button" onclick="test();">Test</button>
<script type="text/javascript">
    function test() {
        try {
            $.ajax({
                type: "GET",
                url: '/_vti_bin/T5Service/UploadService.svc/Upload',
                contentType: "application/json; charset=utf-8",
                data: { "url": '/Shared Documents/1-Hindi.png' },
                dataType: 'json',
                success: function (msg) {
                    console.log(msg);
                },
                error: function (err) {
                    console.log(err);
                }
            });
        }
        catch (e) {

            alert('error invoking service.get()' + e);
        }
    }
</script>--%>