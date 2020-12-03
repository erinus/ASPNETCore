using System;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        public DateTime? Time { get; set; }

        public String Code { get; set; }

        public Double? Open { get; set; }
        public Double? High { get; set; }
        public Double? Low { get; set; }
        public Double? Close { get; set; }
    }
}
