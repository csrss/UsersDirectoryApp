using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using UsersDirectoryApp.Controllers;
using UsersDirectoryApp.Models;

namespace UsersDirectoryApp
{
    public class UsersDataProvider : IDataProvider
    {
        /// <summary>
        /// W tej implementacji siorbiemy dane z bazy
        /// </summary>
        private UsersDataContext m_context = new UsersDataContext();

        public void Add(Person person)
        {
            m_context.Users.Add(person);
        }

        public void Update(Person person)
        {
            m_context.Entry(person).State = EntityState.Modified;
        }

        public void Remove(int id)
        {
            Person tmp = m_context.Users.Find(id);
            if (tmp != null)
            {
                m_context.Users.Remove(tmp);
            }
        }

        public Person Find(int id)
        {
            Person tmp = m_context.Users.Find(id);
            return tmp;
        }

        public IEnumerable<Person> GetUsers()
        {
            return m_context.Users;
        }

        public void Reset()
        {
            m_context.Database.ExecuteSqlCommand("delete from People");
        }

        public void Save()
        {
            m_context.SaveChanges();
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
                m_context.Dispose();
            }
            // free native resources if there are any.
        }
    }
}