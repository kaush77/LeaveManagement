using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagement.Controllers
{     
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveAllocationRepository _leaveAllocRepo;
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManger;

        public LeaveRequestController(ILeaveRequestRepository leaveRequestRepo, ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocRepo,
            IMapper mapper, UserManager<Employee> userManger)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocRepo = leaveAllocRepo;
            _mapper = mapper;
            _userManger = userManger;
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var leaveRequests = _leaveRequestRepo.FindAll();
            var leaveRequestModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);

            var model = new AdminLeaveRequestViewVM
            {
                TotalRequest = leaveRequestModel.Count,
                ApprovedRequest = leaveRequestModel.Count(q => q.Approved == true),
                PendingRequest = leaveRequestModel.Count(q => q.Approved == null),
                RejectedRequest = leaveRequestModel.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModel
            };

            return View(model);
        }

        // GET: LeaveRequest/Details/5
        public ActionResult Details(int id)
        {

            var leaveRequest = _leaveRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        public ActionResult ApproveRequest(int id)
        {
            try
            {
                var user = _userManger.GetUserAsync(User).Result;
                var leaveRequest = _leaveRequestRepo.FindById(id);
                var allocation = _leaveAllocRepo.GetLeaveAllocationByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);

                
                var startDate = Convert.ToDateTime(leaveRequest.StartDate);
                var endDate = Convert.ToDateTime(leaveRequest.EndDate);
                int daysRequested = (int)(endDate - startDate).TotalDays;

                allocation.NumberOfDays = allocation.NumberOfDays - daysRequested;

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var isSuccess = _leaveRequestRepo.Update(leaveRequest);

                if (isSuccess)
                {
                    _leaveAllocRepo.Update(allocation);
                }

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                return RedirectToAction(nameof(Index));
            } 
        }

        public ActionResult RejectRequest(int id)
        {
            try
            {
                var user = _userManger.GetUserAsync(User).Result;
                var leaveRequest = _leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var isSuccess = _leaveRequestRepo.Update(leaveRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        
        public ActionResult MyLeave()
        {
            try
            {
                var employee = _userManger.GetUserAsync(User).Result;
                var employeeid = employee.Id;
                var employeeAllocation = _leaveAllocRepo.GetLeaveAllocationByEmployee(employeeid);
                var employeeRequests = _leaveRequestRepo.GetLeaveRequestByEmployee(employeeid);

                var employeeAllocationModel = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocation);
                var employeeRequestModel = _mapper.Map<List<LeaveRequestVM>>(employeeRequests);

                var model = new EmployeeLeaveRequestViewVM
                {
                    leaveAllocations = employeeAllocationModel,
                    LeaveRequests = employeeRequestModel
                };

                return View(model);

            }
            catch(Exception ex)
            {
                return View();
            } 
        }

        public ActionResult CancelRequest(int id)
        {
            //var leaverequest = _leaveRequestRepo.FindById(id);
            //leaverequest.can = true;
            //_leaveRequestRepo.Update(leaverequest);
            return RedirectToAction("MyLeave");
        }

        // GET: LeaveRequest/Create
        public ActionResult Create()
        {
            var leaveTypes = _leaveTypeRepo.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem {
                Text =q.Name, 
                Value = q.Id.ToString() 
            });
            var model = new CreatedLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };

            return View(model);
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreatedLeaveRequestVM model)
        { 
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);

                var leaveTypes = _leaveTypeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });

                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if(DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start date cannot be further in the future than the End date");
                    return View(model);
                }
                var employee = _userManger.GetUserAsync(User).Result;
                var allocation = _leaveAllocRepo.GetLeaveAllocationByEmployeeAndType(employee.Id,model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays; 

                if(daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficent days for this request");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComments = model.RequestComments
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var IsSuccess = _leaveRequestRepo.Update(leaveRequest);

                if (!IsSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong with submitting your record");
                    return View(model);
                }

                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequest/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}