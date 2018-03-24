using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using UsersDirectoryApp.Models;

namespace UsersDirectoryApp
{
    public class UsersDataContext : DbContext
    {
        public UsersDataContext()
            : base("name=UsersDataContext")
        {
        }
        public DbSet<Person> Users { get; set; }
    }
}