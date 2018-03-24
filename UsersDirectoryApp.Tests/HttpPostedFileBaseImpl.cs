using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UsersDirectoryApp.Tests
{
    public class HttpPostedFileBaseImpl : HttpPostedFileBase
    {
        private string m_fileName;
        private System.IO.Stream m_stream;
        public HttpPostedFileBaseImpl(string fileName, string contents)
        {
            this.m_fileName = fileName;

            byte[] byteArray = Encoding.UTF8.GetBytes(contents);
            m_stream = new MemoryStream(byteArray);
        }

        public override string FileName
        {
            get
            {
                return this.m_fileName;
            }
        }

        public override System.IO.Stream InputStream
        {
            get
            {
                return this.m_stream;
            }
        }

        public override int ContentLength
        {
            get
            {
                return (int)this.m_stream.Length;
            }
        }
    }
}
