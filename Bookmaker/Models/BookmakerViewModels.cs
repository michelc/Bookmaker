using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Bookmaker.Models
{
    /// <summary>
    /// Classe pour afficher la liste des brochures dans la vue Index
    /// </summary>
    public class BookletIndex
    {
        public int Booklet_ID { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public int TravelsCount1 { get; set; }
        public int TravelsCount2 { get; set; }
        public bool IsReadOnly { get; set; }

        public string TextReadOnly { get { return IsReadOnly ? "x" : ""; } }
    }

    /// <summary>
    /// Classe pour afficher la liste des tarifs d'une brochure
    /// </summary>
    public class PriceIndex
    {
        public int Price_ID { get; set; }
        public int Travel_ID { get; set; }
        public string TravelTitle { get; set; }
        public string Title { get; set; }
        public float Price1 { get; set; }
        public float Price2 { get; set; }
        public float Price3 { get; set; }
        public float Price4 { get; set; }
        public float Price5 { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Index
    /// </summary>
    public class TravelIndex
    {
        public int Travel_ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public TravelType TravelType { get; set; }
        public int PricesCount { get; set; }
        public int SectionsCount { get; set; }
    }

    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Search
    /// </summary>
    public class TravelSearch
    {
        public int Travel_ID { get; set; }
        public string BookletYear { get; set; }
        public int Booklet_ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public TravelType TravelType { get; set; }
        public string BookletTitle { get; set; }
    }

    /// <summary>
    /// Classe pour copier un voyage vers une autre brochure
    /// </summary>
    public class TravelCopy
    {
        public Travel Travel { get; set; }
        [Display(Name = "Copier vers la brochure ?")]
        public int Destination_ID { get; set; }
        public SelectList Destinations { get; set; }
    }

    /// <summary>
    /// Classe pour importer les sections d'un voyage
    /// </summary>
    public class SectionImport
    {
        public Travel Travel { get; set; }
        [Display(Name = "Contenu brut")]
        [DataType(DataType.MultilineText)]
        [Required]
        public string RawContent { get; set; }
    }

    /// <summary>
    /// Classe pour importer les tarifs d'un voyage
    /// </summary>
    public class PriceImport
    {
        public Travel Travel { get; set; }
        [Display(Name = "Tarifs bruts")]
        [DataType(DataType.MultilineText)]
        [Required]
        public string RawContent { get; set; }
    }

    /// <summary>
    /// Classe pour saisir la source d'un import Json
    /// </summary>
    public class JsonImport
    {
        [DataType(DataType.MultilineText)]
        [Display(Name = "Source JSON")]
        [Required]
        public string RawContent { get; set; }
    }
}