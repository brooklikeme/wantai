using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WanTai.RESTService
{
    [ServiceContract(Name = "RESTServices")]
    public interface IRESTServices
    {
        [OperationContract]
        [WebGet(UriTemplate = Routing.GetExperimentResultRoute, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        string GetExperimentResultByName(string Name);
    }
}
