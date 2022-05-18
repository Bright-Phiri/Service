using API;
using API.Models;
using API2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace API2.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : ApiController
    {
        [Route("getAll")]
        [HttpGet]
        public IHttpActionResult GetAllStudents()
        {
            var serviceCallResult = new ServiceCallResult<object>()
            {
                ResultCode = ResultCodeEnum.Successful,
            };
            var db = new CRUDEntities();
            try
            {
                var students = db.Students.ToList();
                var studentsDTO = new List<StudentDto>();
                foreach (var item in students)
                {
                    studentsDTO.Add(StudentDto.FromEFModel(item));
                }
                serviceCallResult.Remark = "Students pulled successfully";
                serviceCallResult.data = studentsDTO;
                return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
            }
            catch (Exception ex)
            {
                serviceCallResult.ResultCode = ResultCodeEnum.Error;
                serviceCallResult.Remark = $"Error while loading students {ex}";
                serviceCallResult.data = null;
                return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
            }
        }

        [Route("getStudent/{id}")]
        [HttpGet]
        public IHttpActionResult GetStudent(int id)
        {
            var serviceCallResult = new ServiceCallResult<object>()
            {
                ResultCode = ResultCodeEnum.Successful,
                data = null
            };
            using (var db = new CRUDEntities())
            {
                try
                {
                    var student = db.Students.Where(s => s.StudentID == id).FirstOrDefault();
                    if (student != null)
                    {
                        serviceCallResult.Remark = "Student loaded";
                        serviceCallResult.data = StudentDto.FromEFModel(student);
                        return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                    }
                    else
                    {
                        serviceCallResult.Remark = "Student record not found";
                        serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                        return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                    }
                }
                catch (Exception ex)
                {
                    serviceCallResult.Remark = "Error";
                    serviceCallResult.ResultCode = ResultCodeEnum.Error;
                    return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                }
            }
        }


        [Route("add")]
        [HttpPost]
        public IHttpActionResult AddStudent(StudentDto studentDetails)
        {
            if (ModelState.IsValid)
            {
                var serviceCallResult = new ServiceCallResult<object>()
                {
                    ResultCode = ResultCodeEnum.Successful,
                    Remark = "Student successfully added",
                    data = null
                };
                using (var db = new CRUDEntities())
                {
                    using (var dbTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var studentRecord = studentDetails.ToEFModel();
                            db.Students.Add(studentRecord);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            serviceCallResult.data = StudentDto.FromEFModel(studentRecord);
                            return Content<ServiceCallResult<object>>(HttpStatusCode.Created, serviceCallResult);
                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();
                            serviceCallResult.ResultCode = ResultCodeEnum.Error;
                            serviceCallResult.Remark = "Error while saving student record";
                            return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                        }
                    }
                }
            }
            else
            {
                return BadRequest("Check parameters");
            }
        }

        [Route("update")]
        [HttpPut]
        public IHttpActionResult UpdateStudent(StudentDto studentDetails)
        {
            if (ModelState.IsValid)
            {
                var serviceCallResult = new ServiceCallResult<object>()
                {
                    ResultCode = ResultCodeEnum.Successful
                };
                using (var db = new CRUDEntities())
                {
                    using (var dbTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var editStudent = db.Students.Where(s => s.StudentID == studentDetails.StudentID).FirstOrDefault();
                            if (editStudent != null)
                            {
                                updateDetails(ref editStudent, studentDetails);
                                db.SaveChanges();
                                dbTransaction.Commit();
                                serviceCallResult.Remark = "Student successfully updated";
                                serviceCallResult.data = StudentDto.FromEFModel(editStudent);
                                return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                            }
                            else
                            {
                                serviceCallResult.Remark = "Student record not found";
                                serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                                serviceCallResult.data = null;
                                return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                            }

                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();
                            serviceCallResult.Remark = "Error while updating student record";
                            serviceCallResult.ResultCode = ResultCodeEnum.Error;
                            serviceCallResult.data = null;
                            return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                        }
                    }
                }
            }
            else
            {
                return BadRequest("Check parameters");
            }
        }

        [Route("delete/{id}")]
        [HttpDelete]
        public IHttpActionResult DeleteStudent(int id)
        {
            var serviceCallResult = new ServiceCallResult<object>()
            {
                ResultCode = ResultCodeEnum.Successful,
                data = null,
            };
            using (var db = new CRUDEntities()) {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var student = db.Students.Where(s => s.StudentID == id).FirstOrDefault();
                        if (student != null)
                        {
                            db.Students.Remove(student);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            serviceCallResult.data = StudentDto.FromEFModel(student);
                            serviceCallResult.Remark = "Student successfully deleted";
                            return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                        }
                        else
                        {
                            serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                            serviceCallResult.Remark = "Student record not found";
                            return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        serviceCallResult.ResultCode = ResultCodeEnum.Error;
                        serviceCallResult.Remark = "Error while deleting student";
                        return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                    }
                }
            }
            
        }

        private void updateDetails(ref Student editStudent, StudentDto studentDetails)
        {
            //Convert dto to ef model for processing
            var updateDetails = studentDetails.ToEFModel();

            //Add all navigation properties so they can be skipped
            var skipProperties = new string[] { "id"};

            //loop through properties and update
            foreach (PropertyInfo prop in editStudent.GetType().GetProperties())
            {
                if (!skipProperties.Contains(prop.Name))
                {
                    prop.SetValue(editStudent, prop.GetValue(updateDetails));
                }
            }
        }
    }
}
