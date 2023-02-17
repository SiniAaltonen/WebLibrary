using System;
using System.Collections.Generic;

namespace Kirjasto.Models
{
    public partial class Kirjailija
    {
        public Kirjailija()
        {
            Kirjas = new HashSet<Kirja>();
        }

        public int KirjailijaId { get; set; }
        public string Etunimi { get; set; } = null!;
        public string Sukunimi { get; set; } = null!;

        public virtual ICollection<Kirja> Kirjas { get; set; }
    }
}
