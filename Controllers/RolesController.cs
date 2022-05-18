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
    [RoutePrefix("api/roles")]
    public class RolesController : ApiController
    {

        [Route("getAll")]
        [HttpGet]
        public IHttpActionResult GetAllRoles()
        {
            var serviceCallResult = new ServiceCallResult<object>()
            {
                ResultCode = ResultCodeEnum.Successful,
                data = null 
            };
            using(var db = new CRUDEntities())
            {
                try
                {
                    var roles = db.Roles.ToList();
                    var rolesDto = new List<RoleDto>();
                    foreach (var item in roles)
                    {
                        rolesDto.Add(RoleDto.FromEFModel(item));
                    }
                    serviceCallResult.Remark = "Roles pulled succssfully";
                    serviceCallResult.data = rolesDto;
                    return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                }
                catch (Exception ex)
                {
                    serviceCallResult.ResultCode = ResultCodeEnum.Error;
                    serviceCallResult.Remark = "Error occured";
                    return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                }
            }
        }

        [Route("usersCount/{id}")]
        [HttpGet]
        public IHttpActionResult GetUsersCount(int id)
        {
            var serviceCallResult = new ServiceCallResult<int>()
            {
                ResultCode = ResultCodeEnum.Successful,
                Remark = "Count retrieved successfully!"
            };
            using (var db = new CRUDEntities())
            {
                try
                {
                    var role = db.Roles.Where(r => r.Id == id).FirstOrDefault();
                    if (role != null)
                    {
                        var usersCount = db.Entry(role).Collection(r => r.Users).Query().Count();
                        serviceCallResult.data = usersCount;
                        return Content<ServiceCallResult<int>>(HttpStatusCode.OK,serviceCallResult);
                    }
                    else
                    {
                        serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                        serviceCallResult.Remark = "Role not found";
                        serviceCallResult.data = 0;
                        return Content<ServiceCallResult<int>>(HttpStatusCode.NotFound, serviceCallResult);
                    }
                } catch (Exception ex)
                {
                    serviceCallResult.ResultCode = ResultCodeEnum.Error;
                    serviceCallResult.Remark = "An Error Occured. Contact ICT.";
                    serviceCallResult.data = 0;
                    return Content<ServiceCallResult<int>>(HttpStatusCode.InternalServerError, serviceCallResult);
                }
            }
        }

        [Route("getRole/{id")]
        [HttpGet]
        public IHttpActionResult GetRole(int id)
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
                    var role = db.Roles.Where(r => r.Id == id).FirstOrDefault();
                    if (role != null)
                    {
                        serviceCallResult.Remark = "Role loaded";
                        serviceCallResult.data = RoleDto.FromEFModel(role);
                        return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                    }
                    else
                    {
                        serviceCallResult.Remark = "Role not found";
                        serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                        return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                    }
                } catch (Exception ex)
                {
                    serviceCallResult.Remark = $"Error{ex}";
                    serviceCallResult.ResultCode = ResultCodeEnum.Error;
                    return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                }
            }
        }

        [Route("add")]
        [HttpPost]
        public IHttpActionResult AddRole(RoleDto roleDetails)
        {
            if (ModelState.IsValid)
            {
                var serviceCallResult = new ServiceCallResult<object>()
                {
                    ResultCode = ResultCodeEnum.Successful,
                    data = null
                };
                using (var db = new CRUDEntities()) {
                    using (var dbTransaction = db.Database.BeginTransaction()) {
                        try
                        {
                            var role = roleDetails.ToEFModel();
                            db.Roles.Add(role);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            serviceCallResult.Remark = "Role successfully added";
                            serviceCallResult.data = RoleDto.FromEFModel(role);
                            return Content<ServiceCallResult<object>>(HttpStatusCode.Created, serviceCallResult);
                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();
                            serviceCallResult.Remark = "Error while saving role";
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

        [Route("edit")]
        [HttpPut]
        public IHttpActionResult UpdateRole(RoleDto roleDetails)
        {
            if (ModelState.IsValid)
            {
                var serviceCallResult = new ServiceCallResult<object>()
                {
                    ResultCode = ResultCodeEnum.Successful,
                    data = null
                };
                using (var db = new CRUDEntities()) {
                    using (var dbTransaction = db.Database.BeginTransaction()){
                        try
                        {
                            var editRole = db.Roles.Where(r => r.Id == roleDetails.Id).FirstOrDefault();
                            if (editRole != null)
                            {
                                updateDetails(ref editRole, roleDetails);
                                db.SaveChanges();
                                dbTransaction.Commit();
                                serviceCallResult.Remark = "Role successfully updated";
                                serviceCallResult.data = RoleDto.FromEFModel(editRole);
                                return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                            }
                            else
                            {
                                serviceCallResult.Remark = "Role record not found";
                                serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                                return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            dbTransaction.Rollback();
                            serviceCallResult.Remark = "Error while updating role";
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

        [Route("delete/{id}")]
        [HttpPut]
        public IHttpActionResult DeleteRole(int id)
        {
            var serviceCallResult = new ServiceCallResult<object>()
            {
                ResultCode = ResultCodeEnum.Successful,
                data = null,
            };
            using (var db = new CRUDEntities())
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var role = db.Roles.Where(r => r.Id == id).FirstOrDefault();
                        if (role != null)
                        {
                            db.Roles.Remove(role);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            serviceCallResult.data = RoleDto.FromEFModel(role);
                            serviceCallResult.Remark = "Role successfully deleted";
                            return Content<ServiceCallResult<object>>(HttpStatusCode.OK, serviceCallResult);
                        }
                        else
                        {
                            serviceCallResult.ResultCode = ResultCodeEnum.Fail;
                            serviceCallResult.Remark = "Role record not found";
                            return Content<ServiceCallResult<object>>(HttpStatusCode.NotFound, serviceCallResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        serviceCallResult.ResultCode = ResultCodeEnum.Error;
                        serviceCallResult.Remark = "Error while deleting role";
                        return Content<ServiceCallResult<object>>(HttpStatusCode.InternalServerError, serviceCallResult);
                    }
                }
            }
        }

        private void updateDetails(ref Role editRole, RoleDto roleDetails)
        {
            //Convert dto to ef model for processing
            var updateDetails = roleDetails.ToEFModel();

            //Add all navigation properties so they can be skipped
            var skipProperties = new string[] { "id","Users" };

            //loop through properties and update
            foreach (PropertyInfo prop in editRole.GetType().GetProperties())
            {
                if (!skipProperties.Contains(prop.Name))
                {
                    prop.SetValue(editRole, prop.GetValue(updateDetails));
                }
            }
        }
    }
}
