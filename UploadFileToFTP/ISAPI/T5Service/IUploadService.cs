using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Runtime.Serialization;
namespace UploadFileToFTP
{
    [ServiceContract]
    //[ServiceContract(Namespace = "http:www.jobrocol.com/SPWcfExample/OrderService/2012/06", Name = "UploadService")]
    public interface IUploadService
    {
        //[OperationContract]
        //[WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped,
        //    ResponseFormat = WebMessageFormat.Json, UriTemplate = "/HelloWorld")]
        //string HelloWorld();
        //[OperationContract]
        //[WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare,
        //    RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //void ReadFileOnDocument(string url);
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ResultRespone Upload(string secureID, string url);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ResultRespone UpdateStatusFile(List<MyFile> myFiles);
    }
    
}
