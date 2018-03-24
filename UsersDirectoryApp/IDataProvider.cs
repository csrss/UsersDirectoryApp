using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using UsersDirectoryApp.Controllers;
using UsersDirectoryApp.Models;

namespace UsersDirectoryApp
{
    /// <summary>
    /// Serwis do zarządzania danymi.
    /// </summary>
    public interface IDataProvider : IDisposable
    {
        /// <summary>
        /// Dodajemy usera do kolekcji
        /// </summary>
        /// <param name="person"></param>
        void Add(Person person);

        /// <summary>
        /// Modyfikujemy usera
        /// </summary>
        /// <param name="person"></param>
        void Update(Person person);

        /// <summary>
        /// Usuwamy usera z kolekcji
        /// </summary>
        /// <param name="person"></param>
        void Remove(int id);

        /// <summary>
        /// Wyszukujemy usera po ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Person Find(int id);

        /// <summary>
        /// Zwraca nam wszystkich userów jak leci, nic nie filtruje, nic nie sortuje, taka dziewicza kolekcja.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Person> GetUsers();

        /// <summary>
        /// Czysci wszystkie dane - przywraca początkowy stan 
        /// </summary>
        void Reset();

        /// <summary>
        /// Zapisujemy stan naszego kontenera.
        /// </summary>
        void Save();
    }
}