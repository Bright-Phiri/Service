//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace API.Models
{
    using System;
    using System.Collections.Generic;
    
    using FluentValidation.Internal;
    using FluentValidation.Resources;
    using FluentValidation.Results;
    using FluentValidation;
    using FluentValidation.Validators;
    
     public partial class UserDto
    {
        
        //Default Constructor
        public UserDto(){}
        
        public  class AbstractValidatorEF : AbstractValidator<UserDto>
        {
            public AbstractValidatorEF()
            {
              SetValidationRules();
            }
            
            public virtual void SetValidationRules()
            {
                RuleFor(x => x.Id).NotNull();
                RuleFor(x => x.Username).MaximumLength(50).NotEmpty();
                RuleFor(x => x.Email).MaximumLength(50).NotEmpty();
                RuleFor(x => x.Phone).MaximumLength(50).NotEmpty();
               
            }
       
        }
    
        
        
        
        /// <summary>
        /// Converts User Dto to User EF Model
        /// </summary>
        /// <returns>User EF model</returns>
        public User ToEFModel()
        {
            var model = new User();
            model.Id = this.Id;
            model.Username = this.Username;
            model.Email = this.Email;
            model.Phone = this.Phone;
            model.RoleId = this.RoleId;
            return model;
        }
    
    
        // <summary>
        /// Creates a Dto from a User Entity Model
        /// </summary>
        /// <param name="model">User model</param>
        /// <returns>User Dto</returns>
        public static UserDto FromEFModel(User model)
        {
            if(model == null) return null;
            var dto = new UserDto();
            dto.Id = model.Id;
            dto.Username = model.Username;
            dto.Email = model.Email;
            dto.Phone = model.Phone;
            dto.RoleId = model.RoleId;
            return dto;
        }
    
    
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Nullable<int> RoleId { get; set; }
    }
}
