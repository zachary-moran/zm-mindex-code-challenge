using System;
using System.Collections.Generic;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Services
{
    public class CompensationService : ICompensationService
    {
        private readonly ICompensationRepository _compensationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<CompensationService> _logger;

        public CompensationService(ILogger<CompensationService> logger, ICompensationRepository compensationRepository, IEmployeeRepository employeeRepository)
        {
            _compensationRepository = compensationRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Compensation Create(Compensation compensation)
        {
            var employeeId = compensation?.Employee?.EmployeeId;
            if (String.IsNullOrEmpty(employeeId))
                return null;

            var employee = _employeeRepository.GetById(employeeId);
            if (employee == null)
                return null;

            compensation.Employee = employee;
            _compensationRepository.Add(compensation);
            _compensationRepository.SaveAsync().Wait();

            return compensation;
        }

        public Compensation GetByEmployeeId(string employeeId)
        {
            if (String.IsNullOrEmpty(employeeId))
                return null;

            return _compensationRepository.GetByEmployeeId(employeeId);
        }

        public List<Compensation> GetAllByEmployeeId(string employeeId)
        {
            if (String.IsNullOrEmpty(employeeId))
                return null;

            return _compensationRepository.GetAllByEmployeeId(employeeId);
        }
    }
}
