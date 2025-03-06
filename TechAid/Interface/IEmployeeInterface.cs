using TechAid.Dto.ResponseDto;
using TechAid.Dto;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Interface
{
    public interface IEmployeeInterface
    {
        EmployeeResponse AddEmployee(AddEmployeeDto addEmployeeDto);
        List<EmployeeResponse> GetAllEmployees();
        Employee? GeEmployeeById(Guid? id);
        EmployeeResponse? UpdateEmployee(Guid? id, UpdateEmployeeDto updateEmployeeDto, Department? department);
        string DeleteEmployee(Guid id);
        LoginResponse? Login(LoginDto loginDto);
        string Logout(Guid? userId);
    }
}
