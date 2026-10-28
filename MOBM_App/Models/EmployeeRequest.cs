using System;

namespace MOBM_App.Models
{
    public class EmployeeRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public Role DesiredRole { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string AdminComment { get; set; }
    }
}