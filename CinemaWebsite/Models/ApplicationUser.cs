using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaWebsite.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}