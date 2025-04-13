using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using Proje.Models;
using Proje.Security;

namespace Proje.Controllers
{
    public class HomeController : Controller
    {
        KUTUPHANEEntities1 y = new KUTUPHANEEntities1() ;
       

        [AllowAnonymous]
        public ActionResult Index()
        {


            return View();
        }

        public ActionResult deneme()
        {


            return View();
        }
        [AllowAnonymous]
        public ActionResult PartialView1()
        {
            List<Authors> x = y.Authors.ToList();
            List<Publishers> a = y.Publishers.ToList();
            List<Genre> g = y.Genre.ToList();
           
            ViewBag.yayınevi = a;
            ViewBag.tür = g;

            return PartialView("_PartialView1", x);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Liste(int id)
        {
            var books = (from b in y.Books
                         join bp in y.BookPublishers on b.bookID equals bp.bookID
                         join p in y.Publishers on bp.pubID equals p.pubID
                         where y.BookAuthors.Any(ba => ba.bookID == b.bookID && ba.authorID == id)
                         select new kitaplar
                         {
                             BookID = b.bookID,
                             Title = b.title,
                             PubID = p.pubID,
                             CoverURL = bp.coverURL, 
                             Price = bp.price,      
                             PageNum = bp.pageNum,   
                             PublisherName = p.pName,
                             Stock= bp.stock,
                             Genres = y.BookGenre
                                        .Where(bg => bg.bookID == b.bookID)
                                        .Join(y.Genre,
                                              bg => bg.genreID,
                                              g => g.genreID,
                                              (bg, g) => g.gName)
                                        .ToList()
                         }).ToList();

        
            return View(books);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult KitapYayın(int id1)
        {
            var books = (from b in y.Books
                         join bp in y.BookPublishers on b.bookID equals bp.bookID
                         where bp.pubID == id1
                         select new kitaplar
                         {
                             BookID = b.bookID,
                             Title = b.title,
                             Stock= bp.stock,
                             CoverURL = bp.coverURL,
                             PubID = id1,
                             Price = bp.price, 
                             PageNum = bp.pageNum, 
                             PublisherName = y.Publishers
                                            .Where(p => p.pubID == id1)
                                            .Select(p => p.pName)
                                            .FirstOrDefault(), 
                             Genres = (from bg in y.BookGenre
                                       join g in y.Genre on bg.genreID equals g.genreID
                                       where bg.bookID == b.bookID
                                       select g.gName).ToList()
                         }).ToList();

         
            return View("Liste", books);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult KitapFiyat(string range)
        {
            var rangeParts = range.Split('-');
            if (rangeParts.Length == 2 &&
                decimal.TryParse(rangeParts[0], out var minPrice) &&
                decimal.TryParse(rangeParts[1], out var maxPrice))
            {
               
                var kitaplarIDleri = y.BookPublishers
                    .Where(bp => bp.price >= minPrice && bp.price <= maxPrice)
                    .Select(bp => bp.bookID)
                    .Distinct()
                    .ToList();

              
                var books = y.Books
                    .Where(b => kitaplarIDleri.Contains(b.bookID))
                    .ToList();

             
                var bookPublishers = y.BookPublishers
                    .Where(bp => kitaplarIDleri.Contains(bp.bookID))
                    .ToList();

            
                var publisherNames = y.Publishers
                    .ToDictionary(p => p.pubID, p => p.pName);

             
                var kitaplarBilgi = books.Select(book => new kitaplar
                {
                    BookID = book.bookID,
                    Title = book.title,
                    CoverURL = bookPublishers
                                .Where(bp => bp.bookID == book.bookID)
                                .Select(bp => bp.coverURL)
                                .FirstOrDefault(),
                    Price = bookPublishers
                            .Where(bp => bp.bookID == book.bookID)
                            .Select(bp => bp.price)
                            .FirstOrDefault(),
                    PageNum = bookPublishers
                              .Where(bp => bp.bookID == book.bookID)
                              .Select(bp => bp.pageNum)
                              .FirstOrDefault(),
                    Stock= bookPublishers
                              .Where(bp => bp.bookID == book.bookID)
                              .Select(bp => bp.stock)
                              .FirstOrDefault(),
                    PubID = (int)bookPublishers
                              .Where(bp => bp.bookID == book.bookID)
                              .Select(bp => bp.pubID)
                              .FirstOrDefault(),
                    Genres = y.BookGenre
                                .Where(bg => bg.bookID == book.bookID)
                                .Join(y.Genre,
                                      bg => bg.genreID,
                                      g => g.genreID,
                                      (bg, g) => g.gName)
                                .ToList(),
                    PublisherName = bookPublishers
                                    .Where(bp => bp.bookID == book.bookID)
                                    .Select(bp => publisherNames
                                        .FirstOrDefault(pn => pn.Key == bp.pubID).Value)
                                    .FirstOrDefault()
                }).ToList();

                return View("Liste", kitaplarBilgi);
            }

            return RedirectToAction("Index", "Home");
        }





        [HttpPost]
        [AllowAnonymous]
        public ActionResult KitapSayfa(string range)
        {
            var rangeParts = range.Split('-');
            if (rangeParts.Length == 2 &&
                decimal.TryParse(rangeParts[0], out var minPage) &&
                decimal.TryParse(rangeParts[1], out var maxPage))
            {
              
                var kitaplarIDleri = y.BookPublishers
                    .Where(bp => bp.pageNum >= minPage && bp.pageNum <= maxPage)
                    .Select(bp => bp.bookID)
                    .Distinct()
                    .ToList();

              
                var allbooks = y.Books
                    .Where(b => kitaplarIDleri.Contains(b.bookID))
                    .ToList();

              
                var bookPublishers = y.BookPublishers
                    .Where(bp => kitaplarIDleri.Contains(bp.bookID))
                    .ToList();

            
                var genreList = y.BookGenre
                    .Where(bg => kitaplarIDleri.Contains(bg.bookID))
                    .Join(y.Genre,
                          bg => bg.genreID,
                          g => g.genreID,
                          (bg, g) => new { bg.bookID, g.gName })
                    .ToList();

             
                var publisherNames = y.Publishers
                    .ToDictionary(p => p.pubID, p => p.pName);

                
                var kitaplarBilgi = allbooks.Select(book => new kitaplar
                {
                    BookID = book.bookID,
                    Title = book.title,
                    CoverURL = bookPublishers
                                .Where(bp => bp.bookID == book.bookID)
                                .Select(bp => bp.coverURL)
                                .FirstOrDefault(),
                    Price = bookPublishers
                            .Where(bp => bp.bookID == book.bookID)
                            .Select(bp => bp.price)
                            .FirstOrDefault(),
                    Stock = bookPublishers
                              .Where(bp => bp.bookID == book.bookID)
                              .Select(bp => bp.stock)
                              .FirstOrDefault(),
                    PubID = (int)bookPublishers
                            .Where(bp => bp.bookID == book.bookID)
                            .Select(bp => bp.pubID)
                            .FirstOrDefault(),
                    PageNum = bookPublishers
                              .Where(bp => bp.bookID == book.bookID)
                              .Select(bp => bp.pageNum)
                              .FirstOrDefault(),
                    Genres = genreList
                                .Where(g => g.bookID == book.bookID)
                                .Select(g => g.gName)
                                .ToList(),
                    PublisherName = bookPublishers
                            .Where(bp => bp.bookID == book.bookID)
                            .Select(bp => publisherNames
                                .FirstOrDefault(pn => pn.Key == bp.pubID).Value)
                            .FirstOrDefault()
                }).ToList();

                return View("Liste", kitaplarBilgi);
            }

            // Sayfa aralığı geçerli değilse anasayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }





        [HttpPost]
        [AllowAnonymous]
        public ActionResult KitapTür(int? id, string bookName)
        {
            var booksQuery = y.Books.AsQueryable();

            if (!string.IsNullOrEmpty(bookName))
            {
                booksQuery = booksQuery.Where(b => b.title.Contains(bookName));
            }

            // Tür ID'ye göre filtreleme
            if (id.HasValue)
            {
                var bookIdsByGenre = y.BookGenre
                    .Where(bg => bg.genreID == id.Value)
                    .Select(bg => bg.bookID)
                    .ToList();

                booksQuery = booksQuery.Where(b => bookIdsByGenre.Contains(b.bookID));
            }

         
            var bookList = booksQuery.ToList();

         
            var bookIds = bookList.Select(b => b.bookID).ToList();
            var bookPublishers = y.BookPublishers
                .Where(bp => bookIds.Contains((int)bp.bookID))
                .ToList();

         
            var publishers = y.Publishers.ToDictionary(p => p.pubID, p => p.pName);

           
            var bookViewModels = bookList.Select(book =>
            {
             
                var bookPublisher = bookPublishers.FirstOrDefault(bp => bp.bookID == book.bookID);
                var publisherName = bookPublisher != null && publishers.ContainsKey((int)bookPublisher.pubID)
                    ? publishers[(int)bookPublisher.pubID]
                    : "Bilinmiyor";

           
                var genres = y.BookGenre
                    .Where(bg => bg.bookID == book.bookID)
                    .Join(y.Genre,
                          bg => bg.genreID,
                          g => g.genreID,
                          (bg, g) => g.gName)
                    .ToList();

                return new kitaplar
                {
                    BookID = book.bookID,
                    Title = book.title,
                    Stock=bookPublisher?.stock??0,
                    CoverURL = bookPublisher?.coverURL, // Cover URL if present in BookPublishers
                    Price = bookPublisher?.price ?? 0, // Price from BookPublishers
                    PageNum = bookPublisher?.pageNum ?? 0,
                    PubID = (int)bookPublisher.pubID,
                    PublisherName = publisherName,
                    Genres = genres
                };
            }).ToList();

            return View("Liste", bookViewModels);

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Uyelik()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Uyelik(Members m)
        {
            var user = new User
            {
                userName = m.userName,
                password = m.password,
                rolID = 3
            };

            y.Users.Add(user);
            y.SaveChanges();
            var userId = user.userID;
            m.userID = userId;
            y.Members.Add(m);
            y.SaveChanges();

            return RedirectToAction("Giris");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Giris()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Giris(User u)
        {
            User k = y.Users.FirstOrDefault(x => x.userName == u.userName && x.password == u.password);
            if (k != null)
            {
                FormsAuthentication.SetAuthCookie(u.userName, false);
                return RedirectToAction("deneme");


            }
            else
            {
                ViewBag.hata = "E-posta veya şifre hatalı";
                return View();
            }

        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }


        






        [HttpPost]
        [MyAuthorization(Roles = "C")]
        public ActionResult AddToCart(int bookId, int pubId)
        {


            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Kullanıcı girişi yapılmamış." });
            }

         
            var user = y.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
            if (member == null)
            {
                return Json(new { success = false, message = "Üye bilgileri bulunamadı." });
            }

            var bookPublisher = y.BookPublishers
                                  .FirstOrDefault(bp => bp.bookID == bookId && bp.pubID == pubId);
            var publisher = y.Publishers.FirstOrDefault(p => p.pubID == pubId);

            if (bookPublisher != null)
            {
                var unitPrice = bookPublisher.price ?? 0;
                var totalPrice = unitPrice;

                var cartItem = new CartItems
                {
                    memberID = member.memberID,
                    bookID = bookId,
                    quantity = 1,
                    pName = publisher.pName,
                    unitPrice = unitPrice,
                    totalPrice = totalPrice
                };

                y.CartItems.Add(cartItem);
                y.SaveChanges();


                return Json(new { success = true, message = "Ürün sepete eklendi.", bookId = bookId, pubId = pubId });
            }
            else
            {
                return Json(new { success = false, message = "Kitap veya yayınevi bulunamadı." });
            }
        }
        [HttpGet]
        [MyAuthorization(Roles = "C")]
        public ActionResult Sepet(int? bookId, int? pubId)
        {

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Giris"); // Giriş yapılmamışsa giriş sayfasına yönlendir
            }

            var user = y.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return HttpNotFound();
            }

            var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
            if (member == null)
            {
                return HttpNotFound();
            }

            var cartItems = (from ci in y.CartItems
                             join bp in y.BookPublishers on ci.bookID equals bp.bookID
                             join p in y.Publishers on bp.pubID equals p.pubID
                             where ci.memberID == member.memberID && ci.pName == p.pName

                             
                             select new sepetim

                             {
                                 BookID = ci.bookID,
                                 Title = bp.title,
                                 UnitPrice = ci.unitPrice,
                                 NewPrice= bp.price, 
                                 Quantity = ci.quantity,
                                 TotalPrice = bp.price,
                                 PublisherName = p.pName,
                                 AuthorName = bp.authorName,
                                 CoverURL = bp.coverURL,
                                 PubID = p.pubID,
                                 Stock = bp.stock,
                                 PageNum = bp.pageNum
                             }).ToList();


            ViewBag.TotalPrice = cartItems.Sum(ci => ci.TotalPrice);

            return View(cartItems);

        }
        
        
        [HttpPost]
        [MyAuthorization(Roles = "C")]
        public string purchase(int bookId, int publisherId)
        {

            try
            {
                var userName = User.Identity.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return "hata";
                }

                // Kullanıcıyı `Users` tablosundan al
                var user = y.Users.SingleOrDefault(u => u.userName == userName);
                if (user == null)
                {
                    return "hata";
                }

                var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
                if (member == null)
                {
                    return "hata";
                }
                var bookPublisher = y.BookPublishers.FirstOrDefault(bp => bp.bookID == bookId && bp.pubID == publisherId);

                var publisher = y.Publishers.FirstOrDefault(p => p.pubID == publisherId);

                if (bookPublisher != null)
                {
                    if (bookPublisher.stock > 0)
                    {

                        var unitPrice = bookPublisher.price ?? 0;
                        var totalPrice = unitPrice;

                        var cartItem = new Purchase
                        {
                            memberID = member.memberID,
                            bookID = bookId,
                            quantity = 1,
                            pName = publisher.pName,
                            unitPrice = unitPrice,
                            totalPrice = totalPrice
                        };

                        y.Purchase.Add(cartItem);
                        y.SaveChanges();


                        bookPublisher.stock -= 1;
                        y.SaveChanges();

                        var cartItems = y.CartItems.Where(ci => ci.memberID == member.memberID && ci.bookID == bookId && cartItem.pName == publisher.pName).ToList();


                        foreach (var cart in cartItems)
                        {
                            y.CartItems.Remove(cart);
                        }
                        y.SaveChanges();


                        return "basarili";
                    
                     }
                    else
                    {
                        return "Ürün tükendi!";
                    }
                }

                else
                {
                    return "hata: Kitap veya yayınevi bulunamadı.";
                }
            }

            catch (Exception ex)
            {
            
                return $"hata: {ex.Message}";
            }




        }

        [HttpGet]
        [MyAuthorization(Roles = "C")]
        public ActionResult Siparis()
        {

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Giris"); 
            }

            var user = y.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return HttpNotFound();
            }

            var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
            if (member == null)
            {
                return HttpNotFound();
            }

            var cartItems = (from ci in y.Purchase
                             join bp in y.BookPublishers on ci.bookID equals bp.bookID
                             join p in y.Publishers on bp.pubID equals p.pubID
                             where ci.memberID == member.memberID && ci.pName == p.pName

                             select new sepetim

                             {
                                 BookID = ci.bookID,
                                 Title = bp.title,
                                 UnitPrice = ci.unitPrice,
                                 Quantity = ci.quantity,
                                 TotalPrice = ci.totalPrice,
                                 PublisherName = p.pName,
                                 AuthorName = bp.authorName,
                                 CoverURL = bp.coverURL,
                                 PubID = p.pubID,
                                 Stock = bp.stock,
                                 PageNum = bp.pageNum
                             }).ToList();


            ViewBag.TotalPrice = cartItems.Sum(ci => ci.TotalPrice);

            return View(cartItems);

        }

