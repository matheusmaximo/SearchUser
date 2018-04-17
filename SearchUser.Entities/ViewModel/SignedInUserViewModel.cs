
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SearchUser.Entities.ViewModel
{
    public class SignedInUserViewModel
    {
        public string Id { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime? LastLoginOn { get; set; }

        public string Token { get; set; }
    }
}
