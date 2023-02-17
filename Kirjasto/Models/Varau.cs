using System;
using System.Collections.Generic;

namespace Kirjasto.Models
{
    public partial class Varau
    {
        public int VarausId { get; set; }
        public int? KirjaId { get; set; }

        public int KäyttäjäId { get; set; }
        public virtual Kirja? Kirja { get; set; }
    }
}
