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
        MenuLong = 5,
        Image = 6,
        Tarif = 7,
        TarifNotes = 8,
        TexteNotes = 9
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
        [Required(ErrorMessage = "L'information «Titre du voyage» est obligatoire")]
        [Display(Name = "Titre")]
        [StringLength(100)]
        public string Title { get; set; }

        // Type du voyage : journée ou séjour
        [Required(ErrorMessage = "L'information «Type du voyage» est obligatoire")]
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
        [Required(ErrorMessage = "L'information «Type de contenu» est obligatoire")]
        [Display(Name = "Type de contenu")]
        public SectionType TypeSection
        {
            get { return (SectionType)SectionType; }
            set { SectionType = (int)value; }
        }
        public int SectionType { get; set; }

        // Texte pour le contenu de la partie
        [Required(ErrorMessage = "L'information «Contenu» est obligatoire")]
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
        [Required(ErrorMessage = "L'information «Année du tarif» est obligatoire")]
        [Display(Name = "Année")]
        [StringLength(20)]
        public string Year { get; set; }

        // Libellé pour décrire le tarif
        [Required(ErrorMessage = "L'information «Titre du tarif» est obligatoire")]
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