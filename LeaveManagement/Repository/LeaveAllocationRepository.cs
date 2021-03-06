﻿using LeaveManagement.Contracts;
using LeaveManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool CheckAllocation(int leavetypeid, string employeeId)
        {
            var period = DateTime.Now.Year;
            return FindAll()
                .Where(q => q.EmployeeId == employeeId && q.LeaveTypeId == leavetypeid && q.Period == period)
                .Any();
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return Save();
        }

        public ICollection<LeaveAllocation> FindAll()
        {
            return _db.LeaveAllocations.
                Include(q=>q.LeaveType).
                Include(q=>q.Employee).
                ToList();
        }

        public LeaveAllocation FindById(int id)
        {
            return _db.LeaveAllocations.
                Include(q => q.LeaveType).
                Include(q => q.Employee).
                FirstOrDefault(q=>q.Id == id);
        }

        public ICollection<LeaveAllocation> GetLeaveAllocationByEmployee(string employeId)
        {
            var period = DateTime.Now.Year;

            return FindAll().
                Where(q => q.EmployeeId == employeId && q.Period == period).
                ToList();
        }

        public LeaveAllocation GetLeaveAllocationByEmployeeAndType(string employeId, int leavetypeid)
        {
            var period = DateTime.Now.Year;

            return FindAll().
                FirstOrDefault(q => q.EmployeeId == employeId && q.Period == period && q.LeaveTypeId == leavetypeid);
                
        }

        public bool isExists(int id)
        {
            var exists = _db.LeaveAllocations.Any(q => q.Id == id);
            return exists;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return Save();
        }
    }
}
