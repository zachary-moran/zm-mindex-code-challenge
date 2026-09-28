using System;
using System.Collections.Generic;
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
    public class CompensationControllerTests
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

        // Each test gets its own employee so compensation records from other tests can't affect latest lookups.
        private static Employee CreateEmployee()
        {
            var employee = new Employee()
            {
                Department = "Finance",
                FirstName = "Penny",
                LastName = "Lane",
                Position = "Accountant",
            };
            var requestContent = new JsonSerialization().ToJson(employee);
            var response = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json")).Result;
            return response.DeserializeContent<Employee>();
        }

        private static HttpResponseMessage PostCompensation(Compensation compensation)
        {
            var requestContent = new JsonSerialization().ToJson(compensation);
            return _httpClient.PostAsync("api/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json")).Result;
        }

        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            var employee = CreateEmployee();
            var compensation = new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 95000.00m,
                EffectiveDate = new DateTime(2026, 1, 1),
            };

            // Execute
            var response = PostCompensation(compensation);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var created = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(created.CompensationId);
            Assert.AreEqual(employee.EmployeeId, created.Employee.EmployeeId);
            Assert.AreEqual(compensation.Salary, created.Salary);
            Assert.AreEqual(compensation.EffectiveDate, created.EffectiveDate);
        }

        [TestMethod]
        public void CreateCompensation_UnknownEmployee_Returns_BadRequest()
        {
            // Arrange
            var compensation = new Compensation()
            {
                Employee = new Employee() { EmployeeId = "Invalid_Id" },
                Salary = 1.00m,
                EffectiveDate = new DateTime(2026, 1, 1),
            };

            // Execute
            var response = PostCompensation(compensation);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void CreateCompensation_MissingEmployee_Returns_BadRequest()
        {
            // Arrange
            var compensation = new Compensation()
            {
                Salary = 1.00m,
                EffectiveDate = new DateTime(2026, 1, 1),
            };

            // Execute
            var response = PostCompensation(compensation);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void GetCompensation_AfterCreate_Returns_Ok()
        {
            // Arrange
            var employee = CreateEmployee();
            var compensation = new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 120000.50m,
                EffectiveDate = new DateTime(2026, 3, 15),
            };
            PostCompensation(compensation);

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var fetched = response.DeserializeContent<Compensation>();
            Assert.AreEqual(employee.EmployeeId, fetched.Employee.EmployeeId);
            Assert.AreEqual(employee.FirstName, fetched.Employee.FirstName);
            Assert.AreEqual(compensation.Salary, fetched.Salary);
            Assert.AreEqual(compensation.EffectiveDate, fetched.EffectiveDate);
        }

        [TestMethod]
        public void GetCompensation_MultipleRecords_ReturnsCurrentlyActive()
        {
            // Arrange
            var employee = CreateEmployee();
            var currentEffectiveDate = DateTime.Today.AddMonths(-1);
            PostCompensation(new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 150000.00m,
                EffectiveDate = currentEffectiveDate,
            });
            PostCompensation(new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 100000.00m,
                EffectiveDate = DateTime.Today.AddYears(-1),
            });
            PostCompensation(new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 200000.00m,
                EffectiveDate = DateTime.Today.AddMonths(1),
            });

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var fetched = response.DeserializeContent<Compensation>();
            Assert.AreEqual(150000.00m, fetched.Salary);
            Assert.AreEqual(currentEffectiveDate, fetched.EffectiveDate);
        }

        [TestMethod]
        public void GetCompensation_OnlyFutureRecords_Returns_NotFound()
        {
            // Arrange
            var employee = CreateEmployee();
            PostCompensation(new Compensation()
            {
                Employee = new Employee() { EmployeeId = employee.EmployeeId },
                Salary = 200000.00m,
                EffectiveDate = DateTime.Today.AddMonths(1),
            });

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetCompensation_NoCompensation_Returns_NotFound()
        {
            // Arrange
            var employee = CreateEmployee();

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetCompensation_UnknownEmployee_Returns_NotFound()
        {
            // Execute
            var response = _httpClient.GetAsync("api/compensation/Invalid_Id").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetAllCompensation_ReturnsEveryRecordOldestFirst()
        {
            // Arrange
            var employee = CreateEmployee();
            var pastDate = DateTime.Today.AddYears(-1);
            var currentDate = DateTime.Today.AddMonths(-1);
            var futureDate = DateTime.Today.AddMonths(1);
            foreach (var (salary, date) in new[] { (150000.00m, currentDate), (100000.00m, pastDate), (200000.00m, futureDate) })
            {
                PostCompensation(new Compensation()
                {
                    Employee = new Employee() { EmployeeId = employee.EmployeeId },
                    Salary = salary,
                    EffectiveDate = date,
                });
            }

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}/all").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var fetched = response.DeserializeContent<List<Compensation>>();
            Assert.AreEqual(3, fetched.Count);
            Assert.AreEqual(pastDate, fetched[0].EffectiveDate);
            Assert.AreEqual(currentDate, fetched[1].EffectiveDate);
            Assert.AreEqual(futureDate, fetched[2].EffectiveDate);
            Assert.IsTrue(fetched.TrueForAll(c => c.Employee.EmployeeId == employee.EmployeeId));
        }

        [TestMethod]
        public void GetAllCompensation_NoCompensation_Returns_NotFound()
        {
            // Arrange
            var employee = CreateEmployee();

            // Execute
            var response = _httpClient.GetAsync($"api/compensation/{employee.EmployeeId}/all").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetAllCompensation_UnknownEmployee_Returns_NotFound()
        {
            // Execute
            var response = _httpClient.GetAsync("api/compensation/Invalid_Id/all").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
