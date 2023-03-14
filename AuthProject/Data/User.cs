using Diplomka.Models.Abstract;
using Microsoft.AspNetCore.Identity;

namespace Diplomka.Models
{
    public class User : Entity
    {
        public IdentityUser IdentityUser { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

}
