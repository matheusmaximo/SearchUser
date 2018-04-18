using System;

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
