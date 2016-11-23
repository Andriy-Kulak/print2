using loreal_print.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.ViewModels
{
    public class BooksViewModel
    {
        public int BookId { get; set; }
        public List<BookYear> SearchYears { get; set; }
        public string Book1 { get; set; }
        public IEnumerable<Book> Books { get; set; }

        public ICollection<BookYear> Years { get; set; }
        public IEnumerable<string> YearsList { get; set; }
        public IEnumerable<string> BooksList { get; set; }
    }
}