using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Models.Entity;

namespace TechAid.Service
{
    public class EmployeeService{

        private readonly ApplicationDbContext dbContext;

        public EmployeeService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Employee? AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var user = dbContext.Employees.FirstOrDefault(e => e.Email.ToLower() == addEmployeeDto.Email.ToLower());
            if (user is not null)
            {
                throw new Exception("User already exists with this email.");
            }
        

            var newEmployee = new Employee()
            {
                Email = addEmployeeDto.Email,
                Phone_number = addEmployeeDto.Phone_number,
                Password = BCrypt.Net.BCrypt.HashPassword(addEmployeeDto.Password),
                First_name = addEmployeeDto.First_name,
                Last_name = addEmployeeDto.Last_name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

           
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(newEmployee);
            bool isValid = Validator.TryValidateObject(newEmployee, validationContext, validationResults, true);

            if (!isValid)
            {
                string errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                throw new ArgumentException("Validation failed: " + errors);
            }

            dbContext.Employees.Add(newEmployee);
            dbContext.SaveChanges();

            return newEmployee;
        }

        public List<Employee> GetAllEmployees()
        {
            var allEmployees = dbContext.Employees.ToList();

            return allEmployees;
        }

        public Employee? GeEmployeeById(Guid id)
        {
            var employee = dbContext.Employees.Find(id);

            if(employee is null)
            {
                return null;
            }

            return employee;
        }


        public Employee? UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var update = dbContext.Employees.Find(id);

            if (update is null)
            {
                return null;
            }

            update.Phone_number = updateEmployeeDto.Phone_number;
            update.Password = updateEmployeeDto.Password;
            update.First_name = updateEmployeeDto.First_name;
            update.Last_name = updateEmployeeDto.Last_name;
            update.UpdatedAt = DateTime.Now;

            dbContext.SaveChanges();

            return update;
        }

        public string DeleteEmployee(Guid id)
        {
            var employee = dbContext.Employees.Find(id);

            if(employee is null)
            {
                return "Employee not found";
            }

            dbContext.Remove(employee);
            dbContext.SaveChanges();

            return "Employee deleted successfully";
        }


        public string Login(LoginDto loginDto)
        {
            var user = dbContext.Employees.FirstOrDefault(e => e.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                return "User does not exist";
            }

            if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password) && user.Email==loginDto.Email)
            {
                return "Login successful!";
            }

            return "Incorrect credentials";
        }

    }
}
