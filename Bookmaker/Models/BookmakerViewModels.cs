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
        public bool HasNotes { get; set; }

        // Impossible de faire àa au niveau de Mapper.CreateMap<Price, PriceIndex>().ForMember(...)
        // car on ne peut pas mélanger des auto-projection LINQ avec des ForMember qui ne peuvent
        // pas être transformés en code SQL
        // https://github.com/AutoMapper/AutoMapper/issues/134#issuecomment-3098563
        public string TextPrice1 { get { return Price1 == Price5 ? "" : Price1.ToString(); } }
        public string TextPrice2 { get { return Price1 == Price5 ? "" : Price2.ToString(); } }
        public string TextPrice3 { get { return Price1 == Price5 ? "" : Price3.ToString(); } }
        public string TextPrice4 { get { return Price1 == Price5 ? "" : Price4.ToString(); } }
        public string TextPrice5 { get { return Price5.ToString(); } }
        public string TextNotes { get { return HasNotes ? "x" : ""; } }
    }

    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Index
    /// </summary>
    public class TravelIndex
    {
        public int Travel_ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public int TravelType_Int { get; set; }
        public int PricesCount { get; set; }
        public int SectionsCount { get; set; }

        public TravelType TravelType
        {
            get { return (TravelType)TravelType_Int; }
        }
    }

    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Search
    /// </summary>
    public class TravelSearch
    {
        public int Travel_ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public int TravelType_Int { get; set; }
        public int Booklet_ID { get; set; }
        public string BookletTitle { get; set; }

        public TravelType TravelType
        {
            get { return (TravelType)TravelType_Int; }
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