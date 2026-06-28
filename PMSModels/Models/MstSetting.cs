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
        public bool UseUppercase { get; set; } = true;
        public bool UseLowercase { get; set; } = true;
        public bool UseNumbers { get; set; } = true;
        public bool UseSymbols { get; set; } = true;
        [Range(6, 20)]
        public int PasswordLength { get; set; } = 16;

    }
}
