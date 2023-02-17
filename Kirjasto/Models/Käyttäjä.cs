using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kirjasto.Models
{
    public partial class Käyttäjä
    {
        public Käyttäjä()
        {
            Arviointis = new HashSet<Arviointi>();
            Lainaus = new HashSet<Lainau>();
            Varaus = new HashSet<Varau>();
        }

        public int KäyttäjäId { get; set; }
        public string? Etunimi { get; set; }
        public string? Sukunimi { get; set; }

        [Required]
        public string Käyttäjänimi { get; set; } = null!;
        public string? Salasana { get; set; }
        public bool? OnkoAdmin { get; set; }

        public bool? VarausSaapunut { get; set; }

        public virtual ICollection<Arviointi> Arviointis { get; set; }
        public virtual ICollection<Lainau> Lainaus { get; set; }

        public virtual ICollection<Varau> Varaus { get; set; }

    }
}
