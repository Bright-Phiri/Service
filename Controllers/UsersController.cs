using API.Models;
using API2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace API.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        [Route("getAll")]
        [HttpGet]
        public IHttpActionResult GetAllUsers()
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
                    var users = db.Users.ToList();
                    var usersDto = new List<UserDto>();
                    foreach (var item in users)
                    {
                        usersDto.Add(UserDto.FromEFModel(item));
                    }
                    serviceCallResult.Remark = "Users pulled successfully";
                    serviceCallResult.data = usersDto;
                    return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                }
                catch (Exception ex)
                {
                    serviceCallResult.Remark = "Error while pulling users";
                    serviceCallResult.ResultCode = ResultCodeEnum.Error;
                    return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                }
            }
        }

        [Route("assignUser")]
        [HttpPost]
        public IHttpActionResult AssignUser(UserDto userDetails)
        {
            if (ModelState.IsValid)
            {
                var serviceCallResult = new ServiceCallResult<object>()
                {
                    ResultCode = ResultCodeEnum.Successful,
                    Remark = "user successfully added",
                    data = null
                };
                using (var db = new CRUDEntities())
                {
                    using (var dbTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var user = userDetails.ToEFModel();
                            var role = db.Roles.Where(r => r.Id == userDetails.RoleId).FirstOrDefault();
                            if (role != null)
                            {
                                role.Users.Add(user);
                                db.SaveChanges();
                                dbTransaction.Commit();
                                serviceCallResult.data = UserDto.FromEFModel(user);
                                return Content<ServiceCallResult<object>>(HttpStatusCode.Created, serviceCallResult);
                            }
                            else
                            {
                                serviceCallResult.Remark = "Role not found";
                                serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                                return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            serviceCallResult.Remark = "Error while adding user";
                            serviceCallResult.ResultCode = ResultCodeEnum.Error;
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
        public IHttpActionResult EditUser(UserDto userDetails)
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
                            var editUser = db.Users.Where(u => u.Id == userDetails.Id).FirstOrDefault();
                            if (editUser != null)
                            {
                                updateDetails(ref editUser, userDetails);
                                db.SaveChanges();
                                dbTransaction.Commit();
                                serviceCallResult.Remark = "User successfully updated";
                                serviceCallResult.data = UserDto.FromEFModel(editUser);
                                return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                            }
                            else
                            {
                                serviceCallResult.Remark = "User record not found";
                                serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                                serviceCallResult.data = null;
                                return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();
                            serviceCallResult.Remark = "Error while updating user record";
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

        private void updateDetails(ref User editUser, UserDto userDetails)
        {
            //Convert dto to ef model for processing
            var updateDetails = userDetails.ToEFModel();

            //Add all navigation properties so they can be skipped
            var skipProperties = new string[] { "id", "Role" };

            //loop through properties and update
            foreach (PropertyInfo prop in editUser.GetType().GetProperties())
            {
                if (!skipProperties.Contains(prop.Name))
                {
                    prop.SetValue(editUser, prop.GetValue(updateDetails));
                }
            }
        }

    }
}
