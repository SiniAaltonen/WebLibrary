using System;
using System.Collections.Generic;

namespace Kirjasto.Models
{
    public partial class Kirja
    {
        public Kirja()
        {
            Arviointis = new HashSet<Arviointi>();
            Lainaus = new HashSet<Lainau>();
            Varaus = new HashSet<Varau>();
        }

        public int KirjaId { get; set; }
        public string Nimi { get; set; } = null!;
        public string? Kuvaus { get; set; }
        public string? Tyyppi { get; set; }
        public string? Avainsanat { get; set; }
        public bool? NäytäKuvaus { get; set; }
        public bool? Lainassa { get; set; }
        public int? KirjailijaId { get; set; }

        public virtual Kirjailija? Kirjailija { get; set; }
        public virtual ICollection<Arviointi> Arviointis { get; set; }
        public virtual ICollection<Lainau> Lainaus { get; set; }
        public virtual ICollection<Varau> Varaus { get; set; }
    }
}
