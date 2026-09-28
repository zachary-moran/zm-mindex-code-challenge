using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Data;
using CodeChallenge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Repositories
{
    public class CompensationRepository : ICompensationRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<ICompensationRepository> _logger;

        public CompensationRepository(ILogger<ICompensationRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Compensation Add(Compensation compensation)
        {
            compensation.CompensationId = Guid.NewGuid().ToString();
            _employeeContext.Compensations.Add(compensation);
            return compensation;
        }

        public Compensation GetByEmployeeId(string employeeId)
        {
            return _employeeContext.Compensations
                .Include(c => c.Employee)
                .Where(c => c.Employee.EmployeeId == employeeId && c.EffectiveDate <= DateTime.Now)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefault();
        }

        public List<Compensation> GetAllByEmployeeId(string employeeId)
        {
            return _employeeContext.Compensations
                .Include(c => c.Employee)
                .Where(c => c.Employee.EmployeeId == employeeId)
                .OrderBy(c => c.EffectiveDate)
                .ToList();
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }
    }
}
