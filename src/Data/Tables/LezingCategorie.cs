using System.Collections.Generic;

namespace Data.Tables
{
    public class LezingCategorie
    {
        public int Id { get; set; }  
        public string Omschrijving { get; set; } 
        public int? OudId { get; set; }  

        public virtual ICollection<Preek> Preeks { get; set; }  

        public LezingCategorie()
        {
            Preeks = new List<Preek>(); 
        }
    }

}
