using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Bookmaker.Models
{
    public enum TravelType : int
    {
        [Display(Name = "Journée")]
        Journee = 1,
        [Display(Name = "Séjour")]
        Sejour = 2
    }

    public enum SectionType : int
    {
        Titre = 1,
        Texte = 2,
        Menu = 3,
        [Display(Name = "Menu centré")]
        Menu_Centre = 4,
        Image = 5,
        Tarif = 6
    }

    public class Booklet
    {
        // Identifiant automatique de la brochure
        [Key]
        public int Booklet_ID { get; set; }

        // Titre de la brochure
        [Required]
        [Display(Name = "Titre")]
        [StringLength(100)]
        public string Title { get; set; }

        // Année de la brochure
        [Required]
        [Display(Name = "Année")]
        [StringLength(20)]
        public string Year { get; set; }

        // Commentaires sur cette brochure
        [Display(Name = "Remarques")]
        [Column(TypeName = "ntext")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        // Voyages de la brochure
        public virtual ICollection<Travel> Travels { get; set; }
    }

    public class Travel
    {
        // Identifiant automatique du voyage
        [Key]
        public int Travel_ID { get; set; }

        // Référence de la brochure dont fait parti le voyage
        [Display(Name = "Brochure")]
        public int Booklet_ID { get; set; }
        public virtual Booklet Booklet { get; set; }

        // Position (ordre) du voyage
        [Display(Name = "Position")]
        public int Position { get; set; }

        // Titre du voyage
        [Required]
        [Display(Name = "Titre")]
        [StringLength(100)]
        public string Title { get; set; }

        // Suite du titre du voyage
        [Display(Name = "Sous-Titre")]
        [StringLength(100)]
        public string Subtitle { get; set; }

        // Type du voyage : journée ou séjour
        [Required]
        [Display(Name = "Type de voyage")]
        [UIHint("Enum")]
        public TravelType TravelType
        {
            get { return (TravelType)TravelType_Int; }
            set { TravelType_Int = (int)value; }
        }
        [Column("TravelType")]
        public int TravelType_Int { get; set; }

        // Commentaires sur ce voyage
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
        public int Section_ID { get; set; }

        // Référence du voyage auquel correspond cette partie
        [Display(Name = "Voyage")]
        public int Travel_ID { get; set; }
        public virtual Travel Travel { get; set; }

        // Position (ordre) de cette partie dans le voyage
        [Display(Name = "Position")]
        public int Position { get; set; }

        // Type de la partie (introduction, sous-titre, menu, tarif...)
        [Required]
        [Display(Name = "Type de contenu")]
        [UIHint("Enum")]
        public SectionType SectionType
        {
            get { return (SectionType)SectionType_Int; }
            set { SectionType_Int = (int)value; }
        }
        [Column("SectionType")]
        public int SectionType_Int { get; set; }

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
        public int Price_ID { get; set; }

        // Référence du voyage auquel correspond le tarif
        [Display(Name = "Voyage")]
        public int Travel_ID { get; set; }
        public virtual Travel Travel { get; set; }

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

        // Commentaires sur ce tarif
        [Display(Name = "Remarques")]
        [Column(TypeName = "ntext")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
    }

    public class BookmakerContext : DbContext
    {
        public DbSet<Booklet> Booklets { get; set; }
        public DbSet<Travel> Travels { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Section> Sections { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookmakerContext, Configuration>());
        }

        private void ExecuteSql(string sql)
        {
            this.Database.ExecuteSqlCommand(sql);
        }

        public void RefillPositions(string tableName, string parentColumn, int parentId, int currentPosition)
        {
            var sql = string.Format("UPDATE {0} SET Position = Position - 1 WHERE {1} = {2} AND Position > {3}", tableName, parentColumn, parentId, currentPosition);
            this.ExecuteSql(sql);
        }

        public bool SortPositions(string tableName, string parentColumn, int parentId, int from, int to)
        {
            var success = true;

            try
            {
                // Les positions démarrent à 1
                // (alors que les index jQuery commencent à 0)
                from++;
                to++;

                var sql = string.Empty;

                // Met de côté l'élément à la position de départ
                sql = string.Format("UPDATE {0} SET Position = 0 WHERE {1} = {2} AND Position = {3}", tableName, parentColumn, parentId, from);
                this.ExecuteSql(sql);

                if (from < to)
                {
                    // Ramène d'un rang tous les élements entre le départ et l'arrivée
                    sql = string.Format("UPDATE {0} SET Position = Position - 1 WHERE {1} = {2} AND Position BETWEEN {3} AND {4}", tableName, parentColumn, parentId, from, to);
                    this.ExecuteSql(sql);
                }
                else
                {
                    // Repousse d'un rang tous les élements entre l'arrivée et le départ
                    sql = string.Format("UPDATE {0} SET Position = Position + 1 WHERE {1} = {2} AND Position BETWEEN {3} AND {4}", tableName, parentColumn, parentId, to, from);
                    this.ExecuteSql(sql);
                }

                // Déplace l'élément mis de coté à la position d'arrivée
                sql = string.Format("UPDATE {0} SET Position = {3} WHERE {1} = {2} AND Position = 0", tableName, parentColumn, parentId, to);
                this.ExecuteSql(sql);
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public void TruncateTable(string tableName, string idColumn)
        {
            // Vide la table
            this.ExecuteSql(string.Format("DELETE FROM {0} WHERE {1} IS NOT NULL", tableName, idColumn));

            // Réinitialise la numérotation automatique
            try
            {
                // SQL Server CE
                this.ExecuteSql(string.Format("ALTER TABLE {0} ALTER COLUMN {1} IDENTITY (1, 1)", tableName, idColumn));
            }
            catch { }
            try
            {
                // SQL Server pas CE (ne semble pas fonctionner !)
                this.ExecuteSql(string.Format("TRUNCATE TABLE {0}", tableName));
                this.ExecuteSql(string.Format("DBCC CHECKIDENT('{0}', RESEED, 0)", tableName));
            }
            catch { }
        }

        /// <summary>
        /// Détermine si une brochure peut être mise à jour
        /// </summary>
        /// <param name="Booklet_ID">Identifiant de la brochurer à tester</param>
        /// <returns>True si la màj est possible</returns>
        public bool BookletIsUpdatable(int Booklet_ID)
        {
            // Retrouve la brochure concernée
            var booklet = this.Booklets.Find(Booklet_ID);

            // Sa màj n'est pas possible si elle n'existe pas
            if (booklet == null) return false;

            // Sa màj est interdite si elle est clôturée
            if (booklet.Booklet_ID == 1)
            {
                return false;
            }

            // La màj ne fait pas de difficulté
            return true;
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