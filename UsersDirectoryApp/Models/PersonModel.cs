using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace UsersDirectoryApp.Models
{
    public class Person
    {
        public int PersonID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public double Birthdate { get; set; }

        [Required]
        public double Height { get; set; }

        [Required]
        public bool Member { get; set; }

        [NotMapped]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Birthdate")]
        public DateTime BirthdateDisplay { get; set; }
    }

    /// <summary>
    /// Pomocnicza klaska do tworzenia kolekcji userów z dostarczonego pliku. Nie nadajemy tu ID Dla userów.
    /// </summary>
    public class PersonsFactory
    {
        public static List<Person> Create(Stream stream)
        {
            List<Person> result = new List<Person>();
            using (TextFieldParser csvParser = new TextFieldParser(stream))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.ReadLine();
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

                    Person p = new Person
                    {
                        FirstName = fields[0],
                        LastName = fields[1],
                        Birthdate = (birthdate - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime()).TotalMilliseconds,
                        Height = height,
                        Member = member
                    };

                    result.Add(p);
                }

                return result;
            }
        }
    }
}