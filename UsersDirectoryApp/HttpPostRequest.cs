using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsersDirectoryApp
{
    public class HttpPostRequest
    {
        public HttpPostRequest()
        {
        }
        public HttpPostRequest(string id)
        {
            this.ID = id;
        }

        public string ID { get; set; }
    }

    public class HttpPostRequest_DropDatabase : HttpPostRequest
    {
        public HttpPostRequest_DropDatabase()
            : base("HttpPostRequest_DropDatabase")
        {
        }
    }

    public class HttpPostRequest_AddUser : HttpPostRequest
    {
        public HttpPostRequest_AddUser()
            : base("HttpPostRequest_AddUser")
        {
        }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime Birthdate { get; set; }
        public double Height { get; set; }
        public bool ClubMember { get; set; }
    }

    public class HttpPostRequest_DropUser : HttpPostRequest
    {
        public HttpPostRequest_DropUser()
            : base("HttpPostRequest_DropUser")
        {
        }

        public int UserID { get; set; }
    }

    public class HttpPostRequest_UpdateUser : HttpPostRequest
    {
        public HttpPostRequest_UpdateUser()
            : base("HttpPostRequest_UpdateUser")
        {
        }

        public int UserID { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime Birthdate { get; set; }
        public double Height { get; set; }
        public bool ClubMember { get; set; }
    }

    public class HttpPostRequest_Users : HttpPostRequest
    {
        public HttpPostRequest_Users()
            : base("HttpPostRequest_Users")
        {
        }

        public int Page { get; set; }
        public int TotalPerPage { get; set; }
        public string Filter { get; set; }
        public string SortBy { get; set; }
        public bool SortOrder { get; set; }
    }
}