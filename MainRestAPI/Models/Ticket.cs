using System;

namespace MainRestApi.Models
{
    public class Ticket
    {
        public string KullaniciAdi { get; set; }
        public Guid ID { get; set; }
        public bool Sonuc { get; set; }
    }

    public class Kullanici
    {
        public string KullaniciAdi { get; set; }


        public string Sifre { get; set; }
    }
}