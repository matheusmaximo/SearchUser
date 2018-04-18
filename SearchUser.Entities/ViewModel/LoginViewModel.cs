namespace SearchUser.Entities.ViewModel
{
    /// <summary>
    /// Model to be used to login
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User email provided on login
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User password provided on login
        /// </summary>
        public string Password { get; set; }
    }
}
