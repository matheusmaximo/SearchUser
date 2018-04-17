﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SearchUser.Entities.Models
{
    /// <summary>
    /// Create 
    /// </summary>
    public interface IBaseEntity
    {
        /// <summary>
        /// User creation date
        /// </summary>
        DateTime? CreatedOn { get; set; }

        /// <summary>
        /// User last update date
        /// </summary>
        DateTime? LastUpdatedOn { get; set; }
    }
}
