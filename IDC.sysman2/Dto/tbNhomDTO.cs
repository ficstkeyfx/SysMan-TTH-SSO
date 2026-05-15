using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SysMan.Models
{
    public partial class tbNhomDTO
    {

        [Key]
        public int IdNhom { get; set; }
        public string TenNhom { get; set; }
    }
}
