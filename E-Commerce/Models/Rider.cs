using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Rider
    {
        [Key] public int RiderId { get; set; }

        [Required] public int UserId { get; set; }
        [ForeignKey("UserId")] public UserModel User { get; set; }
        [Required] public string FullName { get; set; }
        [Required] public string ContactNumber { get; set; }
        [Required] public string VehicleInfo { get; set; }

        [NotMapped]
        public bool IsProfileComplete =>
            !string.IsNullOrEmpty(FullName) &&
            !string.IsNullOrEmpty(ContactNumber) &&
            !string.IsNullOrEmpty(VehicleInfo);

        public ICollection<Transaction> Transactions { get; set; }
    }
}
