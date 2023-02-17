using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kirjasto.Models
{
    public partial class KirjastoDBContext : DbContext
    {
        public KirjastoDBContext()
        {
        }

        public KirjastoDBContext(DbContextOptions<KirjastoDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Arviointi> Arviointis { get; set; } = null!;
        public virtual DbSet<Kirja> Kirjas { get; set; } = null!;
        public virtual DbSet<Kirjailija> Kirjailijas { get; set; } = null!;
        public virtual DbSet<Käyttäjä> Käyttäjäs { get; set; } = null!;
        public virtual DbSet<Lainau> Lainaus { get; set; } = null!;
        public virtual DbSet<Varau> Varaus { get; set; } = null!;

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                optionsBuilder.UseSqlServer("Server=tcp:kirjasto.database.windows.net,1433;Initial Catalog=KirjastoDB;Persist Security Info=False;User ID=kirjasto;Password=Salainen1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
//            }
//        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Arviointi>(entity =>
            {
                entity.HasKey(e => e.ArvioId)
                    .HasName("PK__Arvioint__1A6423CE0A54BC21");

                entity.ToTable("Arviointi");

                entity.Property(e => e.ArvioId).HasColumnName("ArvioID");

                entity.Property(e => e.KirjaId).HasColumnName("KirjaID");

                entity.Property(e => e.KäyttäjäId).HasColumnName("KäyttäjäID");

                entity.HasOne(d => d.Kirja)
                    .WithMany(p => p.Arviointis)
                    .HasForeignKey(d => d.KirjaId)
                    .HasConstraintName("FK__Arviointi__Kirja__5EBF139D");

                entity.HasOne(d => d.Käyttäjä)
                    .WithMany(p => p.Arviointis)
                    .HasForeignKey(d => d.KäyttäjäId)
                    .HasConstraintName("FK__Arviointi__Käytt__5DCAEF64");
            });

            modelBuilder.Entity<Kirja>(entity =>
            {
                entity.ToTable("Kirja");

                entity.Property(e => e.KirjaId).HasColumnName("KirjaID");

                entity.Property(e => e.Avainsanat).HasMaxLength(250);

                entity.Property(e => e.KirjailijaId).HasColumnName("KirjailijaID");

                entity.Property(e => e.Kuvaus).HasMaxLength(500);

                entity.Property(e => e.Lainassa).HasDefaultValueSql("((0))");

                entity.Property(e => e.Nimi).HasMaxLength(100);

                entity.Property(e => e.NäytäKuvaus).HasDefaultValueSql("((0))");

                entity.Property(e => e.Tyyppi).HasMaxLength(15);

                entity.HasOne(d => d.Kirjailija)
                    .WithMany(p => p.Kirjas)
                    .HasForeignKey(d => d.KirjailijaId)
                    .HasConstraintName("FK__Kirja__Kirjailij__4316F928");
            });

            modelBuilder.Entity<Kirjailija>(entity =>
            {
                entity.ToTable("Kirjailija");

                entity.Property(e => e.KirjailijaId).HasColumnName("KirjailijaID");

                entity.Property(e => e.Etunimi).HasMaxLength(30);

                entity.Property(e => e.Sukunimi).HasMaxLength(30);
            });

            modelBuilder.Entity<Käyttäjä>(entity =>
            {
                entity.ToTable("Käyttäjä");

                entity.Property(e => e.KäyttäjäId).HasColumnName("KäyttäjäID");

                entity.Property(e => e.Etunimi).HasMaxLength(30);

                entity.Property(e => e.Käyttäjänimi).HasMaxLength(30);

                entity.Property(e => e.OnkoAdmin).HasDefaultValueSql("((0))");

                entity.Property(e => e.Salasana).HasMaxLength(20);

                entity.Property(e => e.Sukunimi).HasMaxLength(30);
            });

            modelBuilder.Entity<Lainau>(entity =>
            {
                entity.HasKey(e => e.LainausId)
                    .HasName("PK__Lainaus__831E662B05D26E37");

                entity.Property(e => e.LainausId).HasColumnName("LainausID");

                entity.Property(e => e.EräPvm).HasColumnType("datetime");

                entity.Property(e => e.KirjaId).HasColumnName("KirjaID");

                entity.Property(e => e.KäyttäjäId).HasColumnName("KäyttäjäID");

                entity.Property(e => e.LainausPvm).HasColumnType("datetime");

                entity.HasOne(d => d.Kirja)
                    .WithMany(p => p.Lainaus)
                    .HasForeignKey(d => d.KirjaId)
                    .HasConstraintName("FK__Lainaus__KirjaID__59063A47");

                entity.HasOne(d => d.Käyttäjä)
                    .WithMany(p => p.Lainaus)
                    .HasForeignKey(d => d.KäyttäjäId)
                    .HasConstraintName("FK__Lainaus__Käyttäj__59FA5E80");
            });

            modelBuilder.Entity<Varau>(entity =>
            {
                entity.HasKey(e => e.VarausId)
                    .HasName("PK__Varaus__F08C1A5E695BB5D7");

                entity.Property(e => e.VarausId).HasColumnName("VarausID");

                entity.Property(e => e.KirjaId).HasColumnName("KirjaID");

                entity.HasOne(d => d.Kirja)
                    .WithMany(p => p.Varaus)
                    .HasForeignKey(d => d.KirjaId)
                    .HasConstraintName("FK__Varaus__KirjaID__4BAC3F29");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
