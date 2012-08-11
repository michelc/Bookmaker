namespace Bookmaker.Models
{
    /// <summary>
    /// Classe pour afficher la liste des voyages dans la vue Index
    /// </summary>
    public class TravelIndex
    {
        public int TravelID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public int TravelType { get; set; }
        public int PricesCount { get; set; }
        public int SectionsCount { get; set; }

        public TravelType TypeTravel
        {
            get { return (TravelType)TravelType; }
            set { TravelType = (int)value; }
        }
    }
}