// ViewModel/UserManagementViewModel.cs
using SYM_CONNECT.Models; 

namespace SYM_CONNECT.ViewModel
{
    public class UserManagementViewModel 
    {
        public List<Users> Users { get; set; } = new List<Users>(); //get lists of users
        public RegisterViewModel Form { get; set; } = new RegisterViewModel(); //get viewmodel inputs
    }
}