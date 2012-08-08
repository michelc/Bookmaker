using System.ComponentModel.DataAnnotations;

namespace Bookmaker.Models
{
    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Index
    /// </summary>
    public class VoyageIndex
    {
        public int VoyageID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public int VoyageType { get; set; }
        public int PricesCount { get; set; }
        public int PartiesCount { get; set; }

        public VoyageType TypeVoyage
        {
            get { return (VoyageType)VoyageType; }
            set { VoyageType = (int)value; }
        }
    }

    public class VoyageImport
    {
        public Voyage Voyage { get; set; }

        [Display(Name = "Contenu complet")]
        [DataType(DataType.MultilineText)]
        public string Contenu { get; set; }
    }
}