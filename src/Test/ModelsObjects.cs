﻿using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bytewizer.Backblaze.Models;
using System.Diagnostics;
using System.Linq;

namespace Backblaze.Test
{
    [TestClass]
    public class ModelObjects
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cors_Rule_Maximum_Rules()
        {
            var list = new CorsRules();

            for (int i = 0; i < CorsRules.MaximumRulesAllowed + 1; i++)
            {
                list.Add(new CorsRule(
                     Guid.NewGuid().ToString(),
                     new List<string> { "https" },
                     new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                     3600)
                {
                    AllowedHeaders = new List<string> { "range" },
                    ExposeHeaders = new List<string> { "x-bz-content-sha1" },
                }
                );
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cors_Rule_Unique_Name()
        {
            var list = new CorsRules();

            for (int i = 0; i < 2 + 1; i++)
            {
                list.Add(new CorsRule(
                     "123456",
                     new List<string> { "https" },
                     new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                     3600)
                {
                    AllowedHeaders = new List<string> { "range" },
                    ExposeHeaders = new List<string> { "x-bz-content-sha1" },
                }
                );
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Cors_Rule_Length()
        {
            var list = new CorsRules();
            list.Add(new CorsRule(
                    "12345",
                    new List<string> { "https" },
                    new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                    3600)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            }
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Cors_Rule_Invalid_Char()
        {
            var list = new CorsRules();
            list.Add(new CorsRule(
                    "12345$",
                    new List<string> { "https" },
                    new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                    3600)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            }
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cors_Rule_B2()
        {
            var list = new CorsRules();
            list.Add(new CorsRule(
                    "b2-123456",
                    new List<string> { "https" },
                    new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                    3600)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            }
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Cors_Rule_Max_Age()
        {
            var list = new CorsRules();
            list.Add(new CorsRule(
                    "123456",
                    new List<string> { "https" },
                    new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                    100000000)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            }
            );
        }

        [TestMethod]
        public void Cors_Rule()
        {
            var rule1 = new CorsRule(
                     "downloadFromAnyOrigin",
                     new List<string> { "https" },
                     new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                     3600)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            };

            var rule2 = new CorsRule(
                 "downloadFromAnyOrigin",
                 new List<string> { "https" },
                 new List<string> { "b2_download_file_by_id", "b2_download_file_by_name" },
                 3600)
            {
                AllowedHeaders = new List<string> { "range" },
                ExposeHeaders = new List<string> { "x-bz-content-sha1" },
            };

            Assert.IsTrue(rule1.Equals(rule2));
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);

            var rule3 = new CorsRules
                { new CorsRule(
                     "downloadFromAnyOrigin",
                     new List<string> { "https" },
                     new List<string> { "b2_download_file_by_id" , "b2_download_file_by_name" },
                     3600 )
                    {
                        AllowedHeaders = new List<string> { "range" },
                        ExposeHeaders = new List<string> {"x-bz-content-sha1" },
                    }
                };

            var rule4 = new CorsRules
                { new CorsRule(
                     "downloadFromAnyOrigin",
                     new List<string> { "https" },
                     new List<string> { "b2_download_file_by_id" , "b2_download_file_by_name" },
                     3601 )
                    {
                        AllowedHeaders = new List<string> { "range" },
                        ExposeHeaders = new List<string> {"x-bz-content-sha1" },
                    }
                };

            Assert.IsFalse(rule3.Equals(rule4));
            Assert.AreNotEqual(rule3.GetHashCode(), rule4.GetHashCode());
        }

        [TestMethod]
        public void Lifecycle_Rule()
        {
            var rule1 = new LifecycleRule()
            {
                DaysFromHidingToDeleting = 6,
                DaysFromUploadingToHiding = 5,
                FileNamePrefix = "backup/",
            };
            var rule2 = new LifecycleRule()
            {
                DaysFromHidingToDeleting = 6,
                DaysFromUploadingToHiding = 5,
                FileNamePrefix = "backup/",
            };

            Assert.IsTrue(rule1.Equals(rule2));
            Assert.AreEqual(rule1.GetHashCode(), rule2.GetHashCode());
            Assert.IsTrue(rule1 == rule2);
            Assert.IsFalse(rule1 != rule2);

            var rule3 = new LifecycleRules()
                {   new LifecycleRule()
                    {
                        DaysFromHidingToDeleting = 6,
                        DaysFromUploadingToHiding = 5,
                        FileNamePrefix = "backup/",
                    },
                    new LifecycleRule()
                    {
                        DaysFromHidingToDeleting = 45,
                        DaysFromUploadingToHiding = 7,
                        FileNamePrefix = "files/",
                    },
                };

            var rule4 = new LifecycleRules()
                {   new LifecycleRule()
                    {
                        DaysFromHidingToDeleting = 6,
                        DaysFromUploadingToHiding = 5,
                        FileNamePrefix = "backup/",
                    },
                    new LifecycleRule()
                    {
                        DaysFromHidingToDeleting = 45,
                        DaysFromUploadingToHiding = 7,
                        FileNamePrefix = "files/",
                    },
                };

            Assert.IsTrue(rule3.Equals(rule4));
            Assert.AreEqual(rule3.GetHashCode(), rule4.GetHashCode());
            Assert.IsTrue(rule3 == rule4);
            Assert.IsFalse(rule3 != rule4);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Lifecycle_Rule_Maximum_Rules()
        {
            var list = new LifecycleRules();

            for (int i = 0; i < LifecycleRules.MaximumRulesAllowed + 1; i++)
            {
                list.Add(new LifecycleRule()
                {
                    DaysFromHidingToDeleting = 45,
                    DaysFromUploadingToHiding = 7
                });
            }
        }
    }
}
