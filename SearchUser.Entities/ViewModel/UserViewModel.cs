using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SearchUser.Entities.ViewModel
{
    public class UserViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public ICollection<TelephoneViewModel> Telephones { get; set; }

        public UserViewModel()
        {
            this.Telephones = new Collection<TelephoneViewModel>();
        }
    }
}
