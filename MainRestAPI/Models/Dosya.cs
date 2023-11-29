namespace MainRestApi.Models
{
    public class Dosya
    {
        public int ID { get; set; }

        public string Adi { get; set; }

        public int Boyut { get; set; }
    }

    public class DosyaYayinlaBilgi
    {

        public string DosyaAdi { get; set; }
        public string KlasorYolu { get; set; }
    }


}