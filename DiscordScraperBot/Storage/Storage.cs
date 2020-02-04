﻿using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordScraperBot
{
    public class UserPreference
    {
        [PrimaryKey, AutoIncrement]
        public int _id { get; set; }
        public string _category { get; set; }

        public double _minPrice { get; set; }
        public double _maxPrice { get; set; }

        public UserPreference(string category)
        {
            _category = category;
            _minPrice = 0.0;
            _maxPrice = 0.0;
        }

        public UserPreference(string category, double minPrice, double maxPrice)
        {
            _category = category;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }
    }

    public class Storage
    {
        SQLiteConnection _db;

        const string DefaultDbFolder = "Storage";
        const string DefaultDbFilename = "pref.sqlite";
        const string DefaultDbPath = DefaultDbFolder + "/" + DefaultDbFilename;

        public Storage()
        {
            CreateSqliteFile(DefaultDbFolder, DefaultDbFilename);
            _db = new SQLiteConnection(DefaultDbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        }

        public Storage(string dbPath, string dbFilename)
        {
            CreateSqliteFile(dbPath, dbFilename);

            string dbFullPath = dbPath + "/" + dbFilename;
            _db = new SQLiteConnection(dbFullPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        }

        public bool CreatePreferenceTable()
        {
            CreateTableResult result = _db.CreateTable<UserPreference>();
            return result == CreateTableResult.Created;
        }

        public bool DeletePreferenceTable()
        {
            int result = _db.DropTable<UserPreference>();
            return result == 0;
        }

        public List<UserPreference> GetUserPreferences()
        {
            return null;
        }

        public bool InsertUserPreferences(List<UserPreference> preferences)
        {
            int nRows = 0;
            foreach (UserPreference userPreference in preferences)
            {
                nRows += _db.Insert(userPreference);
            }

            return nRows == preferences.Count;
        }

        public bool DeleteUserPreferences(List<UserPreference> preferences)
        {
            int nRows = 0;
            foreach (UserPreference userPreference in preferences)
            {
                nRows += _db.Delete<UserPreference>(userPreference._id);
            }

            return nRows == preferences.Count;
        }

        public int GetNumberOfRows()
        {
            return 0;
        }

        public bool DestroyStorage()
        {
            return false;
        }

        public void CloseStorage()
        {
            _db.Close();
        }

        private void CreateSqliteFile(string path, string filename)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + "/" + filename;
            if (!File.Exists(fullPath))
            {
                using (FileStream fileStream = File.Create(fullPath))
                {
                    // Do nothing, we just wanted to create the file.
                }
            }
        }
    }
}
