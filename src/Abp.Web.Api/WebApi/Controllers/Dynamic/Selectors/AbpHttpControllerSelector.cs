using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Abp.Collections.Extensions;
using Abp.WebApi.Controllers.Dynamic.Builders;

namespace Abp.WebApi.Controllers.Dynamic.Selectors
{
    /// <summary>
    /// This class is used to extend default controller selector to add dynamic api controller creation feature of Abp.
    /// It checks if requested controller is a dynamic api controller, if it is,
    /// returns <see cref="HttpControllerDescriptor"/> to ASP.NET system.
    /// </summary>
    public class AbpHttpControllerSelector : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;

        /// <summary>
        /// Creates a new <see cref="AbpHttpControllerSelector"/> object.
        /// </summary>
        /// <param name="configuration">Http configuration</param>
        public AbpHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method is called by Web API system to select the controller for this request.
        /// </summary>
        /// <param name="request">Request object</param>
        /// <returns>The controller to be used</returns>
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request != null)
            {
                var routeData = request.GetRouteData();
                if (routeData != null)
                {
                    string serviceNameWithAction;
                    if (routeData.Values.TryGetValue("serviceNameWithAction", out serviceNameWithAction) && DynamicApiServiceNameHelper.IsValidServiceNameWithAction(serviceNameWithAction))
                    {
                        var serviceName = DynamicApiServiceNameHelper.GetServiceNameInServiceNameWithAction(serviceNameWithAction);
                        var controllerInfo = DynamicApiControllerManager.FindOrNull(serviceName);
                        if (controllerInfo != null)
                        {
                            var controllerDescriptor = new DynamicHttpControllerDescriptor(_configuration, controllerInfo.ServiceName, controllerInfo.Type, controllerInfo.Filters);
                            controllerDescriptor.Properties["__AbpDynamicApiControllerInfo"] = controllerInfo;
                            return controllerDescriptor;
                        }
                    }
                }
            }

            return base.SelectController(request);
        }

        #region Overrides of DefaultHttpControllerSelector

        /// <summary>
        /// Returns a map, keyed by controller string, of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> that the selector can select. 
        /// </summary>
        /// <returns>
        /// A map of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/>.
        /// </returns>
        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var dic = base.GetControllerMapping();
            var dynamicControllers = DynamicApiControllerManager.GetAll();
            Debug.Assert(dynamicControllers!=null);
            Debug.WriteLine(dynamicControllers.Count, "DynamicApiControllerManager.GetAll().Count");
            foreach (var controllerInfo in dynamicControllers)
            {
                dic.Add(controllerInfo.ServiceName, new DynamicHttpControllerDescriptor(_configuration, controllerInfo.ServiceName,
                            controllerInfo.Type,
                            controllerInfo.Filters));
            }
            return dic;
        }

        #endregion
    }
}