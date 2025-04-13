using Proje.Models;
using Proje.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace Proje.Controllers
{
    public class AdminController : Controller
    {
        KUTUPHANEEntities1 a = new KUTUPHANEEntities1();

        [HttpGet]
        [MyAuthorization(Roles = "A")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [MyAuthorization(Roles = "A")]
        public ActionResult Satici()
        {
            return View(a.Publishers.ToList());
        }

        [HttpPost]
        [MyAuthorization(Roles = "A")]
        public string Sil(int publisherId)
        {

            try
            {
                var bookPublishers = a.BookPublishers.Where(bp => bp.pubID == publisherId).ToList();



                foreach (var bookPublisher in bookPublishers)
                {
                    a.BookPublishers.Remove(bookPublisher);
                }
                a.SaveChanges();


                var publisher = a.Publishers.FirstOrDefault(p => p.pubID == publisherId);


                if (publisher != null)
                {
                    
                    var userId = publisher.userID;

                  
                    a.Publishers.Remove(publisher);
                    a.SaveChanges();

                
                    if (userId.HasValue)
                    {
                        var user = a.Users.FirstOrDefault(u => u.userID == userId);


                        if (user != null)
                        {
                            a.Users.Remove(user);
                            a.SaveChanges();
                        }
                    }

                    return "basarili";
                }
                else
                {
                    return "Satıcı bulunamadı.";
                }
            }


            catch (Exception ex)
            {
                // Hata durumunda, hata mesajını döndür
                return $"hata: {ex.Message}";
            }

        }
        [HttpGet]
        [MyAuthorization(Roles = "A")]
        public ActionResult Analiz()
        {
            var purchases = (from p in a.Purchase
                             join bp in a.BookPublishers on p.bookID equals bp.bookID
                             join pub in a.Publishers on bp.pubID equals pub.pubID
                             join m in a.Members on p.memberID equals m.memberID
                             where  p.pName == pub.pName
                             select new kitaplar
                             {
                                 BookID = p.bookID,
                                 Title = bp.title,
                                 PublisherName = p.pName,
                                 Price = p.unitPrice,
                                 
                                 PubID = pub.pubID,
                                 AuthorName = bp.authorName

                             }).ToList();

            return View(purchases);


        }
       [HttpGet]
        [AllowAnonymous]
       public ActionResult KitapEkle()
        {

            return View();
        }

        [HttpPost]
        [MyAuthorization(Roles = "A")]
        public ActionResult KitapEkle(Analizkitap ktp, HttpPostedFileBase CoverImage)
        {
            if (CoverImage != null && CoverImage.ContentLength > 0)
            {
                var fileName = Path.GetFileName(CoverImage.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                try
                {
                    // Dosyayı belirtilen yola kaydet
                    CoverImage.SaveAs(path);

                    // Dosya URL'sini modelde sakla
                    ktp.CoverURL = Url.Content(Path.Combine("~/Uploads", fileName));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "Yüklenen dosya geçerli değil.");
            }
            var analizKitap = new Analizkitap
            {
              
               
                AuthorName = ktp.AuthorName,
                PublisherName = ktp.PublisherName,
                Price = ktp.Price,
                Title= ktp.Title,
                
                CoverURL = ktp.CoverURL
            };

            // Session'dan mevcut kitapları al
            var analizList = Session["Purchases"] as List<Analizkitap> ?? new List<Analizkitap>();

            // Listeye yeni kitabı ekle
            analizList.Add(analizKitap);

            // Güncellenmiş listeyi Session'a kaydet
            Session["Purchases"] = analizList;

            // İsteğe bağlı olarak başarılı yanıt dönebilirsiniz
            return RedirectToAction("PartialView3", "Home");
            
        }







    }









}
