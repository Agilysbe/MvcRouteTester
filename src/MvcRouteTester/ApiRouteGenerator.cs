﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

using NUnit.Framework;

namespace MvcRouteTester
{
	/// <summary>
	/// code from http://www.strathweb.com/2012/08/testing-routes-in-asp-net-web-api/
	/// </summary>
	internal class ApiRouteGenerator
	{
		readonly HttpConfiguration config;
		readonly HttpRequestMessage request;

		IHttpRouteData routeData;
		IHttpControllerSelector controllerSelector;
		HttpControllerContext controllerContext;

		public ApiRouteGenerator(HttpConfiguration conf, HttpRequestMessage req)
		{
			config = conf;
			request = req;
			
			GenerateRouteData();
		}

		public IHttpRouteData RouteData
		{
			get { return routeData; }
		}

		public Dictionary<string, string> ReadRouteProperties(string url, HttpMethod httpMethod)
		{
			if (RouteData == null)
			{
				var noRouteDataMessage = string.Format("No route to url '{0}'", url);
				Assert.Fail(noRouteDataMessage);
			}

			if (!IsRouteFound())
			{
				var routeNotFoundMessage = string.Format("Route not found to url '{0}'", url);
				Assert.Fail(routeNotFoundMessage);
			}

			if (!IsMethodAllowed())
			{
				var methodNotAllowedMessage = string.Format("Method {0} is not allowed on url '{1}'", httpMethod, url);
				Assert.Fail(methodNotAllowedMessage);
			}

			var actualProps = new Dictionary<string, string>
				{
					{ "controller", ControllerName() },
					{ "action", ActionName() }
				};

			var routeParams = GetRouteParams();

			foreach (var paramKey in routeParams.Keys)
			{
				actualProps.Add(paramKey, routeParams[paramKey]);
			}

			return actualProps;
		}

		public IDictionary<string, string> GetRouteParams()
		{
			var actionDescriptor = MakeActionDescriptor();
			var actionParams = actionDescriptor.GetParameters();

			var result = new Dictionary<string, string>();
			var routeDataValues = GetRouteData();
			if (routeDataValues != null)
			{
				foreach (var param in actionParams)
				{
					var paramName = param.ParameterName;
					if (routeDataValues.Values.ContainsKey(paramName))
					{
						var paramValue = routeDataValues.Values[paramName];
						if (paramValue != null)
						{
							result.Add(paramName, paramValue.ToString());
						}
					}
				}
			}

			return result;
		}

		private HttpRouteData GetRouteData()
		{
			if (! request.Properties.Any(prop => prop.Value is HttpRouteData))
			{
				return null;
			}

			var routeDataProp = request.Properties.First(prop => prop.Value is HttpRouteData);
			return routeDataProp.Value as HttpRouteData;
		}

		public string ActionName()
		{
			if (controllerContext.ControllerDescriptor == null)
			{
				ControllerType();
			}

			var descriptor = MakeActionDescriptor();

			return descriptor.ActionName;
		}

		public bool IsRouteFound()
		{
			try
			{
				return !string.IsNullOrEmpty(ActionName());
			}
			catch (HttpResponseException hrex)
			{
				var status = hrex.Response.StatusCode;
				return status != HttpStatusCode.NotFound;
			}
		}

		public bool IsMethodAllowed()
		{
			try
			{
				return ! string.IsNullOrEmpty(ActionName());
			}
			catch (HttpResponseException hrex)
			{
				var status = hrex.Response.StatusCode;
				return status != HttpStatusCode.MethodNotAllowed;
			}
		}

		public Type ControllerType()
		{
			var descriptor = controllerSelector.SelectController(request);
			controllerContext.ControllerDescriptor = descriptor;
			return descriptor.ControllerType;
		}

		public string ControllerName()
		{
			var controllerType = ControllerType();
			var name = controllerType.Name;
			if (name.EndsWith("Controller"))
			{
				name = name.Substring(0, name.Length - 10);
			}

			return name;
		}

		private void GenerateRouteData()
		{
			routeData = config.Routes.GetRouteData(request);

			if (routeData != null)
			{
				request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
				controllerSelector = new DefaultHttpControllerSelector(config);
				controllerContext = new HttpControllerContext(config, routeData, request);
			}
		}

		private HttpActionDescriptor MakeActionDescriptor()
		{
			var actionSelector = new ApiControllerActionSelector();
			return actionSelector.SelectAction(controllerContext);
		}
	}
}