        [HttpGet]
        [AllowAnonymous]

        public ActionResult SaticiUye()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]

        public ActionResult SaticiUye(Publishers p)
        {
            var user = new User
            {
                userName = p.userName,
                password = p.password,
                rolID = 2
            };

            y.Users.Add(user);
            y.SaveChanges();
            var userId = user.userID;
            p.userID = userId;
            y.Publishers.Add(p);
            y.SaveChanges();
           

            return RedirectToAction("Giris");
            
        }

        [HttpGet]
        [MyAuthorization(Roles = "C")]
        public ActionResult İletisim()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Giris"); // Giriş yapılmamışsa giriş sayfasına yönlendir
            }

            var user = y.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return HttpNotFound(); 
            }

            var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
            if (member == null)
            {
                return HttpNotFound(); // Üye bulunamazsa 404 hatası döndür
            }

          
            var address = y.MemberAddress
                .Where(ma => ma.mID == member.memberID)
                .Select(ma => ma.aID) 
                .FirstOrDefault();
            var addressInfo = address != 0 ? y.address.SingleOrDefault(a => a.addressID == address) : null;

            var viewModel = new addmodel
            {
                AddressID = addressInfo?.addressID,
                City = addressInfo?.city,
                District = addressInfo?.district,
                Nbhood = addressInfo?.nbhood,
                Street = addressInfo?.street
            };
            return View(viewModel);


        }

        [HttpPost]
        [MyAuthorization(Roles = "C")]
        public ActionResult İletisim(addmodel model)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Giris"); // Giriş yapılmamışsa giriş sayfasına yönlendir
            }

            var user = y.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return HttpNotFound(); // Kullanıcı bulunamazsa 404 hatası döndür
            }

            var member = y.Members.SingleOrDefault(m => m.userID == user.userID);
            if (member == null)
            {
                return HttpNotFound(); // Üye bulunamazsa 404 hatası döndür
            }

            address address;
            if (model.AddressID.HasValue)
            {
                // Var olan adresi güncelle
                address = y.address.SingleOrDefault(a => a.addressID == model.AddressID);
                if (address == null)
                {
                    return HttpNotFound(); 
                }

                address.city = model.City;
                address.district = model.District;
                address.nbhood = model.Nbhood;
                address.street = model.Street;
            }
            else
            {
             
                address = new address
                {
                    city = model.City,
                    district = model.District,
                    nbhood = model.Nbhood,
                    street = model.Street
                };

                y.address.Add(address);
            }

            y.SaveChanges(); 

            var addressId = address.addressID;

       
            var memberAddress = y.MemberAddress.SingleOrDefault(ma => ma.mID == member.memberID);
            if (memberAddress == null)
            {
                memberAddress = new MemberAddress
                {
                    aID = addressId,
                    mID = member.memberID
                };

                y.MemberAddress.Add(memberAddress);
            }
            else
            {
                memberAddress.aID = addressId; 
            }

            y.SaveChanges(); 

            return RedirectToAction("Index");



        }
        [AllowAnonymous]
        public ActionResult PartialView3()
        {
            var analizList = Session["Purchases"] as List<Analizkitap>;
            return PartialView("_PartialView3", analizList);
        }












































    }

}  



