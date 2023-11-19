using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscgolfBot.Data.Models
{
    public class Disc
    {
        public int Id { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Speed { get; set; }
        public decimal Glide { get; set; }
        public decimal Turn { get; set; }
        public decimal Fade { get; set; }
    }
}
