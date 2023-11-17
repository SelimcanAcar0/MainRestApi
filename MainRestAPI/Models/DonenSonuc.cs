using System.Collections.Generic;

namespace MainRestApi.Models
{
    public class DonenSonuc
    {
        public string Mesaj { get; set; }
        public List<dynamic> SonucListe { get; set; }
        public bool Sonuc { get; set; }
    }
}