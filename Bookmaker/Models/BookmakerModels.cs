using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Bookmaker.Models
{
    public enum TravelType : int
    {
        Journee = 1,
        Sejour = 2
    }

    public enum SectionType : int
    {
        Titre = 1,
        Presentation = 2,
        Texte = 3,
        Menu = 4,
        Image = 5
    }

    public class Travel
    {
        // dentifiant automatique du voyage
        [Key]
        public int TravelID { get; set; }

        // Position (ordre) du voyage
        [Display(Name = "Position")]
        public int Position { get; set; }

        // Titre du voyage
        [Required]
        [Display(Name = "Titre")]
        [StringLength(100)]
        public string Title { get; set; }

        // Type du voyage : journée ou séjour
        [Required]
        [Display(Name = "Type de voyage")]
        public TravelType TypeTravel
        {
            get { return (TravelType)TravelType; }
            set { TravelType = (int)value; }
        }
        public int TravelType { get; set; }

        // Commentaire sur ce voyage
        [Display(Name = "Remarques")]
        [Column(TypeName = "ntext")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        // Description des parties du voyage, morceau par morceau
        public virtual ICollection<Section> Sections { get; set; }

        // Tarifs du voyage
        public virtual ICollection<Price> Prices { get; set; }
    }

    public class Section
    {
        // Identifiant automatique de la partie
        [Key]
        public int SectionID { get; set; }

        // Référence du voyage auquel correspond cette partie
        [Display(Name = "Voyage")]
        public int TravelID { get; set; }
        public virtual Travel Travel { get; set; }

        // Position (ordre) de cette partie dans le voyage
        [Display(Name = "Position")]
        public int Position { get; set; }

        // Type de la partie (introduction, sous-titre, menu, tarif...)
        [Required]
        [Display(Name = "Type de contenu")]
        public SectionType TypeSection
        {
            get { return (SectionType)SectionType; }
            set { SectionType = (int)value; }
        }
        public int SectionType { get; set; }

        // Texte pour le contenu de la partie
        [Required]
        [Display(Name = "Contenu")]
        [Column(TypeName = "ntext")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
    }

    public class Price
    {
        // Identifiant automatique du tarif
        [Key]
        public int PriceID { get; set; }

        // Référence du voyage auquel correspond le tarif
        [Display(Name = "Voyage")]
        public int TravelID { get; set; }
        public virtual Travel Travel { get; set; }

        // Année du tarif
        [Required]
        [Display(Name = "Année")]
        [StringLength(20)]
        public string Year { get; set; }

        // Libellé pour décrire le tarif
        [Required]
        [Display(Name = "Titre")]
        [StringLength(50)]
        public string Title { get; set; }

        // Prix du voyage pour un groupe de 30 à 34 personnes
        [Display(Name = "Tarif 30 à 34 personnes")]
        public float Price1 { get; set; }

        // Prix du voyage pour un groupe de 35 à 39 personnes
        [Display(Name = "Tarif 35 à 39 personnes")]
        public float Price2 { get; set; }

        // Prix du voyage pour un groupe de 40 à 44 personnes
        [Display(Name = "Tarif 40 à 44 personnes")]
        public float Price3 { get; set; }

        // Prix du voyage pour un groupe de 45 à 49 personnes
        [Display(Name = "Tarif 45 à 49 personnes")]
        public float Price4 { get; set; }

        // Prix du voyage pour un groupe de 50 à 55 personnes
        [Display(Name = "Tarif 50 à 55 personnes")]
        public float Price5 { get; set; }

        // Commentaire sur ce tarif
        [Display(Name = "Remarques")]
        [Column(TypeName = "ntext")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
    }

    public class BookmakerContext : DbContext
    {
        public DbSet<Travel> Travels { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Section> Sections { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookmakerContext, Configuration>());
        }

        public void TruncateTable(string tableName, string idName)
        {
            // Vide la table
            this.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0} WHERE {1} IS NOT NULL", tableName, idName));

            // Réinitialise la numérotation automatique
            try
            {
                // SQL Server CE
                this.Database.ExecuteSqlCommand(string.Format("ALTER TABLE {0} ALTER COLUMN {1} IDENTITY (1, 1)", tableName, idName));
            }
            catch { }
            try
            {
                // SQL Server pas CE
                this.Database.ExecuteSqlCommand(string.Format("TRUNCATE TABLE {0}", tableName));
            }
            catch { }
        }
    }

    public class Configuration : DbMigrationsConfiguration<BookmakerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}