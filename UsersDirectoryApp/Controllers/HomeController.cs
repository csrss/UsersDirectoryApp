using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UsersDirectoryApp.Models;

namespace UsersDirectoryApp.Controllers
{
    public class PostRequestProcessorResult
    {
        public bool Result;
        public string Message;
    }
    public class PostRequestProcessorResultUsers : PostRequestProcessorResult
    {
        public IEnumerable<Person> Users { get; set; }
        public int TotalUsersCount { get; set; }
    }

    [HandleError]
    public class HomeController : Controller
    {
        /// <summary>
        /// Serwis, ktory bedzie zarzadzal userami
        /// </summary>
        private IDataProvider m_dataPrivider = null;

        public HomeController(IDataProvider dataPrivider)
        {
            m_dataPrivider = dataPrivider;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                if (!Path.GetExtension(file.FileName).ToLower().Equals(".csv"))
                {
                    throw new NotSupportedException("Upload of files, other then CSV, is not supported.");
                }

                List<Person> persons = PersonsFactory.Create(file.InputStream);
                persons.ForEach(x => this.m_dataPrivider.Add(x));
                this.m_dataPrivider.Save();
            }
            return View(this.m_dataPrivider.GetUsers());
        }

        /// <summary>
        /// Główna metoda do przetwarzania zapytań od klienta. Tu instalujemy jakigos dispatchera, ktory ma za zadanie rozkminiac
        /// co klient chce od nas. Nastepnie, ten nasz dispatcher przekazuje to co rozkminil dalej.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostRequestProcessor(string request)
        {
            try
            {
                // Nasz request przychodzi w formie JSon
                JObject jObject = JObject.Parse(request);

                Dictionary<string, Delegate> locator = new Dictionary<string, Delegate>
                {
                    {
                        "HttpPostRequest_DropDatabase",
                        new Func<HttpPostRequest_DropDatabase>(() => 
                        {
                            return jObject.ToObject<HttpPostRequest_DropDatabase>();
                        })
                    },
                    {
                        "HttpPostRequest_AddUser",
                        new Func<HttpPostRequest_AddUser>(() => 
                        {
                            return jObject.ToObject<HttpPostRequest_AddUser>();
                        })
                    },
                    {
                        "HttpPostRequest_DropUser",
                        new Func<HttpPostRequest_DropUser>(() => 
                        {
                            return jObject.ToObject<HttpPostRequest_DropUser>();
                        })
                    },
                    {
                        "HttpPostRequest_UpdateUser",
                        new Func<HttpPostRequest_UpdateUser>(() => 
                        {
                            return jObject.ToObject<HttpPostRequest_UpdateUser>();
                        })
                    },
                    {
                        "HttpPostRequest_Users",
                        new Func<HttpPostRequest_Users>(() => 
                        {
                            return jObject.ToObject<HttpPostRequest_Users>();
                        })
                    }
                };

                HttpPostRequest post = jObject.ToObject<HttpPostRequest>();

                PostRequestProcessorResult processorResult = null;
                if (!string.IsNullOrWhiteSpace(post.ID) && locator.ContainsKey(post.ID))
                {
                    processorResult = this.Process((locator[post.ID].DynamicInvoke() as HttpPostRequest) as dynamic);

                }

                // Tłumaczymy go nastepnie na odpowiedni obiekt
                return Json(processorResult);
            }
            catch (Exception ex)
            {
                PostRequestProcessorResult errorResult = new PostRequestProcessorResult { Result = false, Message = ex.Message };
                return Json(errorResult);
            }
        }

        /// <summary>
        /// Tu procesujemy poszczególne żądania od klienta. Wszystkie te metody Process można wrzucic do osobnego controllera oczywiscie.
        /// </summary>
        /// <param name="request"></param>
        private PostRequestProcessorResult Process(HttpPostRequest_DropDatabase request)
        {
            this.m_dataPrivider.Reset();
            return new PostRequestProcessorResult { Result = true, Message = "" };
        }

