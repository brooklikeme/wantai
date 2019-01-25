using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;

using WanTai.Controller;

namespace WanTai.RESTService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RestServices : IRESTServices
    {
        public string GetExperimentResultByName(string Name)
        {
            return new WanTai.Controller.PCR.PCRTestResultViewListController().ExportJSONResult(Name);
        }
    }
}
