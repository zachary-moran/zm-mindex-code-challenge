using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class ReportingStructureControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void GetReportingStructure_TopManager_ReturnsFourReports()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // Execute
            var response = _httpClient.GetAsync($"api/reportingStructure/{employeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(employeeId, reportingStructure.Employee.EmployeeId);
            Assert.AreEqual(4, reportingStructure.NumberOfReports);
            Assert.AreEqual(2, reportingStructure.Employee.DirectReports.Count);
        }

        [TestMethod]
        public void GetReportingStructure_MidManager_ReturnsTwoReports()
        {
            // Arrange
            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            // Execute
            var response = _httpClient.GetAsync($"api/reportingStructure/{employeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(employeeId, reportingStructure.Employee.EmployeeId);
            Assert.AreEqual(2, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_LeafEmployee_ReturnsZero()
        {
            // Arrange
            var employeeId = "b7839309-3348-463b-a7e3-5de1c168beb3";

            // Execute
            var response = _httpClient.GetAsync($"api/reportingStructure/{employeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(0, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_NewEmployee_ReturnsZero()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };
            var requestContent = new JsonSerialization().ToJson(employee);
            var postResponse = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json")).Result;
            var newEmployee = postResponse.DeserializeContent<Employee>();

            // Execute
            var response = _httpClient.GetAsync($"api/reportingStructure/{newEmployee.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(0, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_UnknownId_Returns_NotFound()
        {
            // Execute
            var response = _httpClient.GetAsync("api/reportingStructure/Invalid_Id").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructure_AfterMidManagerUpdated_KeepsReportingLinks()
        {
            // Arrange
            var ringo = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Ringo",
                LastName = "Starr",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(ringo);
            var putResponse = _httpClient.PutAsync($"api/employee/{ringo.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json")).Result;
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            // Execute
            var managerResponse = _httpClient.GetAsync("api/reportingStructure/16a596ae-edd3-4847-99fe-c4518e82c86f").Result;
            var updatedResponse = _httpClient.GetAsync($"api/reportingStructure/{ringo.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(4, managerResponse.DeserializeContent<ReportingStructure>().NumberOfReports);
            var updated = updatedResponse.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(ringo.Position, updated.Employee.Position);
            Assert.AreEqual(2, updated.NumberOfReports);
        }
    }
}
