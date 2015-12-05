using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Web.Http.Controllers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Web.Http.Filters;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.Web.Models;

namespace Abp.WebApi.Controllers.Dynamic.Selectors
{
    public class DyanamicHttpActionDescriptor : ReflectedHttpActionDescriptor
    {
        /// <summary>
        /// The Action filters for the Action Descriptor.
        /// </summary>
        private readonly IFilter[] _filters;

        public override Type ReturnType
        {
            get
            {
                return GetReturnType();
            }
        }

        private Type GetReturnType()
        {
            var originType = this.MethodInfo.ReturnType;
            if (originType == typeof(AjaxResponse) ||
                (originType.IsGenericType && originType.GetGenericTypeDefinition() == typeof(AjaxResponse<>)) ||
                originType == typeof(HttpResponseMessage))
            {
                return originType;
            }
            if (originType == typeof(void))
            {
                return typeof(AjaxResponse);
            }
            return typeof(AjaxResponse<>).MakeGenericType(originType);
        }

        public DyanamicHttpActionDescriptor(HttpControllerDescriptor controllerDescriptor, MethodInfo methodInfo, IFilter[] filters = null)
            : base(controllerDescriptor, methodInfo)
        {
            _filters = filters;
        }

        public override System.Threading.Tasks.Task<object> ExecuteAsync(HttpControllerContext controllerContext, System.Collections.Generic.IDictionary<string, object> arguments, System.Threading.CancellationToken cancellationToken)
        {
            return base
                .ExecuteAsync(controllerContext, arguments, cancellationToken)
                .ContinueWith(task =>
                {
                    try
                    {
                        var originType = this.MethodInfo.ReturnType;
                        if (originType == typeof(AjaxResponse) ||
    (originType.IsGenericType && originType.GetGenericTypeDefinition() == typeof(AjaxResponse<>)) ||
    originType == typeof(HttpResponseMessage))
                        {
                            return task.Result;
                        }
                        if (originType == typeof(void))
                        {
                            return new AjaxResponse();
                        }
                        var returnType = typeof(AjaxResponse<>).MakeGenericType(originType);
                        var instance = Activator.CreateInstance(returnType);
                        returnType.GetProperty("Result").SetValue(instance, task.Result);
                        return instance;
                    }
                    catch (AggregateException ex)
                    {
                        ex.InnerException.ReThrow();
                        throw; // The previous line will throw, but we need this to makes compiler happy
                    }
                });
        }

        /// <summary>
        /// The overrides the GetFilters for the action and adds the Dynamic Action filters.
        /// </summary>
        /// <returns> The Collection of filters.</returns>
        public override Collection<IFilter> GetFilters()
        {
            var actionFilters = new Collection<IFilter>();

            if (!_filters.IsNullOrEmpty())
            {
                foreach (var filter in _filters)
                {
                    actionFilters.Add(filter);
                }
            }

            foreach (var baseFilter in base.GetFilters())
            {
                actionFilters.Add(baseFilter);
            }
            return actionFilters;
        }

        #region Overrides of ReflectedHttpActionDescriptor

        /// <summary>
        /// Returns an array of custom attributes defined for this member, identified by type.
        /// </summary>
        /// <returns>
        /// An array of custom attributes or an empty array if no custom attributes exist.
        /// </returns>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param><typeparam name="T">The type of the custom attributes.</typeparam>
        public override Collection<T> GetCustomAttributes<T>(bool inherit)
        {
            var attributes = base.GetCustomAttributes<T>(inherit);
            Debug.WriteLine(attributes.Count, "GetCustomAttributes<" + typeof(T).FullName + ">(" + inherit + ")");
            foreach (var attribute in attributes)
            {
                Debug.WriteLine(attribute.GetType().FullName, "attribute.GetType().FullName");
            }
            return attributes;
        }

        #endregion
    }
}