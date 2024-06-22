using NUnit.Framework;
using NUnit.Framework.Legacy;
using PaginationDemo.Utilities.Implementations;
using PaginationDemo.Models;
using System;
using System.Linq.Expressions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace PaginationDemoTest
{
    [TestFixture]
    public class QueryBuilderImplementationTest
    {
        private Mock<ExternalPatronDbContext>? _mockContext;
        private QueryBuilderImplementation<ExternalPatron>? _queryBuilder;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<ExternalPatronDbContext>();
            var testData = new List<ExternalPatron>
        {
            new ExternalPatron
            {
                FormID = 1,
                FirstName = "John",
                LastName = "Doe",
                Nationality = "US",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1980, 1, 1),
                PassportNumber = "ABC123",
                CreatedDate = DateTime.Now,
                FormStatus = Status.PendingApproval
            },
            new ExternalPatron
            {
                FormID = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Nationality = "UK",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1990, 5, 10),
                PassportNumber = "DEF456",
                CreatedDate = DateTime.Now,
                FormStatus = Status.Approved
            },
            new ExternalPatron
            {
                FormID = 3,
                FirstName = "Alice",
                LastName = "Johnson",
                Nationality = "Canada",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1985, 3, 15),
                PassportNumber = "GHI789",
                CreatedDate = DateTime.Now,
                FormStatus = Status.Rejected
            }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<ExternalPatron>>();
            mockSet.As<IQueryable<ExternalPatron>>().Setup(m => m.Provider).Returns(testData.Provider);
            mockSet.As<IQueryable<ExternalPatron>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<ExternalPatron>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<ExternalPatron>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(m => m.Set<ExternalPatron>()).Returns(mockSet.Object);
            _queryBuilder = new QueryBuilderImplementation<ExternalPatron>(_mockContext.Object);
        }

        [Test]
        [DisplayName("Test Where method")]
        public void WhereAddsPredicateToQuery()
        {
            // Arrange
            Expression<Func<ExternalPatron, bool>> predicate = p => p.FormID == 1;

            // Act
            _queryBuilder?.Where(predicate);
            var queryResults = _queryBuilder?.Build()?.ToList();

            // Assert
            ClassicAssert.AreEqual(1, queryResults?.Count);
            ClassicAssert.AreEqual(1, queryResults?[0].FormID);
        }

        [Test]
        [DisplayName("Test OrderBy method")]
        public void OrderBySortsQueryResults()
        {
            // Arrange
            Expression<Func<ExternalPatron, object>> keySelector = p => p.FirstName ?? string.Empty;

            // Act
            _queryBuilder?.OrderBy(keySelector);
            var queryResults = _queryBuilder?.Build().ToList();

            // Assert
            for (int i = 1; i < queryResults?.Count; i++)
            {
                ClassicAssert.That(string.Compare(queryResults[i - 1].FirstName, queryResults[i].FirstName, StringComparison.OrdinalIgnoreCase) <= 0);
            }
        }

        [Test]
        [DisplayName("Test OrderByDescending method")]
        public void OrderByDescendingSortsQueryResultsInDescendingOrder()
        {
            // Arrange
            Expression<Func<ExternalPatron, object>> keySelector = p => p.FirstName ?? string.Empty;

            // Act
            _queryBuilder?.OrderByDescending(keySelector);
            var queryResults = _queryBuilder?.Build().ToList();

            // Assert
            for (int i = 1; i < queryResults?.Count; i++)
            {
               ClassicAssert.That(string.Compare(queryResults[i - 1].FirstName, queryResults[i].FirstName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        [Test]
        [DisplayName("Test Build method")]
        public void BuildReturnsSameQueryAsLastOperation()
        {
            // Arrange
            Expression<Func<ExternalPatron, bool>> predicate = p => p.FormID == 1;
            Expression<Func<ExternalPatron, object>> keySelector = p => p.FirstName ?? string.Empty;

            // Act
            _queryBuilder?.Where(predicate);
            _queryBuilder?.OrderBy(keySelector);
            var builtQuery = _queryBuilder?.Build();

            // Assert
            ClassicAssert.IsNotNull(builtQuery);
            ClassicAssert.AreEqual(_queryBuilder?.Build(), builtQuery);
        }
    }
}

