using System.ComponentModel.DataAnnotations;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Migrations;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Service
{
    public class ITPersonelService
    {
        private readonly ApplicationDbContext dbContext;

        public ITPersonelService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ITPersonel AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var user = dbContext.ITPersonels.FirstOrDefault(e => e.Email.ToLower() == addEmployeeDto.Email.ToLower());
            if (user is not null)
            {
                throw new Exception("User already exists with this email.");
            }

            var newEmployee = new ITPersonel()
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

            dbContext.ITPersonels.Add(newEmployee);
            dbContext.SaveChanges();

            return newEmployee;
        }

        public List<ITPersonel> GetAllEmployees()
        {
            var allEmployees = dbContext.ITPersonels.ToList();

            return allEmployees;
        }

        public ITPersonel? GeEmployeeById(Guid id)
        {
            var employee = dbContext.ITPersonels.Find(id);

            if (employee is null)
            {
                return null;
            }

            return employee;
        }


        public ITPersonel? UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var update = dbContext.ITPersonels.Find(id);

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
            var employee = dbContext.ITPersonels.Find(id);

            if (employee is null)
            {
                return "Employee not found";
            }

            dbContext.Remove(employee);
            dbContext.SaveChanges();

            return "Employee deleted successfully";
        }


        public string Login(LoginDto loginDto)
        {
            var user = dbContext.ITPersonels.FirstOrDefault(e => e.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                return "User does not exist";
            }

            if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password) && user.Email == loginDto.Email)
            {
                return "Login successful!";
            }

            return "Incorrect credentials";
        }


        public string AssignTicket(int id, Guid idd)
        {
            var tic = dbContext.Tickets.Find(id);
            var itPersonnel = dbContext.ITPersonels.Find(idd);

            if (tic == null)
            {
                return "Ticket does not exist";
            }
            if (itPersonnel == null)
            {
                return "IT Personnel does not exist";
            }

            tic.ITPersonelId = itPersonnel.Id; 
            tic.Status = Status.ACTIVE;
            tic.UpdatedAt = DateTime.Now;

            dbContext.SaveChanges(); 

            return $"Ticket {id} has been assigned to {itPersonnel.First_name} {itPersonnel.Last_name}";
        }


    }
}
