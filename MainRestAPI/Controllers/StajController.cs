using MainRestApi.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace MainRestApi.Controllers
{

    public class StajController : ApiController
    {
        private const int mb1 = 1024 * 1024;

        //klasör ve dosyaların bulunduğu kök yolumuzdur.
        private string rootPath = System.Web.Hosting.HostingEnvironment.MapPath("\\Content\\");

        //Adlandırma yaparken yasaklı karakterlerin dizesidir.
        public static readonly HashSet<char> yasakliKarakterler
            = new HashSet<char>() { '*', '"', '/', '\'', '<', '>', ':', '|', '?' };

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Ticket ticketGlobal = new Ticket
        {
            ID = Guid.Parse("),
            KullaniciAdi = "",
            Sonuc = true
        };

        public static void logKayit(Exception hata, string mesaj)
        {
            logger.Error(hata.Message + " | " + mesaj);
        }
        private static void scriptEncodeInput(object input)
        {
            if (string.IsNullOrEmpty(input.ToString()))
            {
                System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(input.ToString(), true);
            }
        }


        [HttpPost]
        public string MevcutKlasoruAl()
        {
            string webRootPath = "metutKlasor";//System.Web.Hosting.HostingEnvironment.MapPath("\\Content\\");
            return webRootPath;
        }

        [HttpPost]
        public Ticket TicketAl([FromBody] Kullanici kullanici)
        {
            Ticket ticket = new Ticket();
            if (kullanici.KullaniciAdi == "" && kullanici.Sifre == "")
            {
                ticket = ticketGlobal;
            }
            return ticket;
        }

        #region KLASÖR İŞLEMLERİ
        [HttpPost]
        public DonenSonuc KlasorListesiGetir([FromBody] KlasorListesiModel klasorListesiModel)
        {
            List<object> klasorListesi = new List<object>();
            DonenSonuc donenSonuc;
            string[] GelenKlasorler;
            Guid ticketID = klasorListesiModel.ticketID;
            string klasorYolu = klasorListesiModel.klasorYolu;


            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Content klasörü getirildi",
                        Sonuc = true
                    };
                    klasorYolu = "";
                }

                scriptEncodeInput(ticketID);

                scriptEncodeInput(klasorYolu);

                klasorYolu = rootPath + klasorYolu.Trim();


                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor" + klasorYolu,
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                GelenKlasorler = Directory.GetDirectories(klasorYolu);

                if (GelenKlasorler.Count() <= 0)
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor yolu boş",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                int i = 0;
                foreach (string gelenklasor in GelenKlasorler)
                {
                    i += 1;
                    DirectoryInfo KlasorBilgi = new DirectoryInfo(gelenklasor);
                    Klasor klasor = new Klasor
                    {
                        ID = i,
                        Adi = KlasorBilgi.Name
                    };

                    klasorListesi.Add(klasor);
                }

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Tüm Klasorler Getirildi",
                    SonucListe = klasorListesi,
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };
                logKayit(e, "KlasorListesiGetir metodunda hata oluştu");
                return donenSonuc;
            }

            return donenSonuc;
        }

        [HttpPost]
        public DonenSonuc KlasorOlustur(Guid ticketID, string klasorAdi, string klasorYolu)
        {
            DonenSonuc donenSonuc = new DonenSonuc();
            string[] GelenKlasorler;
            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Content klasörü getirildi",
                        Sonuc = true
                    };
                    klasorYolu = "";
                }

                klasorYolu = rootPath + klasorYolu.Trim();

                scriptEncodeInput(klasorAdi);

                scriptEncodeInput(klasorYolu);

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }


                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (klasorAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(klasorAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör adına Yasaklı Karakter girildi veya boş bırakıldı",
                        Sonuc = false
                    };

                    return donenSonuc;
                }
                //Eğer klasor yolunda aynı adda klasör varsa eklememe kontrolü
                GelenKlasorler = Directory.GetDirectories(klasorYolu);
                foreach (string klasorler in GelenKlasorler)
                {
                    DirectoryInfo klasorBilgi = new DirectoryInfo(klasorler);
                    if (klasorBilgi.Name == klasorAdi)
                    {
                        donenSonuc = new DonenSonuc
                        {
                            Mesaj = "Aynı adda klasör bulunuyor bu yüzden oluşturamayız",
                            Sonuc = false
                        };
                        return donenSonuc;
                    }
                }

                klasorYolu += klasorAdi.Trim();
                Debug.WriteLine(klasorYolu);
                Directory.CreateDirectory(klasorYolu);

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Klasor Oluşturuldu",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };

                logKayit(e, "KlasorOlustur metodunda bir sorun oluştu");
                return donenSonuc;
            }

            return donenSonuc;
        }

        [HttpDelete]
        public DonenSonuc KlasorSil(Guid ticketID, string klasorAdi, string klasorYolu)
        {
            DonenSonuc donenSonuc;

            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor Yolu Boş bırakılmış",
                        Sonuc = false
                    };
                    return donenSonuc;
                }


                scriptEncodeInput(klasorAdi);
                scriptEncodeInput(klasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                klasorYolu += klasorAdi; //Klasör yolunda hiç bir hata bulunmadığında en son silinecek klasor eklenir
                Directory.Delete(klasorYolu);
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Başarıyla silindi",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };

                logKayit(e, "KlasorSil metodunda bir sorun oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPut]
        public DonenSonuc KlasorGuncelle(Guid ticketID, string klasorYolu, string klasorAdi, string yeniKlasorAdi)
        {
            DonenSonuc donenSonuc = new DonenSonuc();

            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(klasorAdi) || string.IsNullOrEmpty(yeniKlasorAdi))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = " Boş Bırakılan Değer veya Değerler var",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                scriptEncodeInput(klasorAdi);
                scriptEncodeInput(yeniKlasorAdi);
                scriptEncodeInput(klasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (yeniKlasorAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(yeniKlasorAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör adına Yasaklı Karakter girildi veya boş bırakıldı",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                string eskiKlasor = klasorYolu + klasorAdi;
                Directory.Move(eskiKlasor, klasorYolu + yeniKlasorAdi);
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Klasör başarıyla güncellendi",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };

                logKayit(e, "KlasorGuncelle metodunda bir sorun oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPut]
        public DonenSonuc KlasorTasi(Guid ticketID, string klasorYolu, string klasorAdi, string yeniKlasorYolu)
        {
            DonenSonuc donenSonuc;
            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(klasorAdi) || string.IsNullOrEmpty(yeniKlasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                scriptEncodeInput(ticketID);
                scriptEncodeInput(klasorAdi);
                scriptEncodeInput(klasorYolu);
                scriptEncodeInput(yeniKlasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }
                if (!yeniKlasorYolu.EndsWith("/"))
                {
                    yeniKlasorYolu += "\\";
                }

                if (!Directory.Exists(yeniKlasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Yeni Klasor Yolu Bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                string mevcutKlasor = klasorYolu + klasorAdi;
                Directory.Move(mevcutKlasor, yeniKlasorYolu + klasorAdi);
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Klasör Başarıyla Taşındı",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };
                logKayit(e, "KlasorTasi metodunda hata oluştu");
                return donenSonuc;
            }

            return donenSonuc;
        }

        #endregion

        #region DOSYA İŞLEMLERİ
        [HttpGet]
        public DonenSonuc DosyaListesiGetir(Guid ticketID, string klasorYolu)
        {
            List<object> dosyaListesi = new List<object>();
            DonenSonuc donenSonuc;


            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar Uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör Yolu Boş Bırakılmış",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                scriptEncodeInput(ticketID);
                scriptEncodeInput(klasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();
                DirectoryInfo KlasorBilgi = new DirectoryInfo(klasorYolu);

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör Yolu Bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                FileInfo[] Dosyalar = KlasorBilgi.GetFiles();

                if (Dosyalar.Count() <= 0)
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör Boş",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                int i = 0;
                foreach (FileInfo gelenDosyaBilgi in Dosyalar)
                {
                    i += 1;
                    Dosya dosya = new Dosya
                    {
                        ID = i,
                        Adi = gelenDosyaBilgi.Name,
                        Boyut = (int)gelenDosyaBilgi.Length
                    };

                    dosyaListesi.Add(dosya);
                }
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosyalar başarıyla getirildi",
                    SonucListe = dosyaListesi,
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = $"Metod Çalıştırılırken Bir Sorunla Karşılaşıldı {e}",
                };

                logKayit(e, "DosyaListesiGetir metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPost]
        public DonenSonuc DosyaOlustur(Guid ticketID, string klasorYolu, string dosyaAdi)
        {
            DonenSonuc donenSonuc = new DonenSonuc();


            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(dosyaAdi))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Boş bırakılmış Değer veya Değerler var ",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                scriptEncodeInput(klasorYolu);
                scriptEncodeInput(dosyaAdi);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (dosyaAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(dosyaAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör adına Yasaklı Karakter girildi veya boş bırakıldı",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (!dosyaAdi.Contains("."))
                {
                    donenSonuc = new DonenSonuc { Mesaj = "Dosyanızın uzantısını giriniz" };
                    return donenSonuc;
                }

                File.Create(klasorYolu + dosyaAdi);

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Oluşturuldu",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı"
                };
                logKayit(e, "DosyaOlustur metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpDelete]
        public DonenSonuc DosyaSil(Guid ticketID, string klasorYolu, string dosyaAdi)
        {
            FileInfo silinenDosya = null;
            DonenSonuc donenSonuc;


            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(dosyaAdi))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Boş bırakılmış Değer Veya Değerler Var",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                scriptEncodeInput(ticketID);
                scriptEncodeInput(dosyaAdi);
                scriptEncodeInput(klasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasor Bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (dosyaAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(dosyaAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör adına Yasaklı Karakter girildi veya boş bırakıldı",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (!dosyaAdi.Contains("."))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Dosyanızın Uzantısını Giriniz",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                File.Delete(klasorYolu + dosyaAdi);
                FileInfo dosyaBilgi = new FileInfo(klasorYolu + dosyaAdi);
                silinenDosya = dosyaBilgi;
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Başarıyla Silindi",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı",
                };
                logKayit(e, "DosyaSil metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPut]
        public DonenSonuc DosyaGuncelle(Guid ticketID, string klasorYolu, string dosyaAdi, string yeniDosyaAdi)
        {
            FileInfo guncellenenDosya = null;
            DonenSonuc donenSonuc = new DonenSonuc();

            try
            {
                if (ticketID != ticketGlobal.ID)
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(dosyaAdi) || string.IsNullOrEmpty(yeniDosyaAdi))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                scriptEncodeInput(ticketID);
                scriptEncodeInput(dosyaAdi);
                scriptEncodeInput(yeniDosyaAdi);
                scriptEncodeInput(klasorYolu);
                klasorYolu = rootPath + klasorYolu.Trim();

                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (dosyaAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(dosyaAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Klasör adına Yasaklı Karakter girildi veya boş bırakıldı",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (!dosyaAdi.Contains("."))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Dosyanızın uzantısını giriniz",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                string eskidosya = klasorYolu + dosyaAdi;
                File.Move(eskidosya, klasorYolu + yeniDosyaAdi);
                FileInfo dosyaBilgi = new FileInfo(klasorYolu + yeniDosyaAdi);
                guncellenenDosya = dosyaBilgi;

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Başarıyla Güncellendi",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı"
                };
                logKayit(e, "DosyaGuncelle metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPut]
        public DonenSonuc DosyaTasi(Guid ticketID, string klasorYolu, string dosyaAdi, string yeniDosyaYolu)
        {
            DonenSonuc donenSonuc;


            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorYolu) || string.IsNullOrEmpty(dosyaAdi) || string.IsNullOrEmpty(yeniDosyaYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (!Directory.Exists(klasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "klasor bulunmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                scriptEncodeInput(ticketID);
                scriptEncodeInput(dosyaAdi);
                scriptEncodeInput(klasorYolu);
                scriptEncodeInput(yeniDosyaYolu);
                klasorYolu = rootPath + klasorYolu.Trim();



                if (!klasorYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }
                if (!yeniDosyaYolu.EndsWith("/"))
                {
                    klasorYolu += "\\";
                }

                if (dosyaAdi.Any(item => yasakliKarakterler.Contains(item) || string.IsNullOrEmpty(dosyaAdi)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Yasakli Karakter Girilmiş",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (!dosyaAdi.Contains("."))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Dosya Uzantısını Giriniz",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                string tasinacakDosya = klasorYolu + dosyaAdi.Trim();
                File.Move(tasinacakDosya, yeniDosyaYolu + dosyaAdi);
                FileInfo dosyaBilgi = new FileInfo(yeniDosyaYolu + dosyaAdi);
                tasinacakDosya = dosyaBilgi.Name;
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Başarıyla Taşındı",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı"
                };
                logKayit(e, "DosyaTasi metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        #endregion

        #region Dosya Yükleme
        [HttpPost]
        public DonenSonuc DosyaMetaDataKaydiOlustur()
        {
            DonenSonuc donenSonuc;
            try
            {

                string dosyaId = Guid.NewGuid().ToString();
                string tempKlasorYolu = Path.Combine(rootPath, "temp", dosyaId);

                Directory.CreateDirectory(tempKlasorYolu);

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Temp Klasörüne oluşturuldu",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı "
                };
                logKayit(e, "DosyaMetaDataKaydiOlustur metodunda bir sorun oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }

        [HttpPost]
        public DonenSonuc DosyaParcalariYukle(Guid ticketID, Guid klasorID, int parcaNumarasi)
        {
            DonenSonuc donenSonuc;
            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(klasorID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var"
                    };

                    return donenSonuc;
                }
                string tempKlasorYolu = Path.Combine(rootPath, "temp", klasorID.ToString());

                if (!Directory.Exists(tempKlasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Temp Klasorü Yolu Bulunamadı"
                    };
                    return donenSonuc;
                }

                if (string.IsNullOrEmpty(klasorID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var"
                    };

                    return donenSonuc;
                }

                Stream stream = System.Web.HttpContext.Current.Request.GetBufferedInputStream();

                if (stream == null || stream.Length == 0)
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Yüklenen dosya bulunamadı veya boş"
                    };

                    return donenSonuc;
                }
                if (stream.Length > mb1)
                {

                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Dosya Boyutu 1 mb dan büyük"
                    };

                    return donenSonuc;
                }

                string klasorYolu = Path.Combine(rootPath, "temp", klasorID.ToString());

                byte[] buffer = new byte[mb1];


                using (StreamReader readStream = new StreamReader(stream))
                {

                    using (FileStream cikti = new FileStream(Path.Combine(klasorYolu, parcaNumarasi.ToString()), FileMode.Create))
                    {
                        int bytesRead;
                        while ((bytesRead = readStream.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cikti.Write(buffer, 0, bytesRead);
                        }
                    }

                }
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Parçalama İşlemi Başarılı",

                    Sonuc = true
                };

            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı "
                };
                logKayit(e, "DosyaParcalariYukle metodunda hata oluştu");
                return donenSonuc;
            }

            return donenSonuc;
        }

        [HttpPost]
        public DonenSonuc DosyaYayinla(Guid ticketID, Guid klasorID, [FromBody] DosyaYayinlaBilgi dosyaBilgi)
        {


            DonenSonuc donenSonuc;
            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }

                if (string.IsNullOrEmpty(klasorID.ToString()) || string.IsNullOrEmpty(dosyaBilgi.DosyaAdi) || string.IsNullOrEmpty(dosyaBilgi.KlasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Boş Bırakılmış Değer Veya Değerler Var"
                    };

                    return donenSonuc;
                }

                scriptEncodeInput(dosyaBilgi.DosyaAdi);
                scriptEncodeInput(dosyaBilgi.KlasorYolu);
                string dosyaAdi = dosyaBilgi.DosyaAdi;
                string klasorYolu = dosyaBilgi.KlasorYolu;
                string hedefYol = Path.Combine(rootPath, klasorYolu);
                string tempKlasorYolu = Path.Combine(rootPath, "temp", klasorID.ToString());

                if (!Directory.Exists(Path.Combine(tempKlasorYolu)))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Temp klasöründe belirttiğiniz id bulunamadı",
                    };

                    return donenSonuc;
                }
                if (!Directory.Exists(hedefYol))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Hedef Klasor Yolu Bulunamadı"
                    };

                    return donenSonuc;
                }
                hedefYol = Path.Combine(rootPath + klasorYolu, dosyaAdi);
                string[] dosyaParcalari = Directory.GetFiles(tempKlasorYolu);

                using (var olusturulanKlasor = File.Create(hedefYol))
                {
                    foreach (var parca in dosyaParcalari)
                    {
                        using (var parcaGetir = File.OpenRead(parca))
                        {
                            parcaGetir.CopyTo(olusturulanKlasor);
                        }

                    }
                }

                Directory.Delete(tempKlasorYolu, true);

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "İşlem Başarılı",
                    Sonuc = true
                };
            }
            catch (Exception e)
            {
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod Çalıştırılırken Bir Sorunla Karşılaşıldı "
                };
                logKayit(e, "Dosya Yayınlada problemle karşılaşıldı");
                return donenSonuc;

            }
            return donenSonuc;
        }

        [HttpPost]
        public DonenSonuc DosyaDirektYukle(Guid ticketID, [FromBody] string DosyaAdi, string KlasorYolu)
        {
            DonenSonuc donenSonuc;
            try
            {
                if (ticketID != ticketGlobal.ID || string.IsNullOrEmpty(ticketID.ToString()))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Mesaj = "Ticketlar uyuşmuyor",
                        Sonuc = false
                    };
                    return donenSonuc;
                }
                if (string.IsNullOrEmpty(DosyaAdi) || string.IsNullOrEmpty(KlasorYolu))
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Boş Bırakılmış Değer veya Değerler Var",
                    };
                    return donenSonuc;
                }


                Stream stream = System.Web.HttpContext.Current.Request.InputStream;
                if (stream == null || stream.Length == 0)
                {
                    donenSonuc = new DonenSonuc
                    {
                        Sonuc = false,
                        Mesaj = "Yüklenen dosya bulunamadı veya boş"
                    };

                    return donenSonuc;
                }
                scriptEncodeInput(DosyaAdi);
                scriptEncodeInput(KlasorYolu);

                string hedefYol = Path.Combine(rootPath, KlasorYolu, DosyaAdi);
                using (FileStream olusturulacakDosya = File.Create(hedefYol))
                {
                    stream.CopyTo(olusturulacakDosya);
                }
                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Dosya Başarıyla Yüklendi",
                    Sonuc = true,
                };

            }
            catch (Exception e)
            {

                donenSonuc = new DonenSonuc
                {
                    Mesaj = "Metod çalıştırılırken bir sorunla karşılaşıldı",
                    Sonuc = false
                };
                logKayit(e, "DosyaDirektYukle metodunda hata oluştu");
                return donenSonuc;
            }
            return donenSonuc;
        }
        #endregion


    }
}

