﻿using System.Net.Http;
using System.Web.Http;
using MvcRouteTester.Test.ApiControllers;
using MvcRouteTester.Test.Assertions;

using NUnit.Framework;

namespace MvcRouteTester.Test.ApiRoute
{
	[TestFixture]
	public class FluentExtensionsTests
	{
		private class TestHandlerOne : DelegatingHandler
		{
		}

		private class TestHandlerTwo : DelegatingHandler
		{
		}

		private HttpConfiguration config;

		[SetUp]
		public void MakeRouteTable()
		{
			RouteAssert.UseAssertEngine(new NunitAssertEngine());

			config = new HttpConfiguration();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional });
		}

		[TearDown]
		public void TearDown()
		{
			RouteAssert.UseAssertEngine(new NunitAssertEngine());
		}

		[Test]
		public void SimpleTest()
		{
			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32));
		}

		[Test]
		public void ShouldMapToFailsWithWrongRoute()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/missing/32/foo").To<CustomerController>(HttpMethod.Get, x => x.Get(32));

			Assert.That(assertEngine.FailCount, Is.EqualTo(4));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("No route matched url 'http://site.com/api/missing/32/foo'"));
		}

		[Test]
		public void WithHandlerShouldFailWithWrongHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
			config.Routes.Clear();
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional },
				constraints: null,
				handler: new TestHandlerOne());

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithHandler<TestHandlerTwo>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Did not match handler type 'TestHandlerTwo' for url 'http://site.com/api/customer/32', found a handler of type 'TestHandlerOne'."));
		}

		[Test]
		public void WithHandlerShouldFailWithNoHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithHandler<TestHandlerTwo>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Did not match handler type 'TestHandlerTwo' for url 'http://site.com/api/customer/32', found no handler."));
		}

		[Test]
		public void WithHandlerShouldSucceedWithCorrectHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
			config.Routes.Clear();
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional },
				constraints: null,
				handler: new TestHandlerOne());

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithHandler<TestHandlerOne>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(0));
		}

		[Test]
		public void WithoutHandlerWithtypeShouldSucceedIfNoHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithoutHandler<TestHandlerOne>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(0));
		}

		[Test]
		public void WithoutHandlerShouldSucceedIfNoHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithoutHandler();

			Assert.That(assertEngine.FailCount, Is.EqualTo(0));
		}

		[Test]
		public void WithoutHandlerWithTypeShouldSucceedIfDifferentHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
			config.Routes.Clear();
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional },
				constraints: null,
				handler: new TestHandlerOne());

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithoutHandler<TestHandlerTwo>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(0));
		}

		[Test]
		public void WithoutHandlerShouldFailIfDifferentHandler()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
			config.Routes.Clear();
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional },
				constraints: null,
				handler: new TestHandlerOne());

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithoutHandler();

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Matching handler of type 'TestHandlerOne' found for url 'http://site.com/api/customer/32'."));
		}

		[Test]
		public void WithoutHandlerShouldFailIfMatchingHandlerIsFound()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
			config.Routes.Clear();
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional },
				constraints: null,
				handler: new TestHandlerOne());

			config.ShouldMap("/api/customer/32").To<CustomerController>(HttpMethod.Get, x => x.Get(32)).WithoutHandler<TestHandlerOne>();

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Matching handler of type 'TestHandlerOne' found for url 'http://site.com/api/customer/32'."));

		}
		[Test]
		public void TestNoRouteForMethod()
		{
			config.ShouldMap("/api/customer/32").ToNoMethod<CustomerController>(HttpMethod.Post);
		}

		[Test]
		public void ShouldMapToNoMethodFailsOnValidRoute()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/customer/32").ToNoMethod<CustomerController>(HttpMethod.Get);

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Method GET is allowed on url '/api/customer/32'"));
		}

		[Test]
		public void TestNoRoute()
		{
			config.ShouldMap("/pai/customer/32").ToNoRoute();
		}

		[Test]
		public void ShouldMapToNoRouteFailsOnValidRoute()
		{
			var assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);

			config.ShouldMap("/api/customer/32").ToNoRoute();

			Assert.That(assertEngine.FailCount, Is.EqualTo(1));
			Assert.That(assertEngine.Messages[0], Is.EqualTo("Found a route for url '/api/customer/32'"));
		}
	}
}
