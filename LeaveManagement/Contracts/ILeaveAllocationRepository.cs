using LeaveManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts
{
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        bool CheckAllocation(int leavetypeid, string employeeId);

        ICollection<LeaveAllocation> GetLeaveAllocationByEmployee(string employeId);

        LeaveAllocation GetLeaveAllocationByEmployeeAndType(string employeId, int leavetypeid);
    }
}
