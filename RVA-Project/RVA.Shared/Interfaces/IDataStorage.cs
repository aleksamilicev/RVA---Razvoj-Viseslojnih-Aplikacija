using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Strategy Pattern - Interfejs za različite formate čuvanja (XML, CSV, JSON)
    public interface IDataStorage
    {
        // Save operacije
        void SaveData<T>(IEnumerable<T> data, string filePath) where T : class;
        void SaveSingleEntity<T>(T entity, string filePath) where T : class;

        // Load operacije
        IEnumerable<T> LoadData<T>(string filePath) where T : class;
        T LoadSingleEntity<T>(string filePath) where T : class;

        // Utility metode
        bool FileExists(string filePath);
        void DeleteFile(string filePath);
        void CreateBackup(string filePath);

        // Properties
        string FileExtension { get; }
        string FormatName { get; }



        /* Alternativna implementacija
         * 
         * 
        // Fokus na Save/Load
        public interface IDataStorage
        {
            void SaveData<T>(IEnumerable<T> data, string filePath) where T : class;
            void SaveSingleEntity<T>(T entity, string filePath) where T : class;

            IEnumerable<T> LoadData<T>(string filePath) where T : class;
            T LoadSingleEntity<T>(string filePath) where T : class;
        }

        // Utility interfejs za rad sa fajlovima
        public interface IFileUtilities
        {
            bool FileExists(string filePath);
            void DeleteFile(string filePath);
            void CreateBackup(string filePath);
        }

        // Odvojeno info o formatu
        public interface IFormatDescriptor
        {
            string FileExtension { get; }
            string FormatName { get; }
        }
        */
    }
}
