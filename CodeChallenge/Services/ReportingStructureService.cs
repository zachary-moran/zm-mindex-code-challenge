using System;
using System.Collections.Generic;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Services
{
    public class ReportingStructureService : IReportingStructureService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<ReportingStructureService> _logger;

        public ReportingStructureService(ILogger<ReportingStructureService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public ReportingStructure GetByEmployeeId(string id)
        {
            if (String.IsNullOrEmpty(id))
                return null;

            var employee = _employeeRepository.GetDirectReports(id);
            if (employee == null)
                return null;

            return new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = CountReports(employee, new HashSet<string> { employee.EmployeeId })
            };
        }

        private int CountReports(Employee employee, ISet<string> visited)
        {
            var count = 0;
            foreach (var report in employee.DirectReports ?? new List<Employee>())
            {
                if (!visited.Add(report.EmployeeId))
                    continue;

                // Tracked query: EF hands back the same instance as `report` and fills its DirectReports,
                // so the returned tree ends up fully populated as a side effect.
                var loadedReport = _employeeRepository.GetDirectReports(report.EmployeeId);
                count += 1 + CountReports(loadedReport, visited);
            }
            return count;
        }
    }
}
