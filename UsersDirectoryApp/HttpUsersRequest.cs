using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsersDirectoryApp
{
    public class HttpUsersRequestSortParams
    {
        public string SortBy { get; set; }
        public bool Asc { get; set; }
    }

    public class HttpUsersRequest
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public string Filter { get; set; }

        public HttpUsersRequestSortParams SortParams { get; set; }
    }
}