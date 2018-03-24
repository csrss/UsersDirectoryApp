using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UsersDirectoryApp;
using UsersDirectoryApp.Controllers;
using Ninject;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace UsersDirectoryApp.Tests.Controllers
{
    /// <summary>
    /// TODO: Description!
    /// </summary>
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = this.Resolve();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Przykladowy test na import userów.
        /// </summary>
        [TestMethod]
        public void Test_File_Upload()
        {
            // Arrange
            HomeController controller = this.Resolve();

            // Act
            HttpPostedFileBaseImpl httpFile = new HttpPostedFileBaseImpl("test.csv", Properties.Resources.data);
            ViewResult result = controller.Upload(httpFile) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            IEnumerable<Models.Person> people = result.Model as IEnumerable<Models.Person>;
            Assert.AreEqual(1000, people.Count(), "Wrong number of imported users!");
        }

        /// <summary>
        /// Przykładowy test na dodawanie userów. Na bazie tego testu można łatywo potworzyć każdy test na manipulacje
        /// userami. Po każdej akcji de facto, wołamy GetUsers na controllerze, i on nam daje to co będziemy widzieć w browserze.
        /// </summary>
        [TestMethod]
        public void Test_Add_Users()
        {
            HomeController controller = this.Resolve();
            List<Models.Person> persons = this.Generate();
            List<Models.Person> used = persons.Take(5).ToList();

            // Tworzymy 5 userów
            used.ForEach(person => 
                {
                    HttpPostRequest_AddUser addUserRequest = new HttpPostRequest_AddUser
                    {
                        Firstname = person.FirstName,
                        Lastname = person.LastName,
                        Birthdate = person.BirthdateDisplay,
                        Height = person.Height,
                        ClubMember = person.Member
                    };

                    // Tworzymy goscia na serwie
                    JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(addUserRequest));
                    JObject jObject = JObject.FromObject(createUserResult.Data);

                    PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                    Assert.IsTrue(resultJson.Result, "Could not create user on web service side!");
                });

            HttpPostRequest_Users request = new HttpPostRequest_Users { ID = "HttpPostRequest_Users", Page = 1, TotalPerPage = 10 };
            JsonResult requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));

            JObject jObjectResult = JObject.FromObject(requestResult.Data);
            PostRequestProcessorResultUsers resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            List<Models.Person> check = used.Intersect(resultUsers.Users, new UsersEqualityComparer()).ToList();
            Assert.AreEqual(used.Count, check.Count, "Got different users!");
        }

        /// <summary>
        /// TODO: Description!
        /// </summary>
        [TestMethod]
        public void Test_Delete_Users()
        {
            HomeController controller = this.Resolve();
            List<Models.Person> persons = this.Generate();
            List<Models.Person> used = persons.Take(50).ToList();

            // Tworzymy 50 userów
            used.ForEach(person =>
            {
                HttpPostRequest_AddUser addUserRequest = new HttpPostRequest_AddUser
                {
                    Firstname = person.FirstName,
                    Lastname = person.LastName,
                    Birthdate = person.BirthdateDisplay,
                    Height = person.Height,
                    ClubMember = person.Member
                };

                // Tworzymy goscia na serwie
                JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(addUserRequest));
                JObject jObject = JObject.FromObject(createUserResult.Data);

                PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                Assert.IsTrue(resultJson.Result, string.Format("Could not create user {0} {1} on web service side!", person.FirstName, person.LastName));
            });

            List<Models.Person> deleted = used.Take(25).ToList();
            deleted.ForEach(person =>
                {
                    HttpPostRequest_DropUser dropUserRequest = new HttpPostRequest_DropUser
                    {
                        UserID = person.PersonID
                    };

                    JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(dropUserRequest));
                    JObject jObject = JObject.FromObject(createUserResult.Data);

                    PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                    Assert.IsTrue(resultJson.Result, string.Format("Could not delete user {0} {1} on web service side!", person.FirstName, person.LastName));
                });

            HttpPostRequest_Users request = new HttpPostRequest_Users { ID = "HttpPostRequest_Users", Page = 1, TotalPerPage = 25 };
            JsonResult requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));

            JObject jObjectResult = JObject.FromObject(requestResult.Data);
            PostRequestProcessorResultUsers resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            // Intersect powinien oddać nam used - deleted
            deleted.ForEach(person =>
                {
                    used.Remove(person);
                });

            List<Models.Person> check = used.Intersect(resultUsers.Users, new UsersEqualityComparer()).ToList();
            Assert.AreEqual(used.Count, check.Count, "Got different users!");
        }

        /// <summary>
        /// TODO: Description!
        /// </summary>
        [TestMethod]
        public void Test_Update_Users()
        {
            HomeController controller = this.Resolve();
            List<Models.Person> persons = this.Generate();
            List<Models.Person> used = persons.Take(50).ToList();

            // Tworzymy 50 userów
            used.ForEach(person =>
            {
                HttpPostRequest_AddUser addUserRequest = new HttpPostRequest_AddUser
                {
                    Firstname = person.FirstName,
                    Lastname = person.LastName,
                    Birthdate = person.BirthdateDisplay,
                    Height = person.Height,
                    ClubMember = person.Member
                };

                // Tworzymy goscia na serwie
                JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(addUserRequest));
                JObject jObject = JObject.FromObject(createUserResult.Data);

                PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                Assert.IsTrue(resultJson.Result, string.Format("Could not create user {0} {1} on web service side!", person.FirstName, person.LastName));
            });

            List<Models.Person> modified = used.Take(25).ToList();
            modified.ForEach(person =>
            {
                person.FirstName += " edited";
                person.LastName += " edited";
                person.Height += 10;
                person.Member = !person.Member;
                person.BirthdateDisplay.AddDays(10);
                person.Birthdate = (person.BirthdateDisplay - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds;

                HttpPostRequest_UpdateUser updateUserRequest = new HttpPostRequest_UpdateUser
                {
                    UserID = person.PersonID,
                    Firstname = person.FirstName,
                    Lastname = person.LastName,
                    Birthdate = person.BirthdateDisplay,
                    Height = person.Height,
                    ClubMember = person.Member
                };

                JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(updateUserRequest));
                JObject jObject = JObject.FromObject(createUserResult.Data);

                PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                Assert.IsTrue(resultJson.Result, string.Format("Could not modify user {0} {1} on web service side!", person.FirstName, person.LastName));
            });

            modified.ForEach(person =>
                {
                    Models.Person p = used.Find(x => x.PersonID == person.PersonID);
                    Assert.IsNotNull(p);

                    p = person;
                });

            HttpPostRequest_Users request = new HttpPostRequest_Users { Page = 1, TotalPerPage = 50 };
            JsonResult requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));

            JObject jObjectResult = JObject.FromObject(requestResult.Data);
            PostRequestProcessorResultUsers resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            List<Models.Person> check = used.Intersect(resultUsers.Users, new UsersEqualityComparer()).ToList();
            Assert.AreEqual(used.Count, check.Count, "Got different users!");
        }

        /// <summary>
        /// TODO: Description! 
        /// </summary>
        [TestMethod]
        public void Test_Filter_Users()
        {
            HomeController controller = this.Resolve();
            List<Models.Person> persons = this.Generate();
            List<Models.Person> used = persons.Take(50).ToList();

            // Tworzymy 50 userów
            used.ForEach(person =>
            {
                HttpPostRequest_AddUser addUserRequest = new HttpPostRequest_AddUser
                {
                    Firstname = person.FirstName,
                    Lastname = person.LastName,
                    Birthdate = person.BirthdateDisplay,
                    Height = person.Height,
                    ClubMember = person.Member
                };

                // Tworzymy goscia na serwie
                JsonResult createUserResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(addUserRequest));
                JObject jObject = JObject.FromObject(createUserResult.Data);

                PostRequestProcessorResult resultJson = jObject.ToObject<PostRequestProcessorResult>();
                Assert.IsTrue(resultJson.Result, string.Format("Could not create user {0} {1} on web service side!", person.FirstName, person.LastName));
            });

            HttpPostRequest_Users request = new HttpPostRequest_Users { Page = 1, TotalPerPage = 10 };
            JsonResult requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));

            JObject jObjectResult = JObject.FromObject(requestResult.Data);
            PostRequestProcessorResultUsers resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            // Sprawdzamy paging
            Assert.IsTrue(resultUsers.Result, "There was an error downloading user from web server!");
            Assert.AreEqual(resultUsers.TotalUsersCount, 50, "Wrong users number on remote web server!");
            Assert.AreEqual(resultUsers.Users.Count(), 10, "Wrong users count returned from web server!");

            // Sprawdzamy czy userzy odpowiednio sie sortują
            // Najpierw sortujemy swoich lokalnych żeby wiedziec co jest gdzie
            IEnumerable<Models.Person> sorted = used.OrderBy(x => x.FirstName);

            // Posortowalismy ascending, teraz wiemy ktory koles bedzie na koncu
            request = new HttpPostRequest_Users { Page = 1, TotalPerPage = 10, SortBy = "firstname", SortOrder = false };
            requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));

            jObjectResult = JObject.FromObject(requestResult.Data);
            resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            Assert.IsTrue(resultUsers.Result, "There was an error downloading user from web server!");
            Assert.IsTrue(sorted.Last().PersonID == resultUsers.Users.First().PersonID, "Users are not sorted correctly!");

            // Sprawdzamy filtrowanie / szukanie
            request = new HttpPostRequest_Users { Page = 1, TotalPerPage = used.Count, Filter = sorted.First().FirstName };
            requestResult = controller.PostRequestProcessor(JsonConvert.SerializeObject(request));
            jObjectResult = JObject.FromObject(requestResult.Data);
            resultUsers = jObjectResult.ToObject<PostRequestProcessorResultUsers>();

            Assert.IsTrue(resultUsers.Result, "There was an error downloading user from web server!");
            // Trzeba obczaic jaki powinien byc wynik
            IEnumerable<Models.Person> expectedResult = 
                sorted.Where(x => x.FirstName.ToLower().StartsWith(sorted.First().FirstName.ToLower()) || 
                    x.LastName.ToLower().StartsWith(sorted.First().FirstName.ToLower()));

            Assert.AreEqual(expectedResult.Count(), resultUsers.Users.Count(), "Wrong number of filtered users!");
        }

        private HomeController Resolve()
        {
            KernelBase kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            return kernel.Get<HomeController>();
        }

        private List<Models.Person> Generate()
        {
            List<Models.Person> result = new List<Models.Person>();
            using (TextFieldParser csvParser = new TextFieldParser(new StringReader(Properties.Resources.data)))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.ReadLine();

                int IdProvider = 0;
                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();

                    DateTime birthdate;
                    if (!DateTime.TryParse(fields[2], out birthdate))
                    {
                        birthdate = DateTime.MinValue;
                    }

                    double height = 0.0;
                    if (!Double.TryParse(fields[3], NumberStyles.Any, CultureInfo.InvariantCulture, out height))
                    {
                        height = 0.0;
                    }

                    bool member = false;
                    if (!Boolean.TryParse(fields[4], out member))
                    {
                        member = false;
                    }

                    
                    Models.Person p = new Models.Person
                    {
                        PersonID = IdProvider++,
                        FirstName = fields[0],
                        LastName = fields[1],
                        BirthdateDisplay = birthdate,
                        Birthdate = (birthdate - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds,
                        Height = height,
                        Member = member
                    };

                    // Dorzucamy do bazy danych
                    result.Add(p);
                }
            }
            return result;
        }
    }
}
