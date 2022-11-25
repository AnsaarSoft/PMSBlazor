using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSModels.Models
{
    public class MstCard
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string CardName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Alias { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string UserCode { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;
        [StringLength(300)]
        public string WebLink { get; set; } = string.Empty;
        [Required]
        public bool IsActive { get; set; } = true;
        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
