using Proje.Models;
using Proje.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;



namespace Proje.Controllers
{
    public class SatıcıController : Controller
    {
        KUTUPHANEEntities1 p = new KUTUPHANEEntities1();

        [MyAuthorization(Roles = "P")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [MyAuthorization(Roles ="P")]
        public ActionResult Listele()
        {
            string username = User.Identity.Name;

          
            var user = p.Users
                        .Where(u => u.userName == username)
                        .Select(u => new { u.rolID, u.userID })
                        .FirstOrDefault();

            if (user != null)
            {
                var publisher = p.Publishers
                          .FirstOrDefault(b => b.userID == user.userID);

                if (publisher == null)
                {
                    return HttpNotFound("Yayınevi bulunamadı.");
                }

              
                var allbooks = p.BookPublishers
                                .Where(bp => bp.pubID == publisher.pubID)
                                .Join(p.Books,
                                      bp => bp.bookID,
                                      b => b.bookID,
                                      (bp, b) => new { b, bp })
                                .ToList();

             
                var kitapIDleri = allbooks.Select(x => x.b.bookID).ToList();
                var genreList = p.BookGenre
                                  .Where(bg => kitapIDleri.Contains((int)bg.bookID))
                                  .Select(bg => new { bg.bookID, bg.genreID })
                                  .ToList();
                var genres = p.Genre.ToList();

                
                var kitapBilgileri = allbooks.Select(x => new kitaplar
                {
                    BookID = x.b.bookID,
                    Title = x.b.title,
                    CoverURL = x.bp.coverURL, 
                    Price = x.bp.price,    
                    Stock = x.bp.stock,    
                    PageNum = x.bp.pageNum,
                    PublisherName = publisher.pName,
                    PubID = publisher.pubID,
                    AuthorName = x.bp.authorName,    
                    Genres = genreList
                                .Where(g => g.bookID == x.b.bookID)
                                .Select(g => genres.FirstOrDefault(gen => gen.genreID == g.genreID)?.gName)
                                .ToList()
                }).ToList();

                return View(kitapBilgileri);
            }
                return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        [MyAuthorization(Roles = "P")]
        public ActionResult Ekle()
        {

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Giris"); 
            }

            var user = p.Users.SingleOrDefault(u => u.userName == userName);
            if (user == null)
            {
                return HttpNotFound();
            }

         
            var publisher = p.Publishers
                                .Where(a => a.userID == user.userID) 
                                .Select(a => new yayinci
                                {
                                    Yayınevi = a.pName

                                }).FirstOrDefault();
         
            return View(publisher);
        }

        [HttpPost]
        [MyAuthorization(Roles = "P")]
        public ActionResult Ekle(kitaplar m, HttpPostedFileBase CoverImage)
        {
            if (ModelState.IsValid)
            {
               
                var newBook = new Books
                {
                    title = m.Title
                };
                p.Books.Add(newBook);
                p.SaveChanges(); 

            
                var author = p.Authors.FirstOrDefault(a => a.authorName == m.AuthorName);
                if (author == null)
                {
                    author = new Authors { authorName = m.AuthorName };
                    p.Authors.Add(author);
                    p.SaveChanges(); 
                }

           
                var publisher = p.Publishers.FirstOrDefault(a => a.pName == m.PublisherName);
                if (publisher == null)
                {
                    publisher = new Publishers { pName = m.PublisherName };
                    p.Publishers.Add(publisher);
                    p.SaveChanges(); 
                }

            
                var bookId = newBook.bookID;

           
                foreach (var genreName in m.Genres)
                {
                    var genre = p.Genre.FirstOrDefault(g => g.gName == genreName);
                    if (genre == null)
                    {
                        genre = new Genre { gName = genreName };
                        p.Genre.Add(genre);
                        p.SaveChanges(); 
                    }

                
                    p.BookGenre.Add(new BookGenre { bookID = bookId, genreID = genre.genreID });
                }

            
                p.BookAuthors.Add(new BookAuthors { bookID = bookId, authorID = author.authorID });
                
                if (CoverImage != null && CoverImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(CoverImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                    try
                    {
                       
                        CoverImage.SaveAs(path);

                      
                        m.CoverURL = Url.Content(Path.Combine("~/Uploads", fileName));
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
                p.BookPublishers.Add(new BookPublishers
                {
                    bookID = bookId,
                    pubID = publisher.pubID,
                    price = m.Price,
                    coverURL = m.CoverURL,
                    stock = m.Stock,
                    authorName = m.AuthorName,
                    pageNum = m.PageNum,
                    title= m.Title
                });
                p.SaveChanges(); 

                return RedirectToAction("Index");
            }

         
            return View(m);

        }

        [HttpGet]
        [MyAuthorization(Roles = "P")]
        public ActionResult guncelle(int id)
        {
            var book = p.Books
                .Where(b => b.bookID == id)
                .Select(b => new kitaplar
                {
                    BookID = b.bookID,
                    Title = b.title,
                    CoverURL = p.BookPublishers
                               .Where(bp => bp.bookID == b.bookID)
                               .Select(bp => bp.coverURL)
                               .FirstOrDefault(),
                    Price = p.BookPublishers
                             .Where(bp => bp.bookID == b.bookID)
                             .Select(bp => bp.price)
                             .FirstOrDefault(),
                    Stock = p.BookPublishers
                             .Where(bp => bp.bookID == b.bookID)
                             .Select(bp => bp.stock)
                             .FirstOrDefault(),
                    PageNum = p.BookPublishers
                               .Where(bp => bp.bookID == b.bookID)
                               .Select(bp => bp.pageNum)
                               .FirstOrDefault(),
                    PubID = (int)p.BookPublishers
                               .Where(bp => bp.bookID == b.bookID)
                               .Select(bp => bp.pubID)
                               .FirstOrDefault(),
                    PublisherName = p.BookPublishers
                                     .Where(bp => bp.bookID == b.bookID)
                                     .Select(bp => p.Publishers
                                         .Where(pub => pub.pubID == bp.pubID)
                                         .Select(pub => pub.pName)
                                         .FirstOrDefault())
                                     .FirstOrDefault(),
                    AuthorName = p.BookAuthors
                                 .Where(ba => ba.bookID == b.bookID)
                                 .Select(ba => p.Authors
                                     .Where(author => author.authorID == ba.authorID)
                                     .Select(author => author.authorName)
                                     .FirstOrDefault())
                                 .FirstOrDefault(),
                    Genres = p.BookGenre
                               .Where(bg => bg.bookID == b.bookID)
                               .Select(bg => p.Genre
                                   .Where(gen => gen.genreID == bg.genreID)
                                   .Select(gen => gen.gName)
                                   .FirstOrDefault())
                               .ToList()
                }).FirstOrDefault();

            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }



        [HttpPost]
        [MyAuthorization(Roles = "P")]

        public ActionResult guncelle(kitaplar model)
        {
            if (ModelState.IsValid)
            {
            
                var book = p.Books
                            .Join(p.BookPublishers,
                                  b => b.bookID,
                                  bp => bp.bookID,
                                  (b, bp) => new { Book = b, BookPublisher = bp })
                            .Join(p.Publishers,
                                  bbp => bbp.BookPublisher.pubID,
                                  p => p.pubID,
                                  (bbp, p) => new { bbp.Book, PublisherName = p.pName })
                            .Where(bbp => bbp.PublisherName == model.PublisherName && bbp.Book.bookID == model.BookID)
                            .Select(bbp => bbp.Book)
                            .FirstOrDefault();

                if (book == null)
                {
                    return HttpNotFound();
                }

                // Kitap bilgilerini güncelle
                book.title = string.IsNullOrEmpty(model.Title) ? book.title : model.Title;
                book.price = model.Price.HasValue ? model.Price.Value : book.price;
                book.stock = model.Stock.HasValue ? model.Stock.Value : book.stock;
                book.pageNum = model.PageNum.HasValue ? model.PageNum.Value : book.pageNum;
                book.coverURL = string.IsNullOrEmpty(model.CoverURL) ? book.coverURL : model.CoverURL;

                // BookPublishers tablosunu güncelle
                var bookPublisher = p.BookPublishers
                                     .FirstOrDefault(bp => bp.bookID == model.BookID &&
                                                            p.Publishers
                                                             .Any(pub => pub.pName == model.PublisherName &&
                                                                         pub.pubID == bp.pubID));
                if (bookPublisher != null)
                {
                    // Yayınevi bilgilerini güncelle
                    bookPublisher.coverURL = string.IsNullOrEmpty(model.CoverURL) ? bookPublisher.coverURL : model.CoverURL;
                    bookPublisher.price = model.Price.HasValue ? model.Price.Value : bookPublisher.price;
                    bookPublisher.stock = model.Stock.HasValue ? model.Stock.Value : bookPublisher.stock;
                    bookPublisher.pageNum = model.PageNum.HasValue ? model.PageNum.Value : bookPublisher.pageNum;
                    bookPublisher.title = string.IsNullOrEmpty(model.Title) ? book.title : model.Title;
                }

                
                p.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);


        }

        [HttpPost]
        [MyAuthorization(Roles = "P")]
        public string Sil(int bookId, int publisherId)
        {
            try
            {
            
                var bookPublisher = p.BookPublishers
                    .FirstOrDefault(bp => bp.bookID == bookId && bp.pubID == publisherId);

                if (bookPublisher != null)
                {
                 
                    bookPublisher.stock = 0;

                  
                    p.SaveChanges();

                    return "basarili";
                }
                else
                {
                    return "hata: Kitap ve yayınevi bulunamadı";
                }
            }
            catch (Exception ex)
            {
                
                return $"hata: {ex.Message}";
            }

        }


























    }
}