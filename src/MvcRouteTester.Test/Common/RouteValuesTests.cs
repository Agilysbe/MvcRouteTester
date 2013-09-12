﻿using System.Collections.Generic;

using MvcRouteTester.Common;

using NUnit.Framework;

namespace MvcRouteTester.Test.Common
{
	[TestFixture]
	public class RouteValuesTests
	{
		private FakeAssertEngine assertEngine;

		[SetUp]
		public void MakeRouteTable()
		{
			assertEngine = new FakeAssertEngine();
			RouteAssert.UseAssertEngine(assertEngine);
		}

		[Test]
		public void TestCreate()
		{
			var values = new RouteValues();
			Assert.That(values, Is.Not.Null);
		}

		[Test]
		public void TestRetrieveWithFromBodyFlagMatching()
		{
			var values = RouteValuesContainingId();

			var valueOut = values.GetRouteValue("Id", RouteValueOrigin.Unknown);

			Assert.That(valueOut, Is.Not.Null);
			Assert.That(valueOut.Name, Is.EqualTo("Id"));
			Assert.That(valueOut.Value, Is.EqualTo(42));
			Assert.That(valueOut.Origin, Is.EqualTo(RouteValueOrigin.Unknown));
		}

		[Test, Ignore("Not working yet")]
		public void TestRetrieveFailsWithFromBodyFlagNotMatching()
		{
			var values = RouteValuesContainingId();

			var valueOut = values.GetRouteValue("Id", RouteValueOrigin.Body);

			Assert.That(valueOut, Is.Null);
		}

		[Test]
		public void ReadsEmptyDataAsFail()
		{
			var data = new Dictionary<string, string>();
			var props = new RouteValues(data);
			props.CheckDataOk();

			Assert.That(props.DataOk, Is.False, "data ok");
			Assert.That(props.Controller, Is.Null, "controller");
			Assert.That(props.Action, Is.Null, "action");
		}

		[Test]
		public void ReadsNoActionDataAsFail()
		{
			var data = new Dictionary<string, string>
				{
					{ "controller", "foo" }
				};

			var props = new RouteValues(data);
			props.CheckDataOk();

			Assert.That(props.DataOk, Is.False, "data ok");
		}

		[Test]
		public void ReadsNoControllerDataAsFail()
		{
			var data = new Dictionary<string, string>
				{
					{ "action", "foo" }
				};

			var props = new RouteValues(data);
			props.CheckDataOk();

			Assert.That(props.DataOk, Is.False, "data ok");
		}

		[Test]
		public void FailsAreReported()
		{
			var data = new Dictionary<string, string>();
			var routeValues = new RouteValues(data);
			routeValues.CheckDataOk();

			Assert.That(assertEngine.FailCount, Is.EqualTo(2));
			Assert.That(routeValues.DataOk, Is.False);
		}

		[Test]
		public void ReadsEmptyDataWithoutRouteValues()
		{
			var data = new Dictionary<string, string>();
			var props = new RouteValues(data);

			Assert.That(props.Values, Is.Not.Null, "route values null");
			Assert.That(props.Values.Count, Is.EqualTo(0), "route values empty");
		}

		[Test]
		public void ReadsControllerAndAction()
		{
			var data = new Dictionary<string, string>
				{
					{ "controller", "foo" },
					{ "action", "bar" },
				};

			var props = new RouteValues(data);

			Assert.That(props.Controller, Is.EqualTo("foo"), "controller");
			Assert.That(props.Action, Is.EqualTo("bar"), "action");
			Assert.That(props.Values.Count, Is.EqualTo(0), "route values empty");
		}

		public void ReadsControllerAndActionToDataOk()
		{
			var data = new Dictionary<string, string>
				{
					{ "controller", "foo" },
					{ "action", "bar" },
				};

			var props = new RouteValues(data);
			props.CheckDataOk();

			Assert.That(props.DataOk, Is.True, "data ok");
		}


		[Test]
		public void ReadsOtherValuesAsRouteValues()
		{
			var data = new Dictionary<string, string>
				{
					{ "controller", "foo" },
					{ "action", "bar" },
					{ "fish", "hallibut" },
				};

			var props = new RouteValues(data);
			props.CheckDataOk();

			Assert.That(props.DataOk, Is.True, "data ok");
			Assert.That(props.Values.Count, Is.EqualTo(1), "route values not empty");

			var output = props.GetRouteValue("fish", RouteValueOrigin.Unknown);

			Assert.That(output, Is.Not.Null, "route value missing");
			Assert.That(output.ValueAsString, Is.EqualTo("hallibut"), "route value wrong");
		}



		private static RouteValues RouteValuesContainingId()
		{
			var values = new RouteValues();
			var idValue = new RouteValue("Id", 42, RouteValueOrigin.Unknown);
			values.Add(idValue);

			return values;
		}
	}
}
