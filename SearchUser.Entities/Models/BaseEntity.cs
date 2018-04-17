using System;
using System.Collections.Generic;
using System.Text;

namespace SearchUser.Entities.Models
{
    public interface IBaseEntity
    {
        DateTime? CreatedOn { get; set; }

        DateTime? LastUpdatedOn { get; set; }
    }
}
