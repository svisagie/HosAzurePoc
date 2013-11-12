using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using HosCommsApi.Models;
using HosCommsApi;
using HosCommsApi.Controllers;
using NUnit.Framework;

namespace HosCommsApi.Tests.Controllers
{
	[TestFixture]
	public class HosCommsControllerTest
	{
		//[TestMethod]
		//public void Get()
		//{
		//	// Arrange
		//	ValuesController controller = new ValuesController();

		//	// Act
		//	IEnumerable<string> result = controller.Get();

		//	// Assert
		//	Assert.IsNotNull(result);
		//	Assert.AreEqual(2, result.Count());
		//	Assert.AreEqual("value1", result.ElementAt(0));
		//	Assert.AreEqual("value2", result.ElementAt(1));
		//}

		//[TestMethod]
		//public void GetById()
		//{
		//	// Arrange
		//	ValuesController controller = new ValuesController();

		//	// Act
		//	string result = controller.Get(5);

		//	// Assert
		//	Assert.AreEqual("value", result);
		//}

		//[Test]
		//public void WorkstatePostTest()
		//{
		//	// Arrange
		//	var controller = new HosCommsController();

		//	// Act
		//	for (var i = 0; i < 5; i++)
		//	{
		//		Task<string> result = controller.WorkstatePost(1,
		//			new DriverWorkStateChange { DriverId = 1, WorkStateId = WorkStates.Driving, Timestamp = DateTime.UtcNow });

		//		Assert.IsTrue(string.IsNullOrWhiteSpace(result.Result));
		//	}

		//	// Assert
		//}

		//[TestMethod]
		//public void Put()
		//{
		//	// Arrange
		//	ValuesController controller = new ValuesController();

		//	// Act
		//	controller.Put(5, "value");

		//	// Assert
		//}

		//[TestMethod]
		//public void Delete()
		//{
		//	// Arrange
		//	ValuesController controller = new ValuesController();

		//	// Act
		//	controller.Delete(5);

		//	// Assert
		//}
	}
}
