using CodeChallenge.Models;
using System;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public interface IEmployeeRepository
    {
        Employee GetById(String id);
        Employee GetDirectReports(String id);
        Employee Add(Employee employee);
        Employee Update(Employee existingEmployee, Employee newEmployee);
        Employee Remove(Employee employee);
        Task SaveAsync();
    }
}