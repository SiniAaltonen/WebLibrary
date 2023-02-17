using System;
using System.Collections.Generic;

namespace Kirjasto.Models
{
    public partial class Lainau
    {
        public int LainausId { get; set; }
        public DateTime LainausPvm { get; set; }
        public DateTime? EräPvm { get; set; }
        public int? KirjaId { get; set; }
        public int? KäyttäjäId { get; set; }
        public virtual Kirja? Kirja { get; set; }
        public virtual Käyttäjä? Käyttäjä { get; set; }
    }
}
