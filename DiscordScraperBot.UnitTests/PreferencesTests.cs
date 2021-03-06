﻿using System;
using Xunit;
using DiscordScraperBot;
using System.Collections.Generic;

namespace DiscordScraperBot.UnitTests
{
    public class PreferencesTests
    {
        [Fact]
        public void NullStorageTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Preferences pref = new Preferences(null);
            });
        }

        [Fact]
        public void AddingNullCategoryTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Assert.Throws<ArgumentNullException>(() =>
            {
                preferences.AddCategory(null);
            });
        }

        [Fact] 
        public void AddingCategoriesTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            List<string> categories = new List<string>();

            categories.Add("gardening");
            categories.Add("electronics");
            categories.Add("test category");

            foreach (string category in categories)
            {
                Assert.True( preferences.AddCategory(category) );
            }

            // Ensure these categories have been added to the mock storage
            List<UserPreference> mockPreferences = storage.GetUserPreferences();
            foreach (string category in categories)
            {
                bool found = false;
                foreach (UserPreference pref in mockPreferences)
                {
                    if (pref._category == category)
                    {
                        found = true;
                    }
                }

                Assert.True(found);
            }

            // Ensure these categories have been added to the cache storage
            foreach (string category in categories)
            {
                bool found = false;
                UserPreference userPref;
                found = preferences.FindUserPreferenceFromCache(category, out userPref);
                Assert.True(found);
                Assert.NotNull(userPref);
            }
        }

        [Fact]
        public void RemovingNullCategoryTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Assert.Throws<ArgumentNullException>(() =>
            {
                preferences.RemoveCategory(null);
            });
        }

        [Fact]
        public void RemovingCategoriesTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            // Add some categories to the mock storage
            List<string> categories = new List<string>();

            categories.Add("gardening");
            categories.Add("electronics");
            categories.Add("test category");
            categories.Add("policeman");
            categories.Add("policewoman");

            foreach (string category in categories)
            {
                preferences.AddCategory(category);
            }

            // Remove some categories 
            Assert.True( preferences.RemoveCategory("policeman") );
            Assert.True( preferences.RemoveCategory("policewoman") );

            // Ensure these filters are removed from the persistence storage
            List<UserPreference> mockPreferences = storage.GetUserPreferences();
            foreach (UserPreference pref in mockPreferences)
            {
                Assert.False(pref._category == "policeman");
                Assert.False(pref._category == "policewoman");
            }

            UserPreference userPrefOne, userPrefTwo;
            Assert.False(preferences.FindUserPreferenceFromCache("policeman", out userPrefOne));
            Assert.False(preferences.FindUserPreferenceFromCache("policewoman", out userPrefTwo));
        }

        [Fact]
        public void AddingNullPriceRangeTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Assert.Throws<ArgumentNullException>(() =>
            {
                preferences.AddPriceRange(null, null);
            });
        }

        /***
         * Ensures that a price range can be correctly applied to a category. 
         */
        [Fact] 
        public void AddingPriceRangeTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            List<string> categories = new List<string>();

            categories.Add("test_cat");
            categories.Add("test_cat2");
            categories.Add("test_cat3");

            foreach (string category in categories) 
            {
                preferences.AddCategory(category);
            }

            Tuple<double, double> priceRange = new Tuple<double, double>(10.0, 100.0);
            preferences.AddPriceRange("test_cat2", priceRange);

            // Check that the category was updated in the mock storage
            UserPreference userPreference = storage.GetUserPreference("test_cat2");
            Assert.Equal("test_cat2", userPreference._category);
            Assert.True(userPreference._minPrice == 10.0);
            Assert.True(userPreference._maxPrice == 100.0);
            
            Assert.True(preferences.FindUserPreferenceFromCache("test_cat2", out userPreference));
            Assert.NotNull(userPreference);
            Assert.True(userPreference._minPrice == 10.0);
            Assert.True(userPreference._maxPrice == 100.0);
        }

        /***
         * Ensures that the user is not able to add a price range for a NULL category.
         */
        [Fact]
        public void AddPriceWithNullCategoryTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Tuple<double, double> priceRange = new Tuple<double, double>(10.0, 100.0);
            Assert.Throws<ArgumentNullException>(() =>
            {
                preferences.AddPriceRange(null, priceRange);
            });
        }

        /***
         * Ensures that the user is unable to add a price range for a category that does NOT exist.
         */
        [Fact]
        public void AddPriceWithFakeCategoryTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Tuple<double, double> priceRange = new Tuple<double, double>(10.0, 100.0);

            Assert.Throws<Preferences.UserPreferenceNotFoundException>(() =>
            {
                preferences.AddPriceRange("fake_category", priceRange);
            });
        }

        [Fact] 
        public void RemoveNullPriceRangeTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            Assert.Throws<ArgumentNullException>(() =>
            {
                preferences.RemovePriceRange(null);
            });
        }

        [Fact]
        public void RemovePriceRangeTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            List<string> categories = new List<string>();

            categories.Add("test_cat");
            categories.Add("test_cat2");
            categories.Add("test_cat3");

            Tuple<double, double> priceRange = new Tuple<double, double>(10.0, 100.0);
            foreach (string category in categories) 
            {
                preferences.AddCategory(category);
                preferences.AddPriceRange(category, priceRange);
            }

            Assert.True( preferences.RemovePriceRange("test_cat2") );

            UserPreference userPreference = storage.GetUserPreference("test_cat2");
            Assert.Equal(0.0, userPreference._minPrice);
            Assert.Equal(0.0, userPreference._maxPrice);

            Assert.True(preferences.FindUserPreferenceFromCache("test_cat2", out userPreference));
            Assert.NotNull(userPreference);
            Assert.True(userPreference._minPrice == 0.0);
            Assert.True(userPreference._maxPrice == 0.0);
        }

        /*
         * Ensure that we can retrieve price ranges for categories.
         */
        [Fact]
        public void RetrievePriceRangeTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            preferences.AddCategory("cars");

            Tuple<double, double> priceRange = new Tuple<double, double>(100.0, 856.0);
            preferences.AddPriceRange("cars", priceRange);

            Tuple<double, double> existingPriceRange = preferences.GetPriceRange("cars");
            Assert.Equal(priceRange, existingPriceRange);
        }

        /***
         * Test that ensures we can retrieve items that were recently added from the cache.
         */
        [Fact]
        public void RetrievePreferenceFromCacheTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            preferences.AddCategory("cars");
            preferences.AddCategory("towns");
            preferences.AddCategory("garages");

            UserPreference pref;
            Assert.True(preferences.FindUserPreferenceFromCache("cars", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("towns", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("garages", out pref));
        }

        /***
         * Test that ensures non-existant items cannot be retrieved from the cache.
         */
        [Fact]
        public void RetrieveFakePreferenceFromCacheTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            UserPreference pref;
            Assert.False(preferences.FindUserPreferenceFromCache("cars", out pref));
        }

        /***
         * Test that ensures that removed items cannot be retrieved from the cache.
         */
        [Fact]
        public void RetrieveRemovedPreferenceFromCacheTest()
        {
            IStorage storage = new MockStorage();
            Preferences preferences = new Preferences(storage);

            preferences.AddCategory("cars");
            preferences.AddCategory("towns");
            preferences.AddCategory("garages");

            UserPreference pref;
            Assert.True(preferences.FindUserPreferenceFromCache("cars", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("towns", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("garages", out pref));

            preferences.RemoveCategory("cars");
            Assert.False(preferences.FindUserPreferenceFromCache("cars", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("towns", out pref));
            Assert.True(preferences.FindUserPreferenceFromCache("garages", out pref));
        }
    }
}
