using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDirectoryApp.Tests
{
    public class UsersDataProviderStub : IDataProvider
    {
        /// <summary>
        /// Instead of a real database, we are using this pretty container.
        /// </summary>
        private List<Models.Person> m_people = new List<Models.Person>();

        /// <summary>
        /// ID provider dla naszych userów. Bedzie symulować przydzielanie identyfikatorów dla nowych użytkowników.
        /// </summary>
        private int m_IdProvider;

        public UsersDataProviderStub()
        {
            this.m_IdProvider = 0;
        }

        public void Add(Models.Person person)
        {
            person.PersonID = m_IdProvider++;
            m_people.Add(person);
        }

        public void Update(Models.Person person)
        {
            Models.Person p = m_people.Find(x => x.PersonID == person.PersonID);
            m_people.Remove(p);
            m_people.Add(person);
        }

        public void Remove(int id)
        {
            Models.Person temp = m_people.Find(x => x.PersonID == id);
            if (temp != null && temp != default(Models.Person))
            {
                m_people.Remove(temp);
            }
        }

        public Models.Person Find(int id)
        {
            Models.Person temp = m_people.Find(x => x.PersonID == id);
            return temp;
        }

        public IEnumerable<Models.Person> GetUsers()
        {
            return m_people;
        }

        public void Reset()
        {
            m_people.Clear();
        }

        public void Save()
        {
            // Nothing to do here. We are not using a physical database here, just manage objects in memory.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources - dispose database object
            }
            // free native resources if there are any.
        }
    }
}
