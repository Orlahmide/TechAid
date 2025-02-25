using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Dto.ResponseDto;
using TechAid.Interface;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Service
{
    public class EmployeeService{

        private readonly ApplicationDbContext dbContext;
        private readonly ITokenGenerator itoken;

         private readonly IConfiguration configuration;

        public EmployeeService(ApplicationDbContext dbContext, ITokenGenerator tokenGenerator)
        {
            this.dbContext = dbContext;
            this.itoken = tokenGenerator;
        }

        public EmployeeResponse AddEmployee(AddEmployeeDto addEmployeeDto)
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
                UpdatedAt = DateTime.Now,
                Role = addEmployeeDto.Role,
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

            return new EmployeeResponse{ first_name = newEmployee.First_name, last_name= newEmployee.Last_name,
            CreatedAt = newEmployee.CreatedAt, email = newEmployee.Email, Phone_number = newEmployee.Phone_number, 
            Id = newEmployee.Id, role = newEmployee.Role, Department= newEmployee.Department};
        }

        public List<EmployeeResponse> GetAllEmployees()
        {
            var employees = dbContext.Employees.ToList();

            var employeeResponses = employees.Select(e => new EmployeeResponse
            {
                first_name = e.First_name,
                last_name = e.Last_name,
                CreatedAt = e.CreatedAt,
                email = e.Email,
                Phone_number = e.Phone_number,
                Id = e.Id,
                role = e.Role,
            }).ToList();

            return employeeResponses;
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


        public EmployeeResponse? UpdateEmployee(Guid? id, UpdateEmployeeDto updateEmployeeDto, Department? department)
        {
            if (id is null)
            {
                throw new ArgumentException("Employee ID cannot be null.");
            }

            var update = dbContext.Employees.Find(id);
            if (update is null)
            {
                return null; // Or throw an exception if preferred
            }

            // Update fields only if they have valid values
            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Phone_number))
            {
                update.Phone_number = updateEmployeeDto.Phone_number;
            }

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Password))
            {
                update.Password = BCrypt.Net.BCrypt.HashPassword(updateEmployeeDto.Password);
            }

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.First_name))
            {
                update.First_name = updateEmployeeDto.First_name;
            }

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Last_name))
            {
                update.Last_name = updateEmployeeDto.Last_name;
            }

            if (department is not null)
            {
                update.Department = department;
            }

            update.UpdatedAt = DateTime.Now;

            try
            {
                dbContext.Employees.Update(update);
                dbContext.SaveChanges();

                return new EmployeeResponse
                {
                    Id = update.Id,
                    first_name = update.First_name,
                    last_name = update.Last_name,
                    email = update.Email,
                    Phone_number = update.Phone_number,
                    Department = update.Department,
                    CreatedAt = update.CreatedAt,
                    UpdatedAt = update.UpdatedAt,
                    role = update.Role,
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating employee: {ex.Message}");
                return null;
            }
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


        public LoginResponse? Login(LoginDto loginDto)
        {
            var user = dbContext.Employees.FirstOrDefault(e => e.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return new LoginResponse { Confirmation = "Invalid credentials", Token = null, RefreshToken = null };
            }

            var accessToken = itoken.GenerateAccessToken(user.Id, user.Role);
            var refreshToken = itoken.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(int.Parse(configuration["JwtSettings:RefreshTokenExpiryDays"]));

            dbContext.SaveChanges();

            return new LoginResponse
            {
                Confirmation = "Login successful",
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

          public string Logout(Guid userId)
        {
            var user = dbContext.Employees.Find(userId);

            if (user == null)
            {
                return "User not found";
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            dbContext.SaveChanges();

            return "User logged out successfully";
        }

    }

    
}
