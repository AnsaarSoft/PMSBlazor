using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSModels.Models
{
    public class MstSetting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public bool flgCapitalLetter { get; set; } = true;
        [Required]
        public bool flgSmallLetter { get; set; } =true;
        [Required]
        public bool flgNumbers { get; set; } = true;
        [Required]
        public bool flgSpecial { get; set; } = true;
        [Required]
        public int PasswodLenght { get; set; } = 16;

    }
}
