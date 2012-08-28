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
    }

    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Index
    /// </summary>
    public class TravelIndex
    {
        public int Travel_ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public int TravelType { get; set; }
        public int PricesCount { get; set; }
        public int SectionsCount { get; set; }

        public TravelType TypeTravel
        {
            get { return (TravelType)TravelType; }
        }
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
}