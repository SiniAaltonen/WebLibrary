using System;
using System.Collections.Generic;

namespace Kirjasto.Models
{
    public partial class Arviointi
    {
        public int ArvioId { get; set; }
        public int? Arvosana { get; set; }
        public int? KäyttäjäId { get; set; }
        public int? KirjaId { get; set; }

        public virtual Kirja? Kirja { get; set; }
        public virtual Käyttäjä? Käyttäjä { get; set; }
    }
}
