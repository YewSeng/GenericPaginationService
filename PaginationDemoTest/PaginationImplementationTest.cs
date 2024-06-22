using NUnit.Framework;
using NUnit.Framework.Legacy;
using PaginationDemo.Utilities.Implementations;
using PaginationDemo.Models;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using PaginationDemo.Utilities.Repositories;

namespace PaginationDemoTest
{
    [TestFixture]
    public class PaginationImplementationTest
    {
        private ExternalPatronDbContext? _mockContext;
        private PaginationImplementation<ExternalPatron>? _paginationImplementation;
        private Mock<IQueryBuilder<ExternalPatron>>? _mockQueryBuilder;


        private ExternalPatronDbContext GetDbContext()
        {
            // Setup in memory DB
            var options = new DbContextOptionsBuilder<ExternalPatronDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _mockContext = new ExternalPatronDbContext(options);

            // Mock Data in memory DB
            var testDataList = new List<ExternalPatron>
            {
            new ExternalPatron { FormID = 1, HighRiskPatronId = 1, DocumentID = 1, CreatedDate = new DateTime(2022, 6, 5), FirstName = "John", LastName = "Doe", Gender = Gender.Male, DateOfBirth = new DateTime(1980, 1, 1), FormStatus = Status.Approved },
            new ExternalPatron { FormID = 2, HighRiskPatronId = 2, DocumentID = 2, CreatedDate = new DateTime(2021, 2, 5), FirstName = "Jane", LastName = "Smith", Gender = Gender.Female, DateOfBirth = new DateTime(1990, 5, 10), FormStatus = Status.PendingApproval },
            new ExternalPatron { FormID = 3, HighRiskPatronId = 3, DocumentID = 3, CreatedDate = new DateTime(2023, 12, 5), FirstName = "Alice", LastName = "Johnson", Gender = Gender.Female, DateOfBirth = new DateTime(1985, 3, 15), FormStatus = Status.Created },
            new ExternalPatron { FormID = 4, HighRiskPatronId = 4, DocumentID = 4, CreatedDate = new DateTime(2022, 6, 5), FirstName = "Steven", LastName = "Lim", Gender = Gender.Others, DateOfBirth = new DateTime(1988, 1, 1), FormStatus = Status.Rejected },
            new ExternalPatron { FormID = 5, HighRiskPatronId = 5, DocumentID = 5, CreatedDate = new DateTime(2024, 5, 5), FirstName = "Peter", LastName = "Lim", Gender = Gender.Male, DateOfBirth = new DateTime(1993, 5, 10), FormStatus = Status.Approved},
            new ExternalPatron { FormID = 6, HighRiskPatronId = 6, DocumentID = 6, CreatedDate = new DateTime(2024, 6, 5), FirstName = "Jane", LastName = "Street", Gender = Gender.Others, DateOfBirth = new DateTime(1979, 8, 25), FormStatus = Status.PendingApproval }
            };

            foreach (var patron in testDataList)
            {
                _mockContext.ExternalPatrons.Add(patron);
                _mockContext.SaveChanges();
            }

            return _mockContext;
        }

        [SetUp]
        public void Setup()
        {
            // Get Db Context
            _mockContext = GetDbContext();

            // Set up mock services
            _mockQueryBuilder = new Mock<IQueryBuilder<ExternalPatron>>();

            // Initialize PaginationImplementation
            _paginationImplementation = new PaginationImplementation<ExternalPatron>(_mockContext, _mockQueryBuilder.Object, "FormID");
        }

        [Test]
        [DisplayName("Test GetPage method - returns Correct Page Number")]
        public void TestGetPage()
        {
            // Act
            var result = _paginationImplementation?.GetPage(page: 1, pageSize: 2);

            // Assert
            if (result != null)
            {
                ClassicAssert.IsNotNull(result.Count(), "Result should not be null");
                ClassicAssert.AreEqual(2, result.Count());
                ClassicAssert.AreEqual(6, result.First().FormID);
                ClassicAssert.AreEqual(5, result.Skip(1).First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - Date values (Single value)")]
        public void TestFilterBySingleDate()
        {
            // Arrange
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", "1980-01-01");

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result != null)
            {
                ClassicAssert.IsNotNull(result, "Result should not be null");
                ClassicAssert.AreEqual(1, result?.Data.Count());
                ClassicAssert.AreEqual(1, result?.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Single value)")]
        public void TestFilterBySingleDateTime()
        {
            // Arrange
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", "1990-05-10T14:30:00Z");

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(1, result.Value.Data.Count());
                ClassicAssert.AreEqual(2, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Multiple null value)")]
        public void TestFilterByDateTimeListContainingMultipleNullValues()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                null,
                null
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left null value)")]
        public void TestFilterByDateTimeListContainingLeftNullRightDateTime()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                null,
                "1990-05-10T14:30:00Z"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Right null value)")]
        public void TestFilterByDateTimeListContainingLeftDateTimeRightNull()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10T14:30:00Z",
                null
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left Date + Right null value)")]
        public void TestFilterByDateTimeListContainingLeftDateRightNull()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10",
                null
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left null + Right Date value)")]
        public void TestFilterByDateTimeListContainingLeftNullRightDate()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                null,
                "1990-05-10"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left Date + Right Date value)")]
        public void TestFilterByDateTimeListContainingLeftDateRightDate()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10",
                "1993-05-10"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left DateTime + Right DateTime value)")]
        public void TestFilterByDateTimeListContainingLeftDateTimeRightDateTime()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10T14:30:00Z",
                "1993-05-10T16:00:00Z"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left Date + Right DateTime value)")]
        public void TestFilterByDateTimeListContainingLeftDateRightDateTime()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10",
                "1993-05-10T16:00:00Z"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - DateTime values (Left Date + Right Date value)")]
        public void TestFilterByDateTimeListContainingLeftDateTimeRightDate()
        {
            // Arrange
            IEnumerable<Object?> dateList = new List<Object?>
            {
                "1990-05-10T14:30:00Z",
                "1993-05-10"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("DateOfBirth", dateList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - string Data Type")]
        public void TestFilterByFirstNameContaining()
        {
            // Arrange
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("FirstName", "j");

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - Struct (Int Data Type)")]
        public void TestFilterByGenderContaining()
        {
            // Arrange
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("Gender", 0);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(5, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - Multiple Enum values")]
        public void TestFilterByMultipleStatusContaining()
        {
            // Arrange
            IEnumerable<Object?> statusList = new List<Object?>
            {
                0,
                1,
                2
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("FormStatus", statusList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(6, result.Value.Data.First().FormID);
            }
        }

        [Test]
        [DisplayName("Test GetPageBySearchTypeAndSearchTerm method - Multiple String values")]
        public void TestFilterByMultipleStringValuesContaining()
        {
            // Arrange
            IEnumerable<Object?> namesList = new List<Object?>
            {
                "J",
                "te"
            };
            Dictionary<string, Object> dataDict = new Dictionary<string, Object>();
            dataDict.Add("FirstName", namesList);

            // Act
            var result = _paginationImplementation?
                .GetPageBySearchTypeAndSearchTerm(page: 1, pageSize: 2, searchCriteria: dataDict);

            // Assert
            if (result.HasValue && result.Value.Data.Any())
            {
                // Data is not empty
                ClassicAssert.AreEqual(2, result.Value.Data.Count());
                ClassicAssert.AreEqual(5, result.Value.Data.First().FormID);
            }
        }
    }
}
