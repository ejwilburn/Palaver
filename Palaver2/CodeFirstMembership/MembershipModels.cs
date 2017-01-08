using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;


namespace CodeFirstMembership.Models
{
    public class User
    {

        //Membership required
		[Required()]
        [Key()]
        public Guid UserId { get; set; }
        [Required()]
        [MaxLength(20)]
        public string Username { get; set; }
        [Required()]
        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required()]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsConfirmed { get; set; }
        public int PasswordFailuresSinceLastSuccess { get; set; }
        public DateTime? LastPasswordFailureDate { get; set; }
        public string ConfirmationToken { get; set; }
		[Required()]
        public DateTime CreateDate { get; set; }
        public DateTime? PasswordChangedDate { get; set; }
        public string PasswordVerificationToken { get; set; }
        public DateTime? PasswordVerificationTokenExpirationDate { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        //Optional
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TimeZone { get; set; }
        public string Culture { get; set; }

    }

    public class Role
    {
        //Membership required
		[Required()]
        [Key()]
        public virtual Guid RoleId { get; set; }
        [Required()]
        [MaxLength(100)]
        public virtual string RoleName { get; set; }

        public virtual ICollection<User> Users { get; set; }

        //Optional
        [MaxLength(250)]
        public virtual string Description { get; set; }
    }

}