        private PostRequestProcessorResultUsers Process(HttpPostRequest_Users request)
        {
            IEnumerable<Person> users = m_dataPrivider.GetUsers();
            Dictionary<Type, Delegate> locator = new Dictionary<Type, Delegate>
            {
                {
                    typeof(string),
                    new Func<string, IEnumerable<Person>>(s =>
                    {
                        return users.Where(x => x.FirstName.ToLower().StartsWith(s.ToLower()) || 
                            x.LastName.ToLower().StartsWith(s.ToLower())).ToList();
                    })
                },
                {
                    typeof(DateTime),
                    new Func<DateTime, IEnumerable<Person>>(s =>
                    {
                        return users.Where(x => x.BirthdateDisplay == s || 
                            x.Birthdate == (s - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds);
                    })
                },
                {
                    typeof(double),
                    new Func<double, IEnumerable<Person>>(s =>
                    {
                        return users.Where(x => x.Height == s);
                    })
                },
                {
                    typeof(bool),
                    new Func<bool, IEnumerable<Person>>(s =>
                    {
                        return users.Where(x => x.Member == s);
                    })
                }
            };

            // Zakładam że filtrujemy po nazwach userów (name i lastname)
            List<Person> result = new List<Person>();
            string[] fltrs = !string.IsNullOrWhiteSpace(request.Filter) ? request.Filter.Split(null) : null;
            List<object> filters = fltrs != null ? new List<object>(fltrs) : new List<object>();
            if (filters != null && filters.Count > 0)
            {
                foreach (object obj in filters)
                {
                    List<Person> temp = (locator[obj.GetType()].DynamicInvoke(obj) as IEnumerable<Person>).ToList();
                    if (result.Count == 0)
                    {
                        // Nie mamy nic innego pozatym wiec chociaż cos - już dobrze
                        result = new List<Person>(temp);
                    }
                    else
                    {
                        // Mamy już jakieś poprzednie wyniki, sprawdzamy wspolne elementy
                        List<Person> unique = result.Intersect(temp).ToList();

                        // Jak w obydwu kolekcjach nie ma nic wspolnego, to wynik de facto jest pusty, bo user szuka cos czego nie ma.
                        result = new List<Person>(unique);
                    }
                }
            }
            else
            {
                result = new List<Person>(users);
            }

            IEnumerable<Person> sorted = null;
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                if (request.SortOrder)
                {
                    sorted = result.OrderBy(x => request.SortBy.ToLower().Equals("firstname") ? x.FirstName : x.LastName);
                }
                else
                {
                    sorted = result.OrderByDescending(x => request.SortBy.ToLower().Equals("firstname") ? x.FirstName : x.LastName);
                }
            }
            else
            {
                sorted = result;
            }

            // Teraz robimy taka paczke, o jaka zostaliśmy poproszeni
            IEnumerable<Person> batch = sorted.Skip((request.Page - 1) * request.TotalPerPage).Take(request.TotalPerPage);

            // Obliczamy ile zostało zakwalifikowanych ale nie oddanych
            int leftBehind = sorted.Count() - batch.Count();

            // Sprawdzamy ile potrzebujemy page'ów, czyli current batch + reszta po stronach + ewentualne niedobitki
            int reminder = 0;
            int pages = Math.DivRem(leftBehind, request.TotalPerPage, out reminder) + 1;
            if (reminder > 0)
            {
                ++pages;
            }

            return new PostRequestProcessorResultUsers { Result = true, Message = "", Users = batch, TotalUsersCount = sorted.Count() };
        }

        private PostRequestProcessorResult Process(HttpPostRequest_AddUser request)
        {
            Person person = new Person
            {
                FirstName = request.Firstname,
                LastName = request.Lastname,
                Height = request.Height,
                Member = request.ClubMember,
                BirthdateDisplay = request.Birthdate,
                Birthdate = (request.Birthdate - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds
            };
            this.m_dataPrivider.Add(person);
            this.m_dataPrivider.Save();

            return new PostRequestProcessorResult { Result = true, Message = "" };
        }

        private PostRequestProcessorResult Process(HttpPostRequest_DropUser request)
        {
            this.m_dataPrivider.Remove(request.UserID);
            this.m_dataPrivider.Save();

            return new PostRequestProcessorResult { Result = true, Message = "" };
        }

        private PostRequestProcessorResult Process(HttpPostRequest_UpdateUser request)
        {
            Person person = new Person
            {
                PersonID = request.UserID,
                FirstName = request.Firstname,
                LastName = request.Lastname,
                Height = request.Height,
                Member = request.ClubMember,
                BirthdateDisplay = request.Birthdate,
                Birthdate = (request.Birthdate - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds
            };
            this.m_dataPrivider.Update(person);
            this.m_dataPrivider.Save();

            return new PostRequestProcessorResult { Result = true, Message = "" };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.m_dataPrivider != null)
            {
                this.m_dataPrivider.Dispose();
                this.m_dataPrivider = null;
            }
            base.Dispose(disposing);
        }
    }
}