using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchUser.Entities.Models
{
    /// <summary>
    /// Application user class
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public ICollection<Telephone> Telephones { get; set; }

        #region Constructor
        public ApplicationUser()
        {
            this.Telephones = new Collection<Telephone>();
        }
        #endregion

        #region Control properties
        /// <summary>
        /// User creation date
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// User last update date
        /// </summary>
        public DateTime? LastUpdatedOn { get; set; }

        /// <summary>
        /// User last login date
        /// </summary>
        public DateTime? LastLoginOn { get; set; }

        [NotMapped]
        public string Token { get; set; }
        #endregion
    }
}
