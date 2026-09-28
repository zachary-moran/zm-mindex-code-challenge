using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeChallenge.Models;

namespace CodeChallenge.Repositories
{
    public interface ICompensationRepository
    {
        Compensation Add(Compensation compensation);
        Compensation GetByEmployeeId(String employeeId);
        List<Compensation> GetAllByEmployeeId(String employeeId);
        Task SaveAsync();
    }
}
