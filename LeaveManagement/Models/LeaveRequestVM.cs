using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Models
{
    public class LeaveRequestVM
    {
        public int Id { get; set; }
        
        public EmployeeVM RequestingEmployee { get; set; }

        [Display(Name="Employee Name")]
        public string RequestingEmployeeId { get; set; }

        [Display(Name = "Start Date")]
        [Required]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public DateTime EndDate { get; set; }
         
        public LeaveTypeVM LeaveType { get; set; }

        public int LeaveTypeId { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime DateRequested { get; set; }

        [Display(Name = "Date Actioned")]
        public DateTime DateActioned { get; set; }

        [Display(Name = "Approval State")]
        public bool? Approved { get; set; }
         
        public EmployeeVM AproveBy { get; set; }

        [Display(Name = "Approver Name")]
        public string ApprovedById { get; set; }


        [Display(Name="Employee Comments")]
        [MaxLength(300)]
        public string RequestComments { get; set; }
    }

    public class AdminLeaveRequestViewVM
    {
        [Display(Name ="Total Number Of Requests")]
        public int TotalRequest { get; set; }

        [Display(Name = "Approved Requests")]
        public int ApprovedRequest { get; set; }

        [Display(Name = "Pending Requests")]
        public int PendingRequest { get; set; }

        [Display(Name = "Rejected Requests")]
        public int RejectedRequest { get; set; }
        public List<LeaveRequestVM> LeaveRequests { get; set; }

    }

    public class CreatedLeaveRequestVM
    {
        [Display(Name="Start Date")]
        [Required] 
        public string StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required] 
        public string EndDate { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; }

        [Display(Name ="Leave Type")]
        public int LeaveTypeId { get; set; }

        [Display(Name = "Employee Comments")]
        [MaxLength(300)]
        public string RequestComments { get; set; }
    }

    public class EmployeeLeaveRequestViewVM
    {
        public List<LeaveAllocationVM> leaveAllocations { get; set; }
        public List<LeaveRequestVM> LeaveRequests { get; set; }
    }
}
