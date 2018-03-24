using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDirectoryApp.Tests
{
    public class JSonResultUsers
    {
        public List<Models.Person> users { get; set; }
        public int totalItems { get; set; }
    }

    public class UsersEqualityComparer : IEqualityComparer<Models.Person>
    {

        public bool Equals(Models.Person x, Models.Person y)
        {
            return x.PersonID == y.PersonID && x.FirstName == y.FirstName && x.LastName == y.LastName && x.Birthdate == y.Birthdate && x.Height == y.Height && x.Member == y.Member;
        }

        public int GetHashCode(Models.Person obj)
        {
            return obj.PersonID.GetHashCode();
        }
    }
}
