using Bogus;
using Microsoft.EntityFrameworkCore;

namespace PaginationDemo.Models
{
    public static class SeedData
    {
        public static void Initialize(ExternalPatronDbContext context)
        {
            context.Database.Migrate();

            // Check if the database has already been seeded
            if (context.ExternalPatrons.Any())
            {
                return; // DB has been seeded
            }

            // Configure the Faker to generate ExternalPatron data
            var faker = new Faker<ExternalPatron>()
                .RuleFor(h => h.FirstName, f => f.Name.FirstName())
                .RuleFor(h => h.LastName, f => f.Name.LastName())
                .RuleFor(h => h.Nationality, f => f.Address.Country())
                .RuleFor(h => h.Gender, f => f.PickRandom<Gender>())
                .RuleFor(h => h.DateOfBirth, f => f.Date.Past(60, DateTime.Now.AddYears(-18)))
                .RuleFor(h => h.PassportNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(h => h.DateJoined, f => f.Date.Past(10))
                .RuleFor(h => h.CreatedDate, f => f.Date.Recent())
                .RuleFor(h => h.FormStatus, f => f.PickRandom<Status>());

            // Generate the list of ExternalPatron objects
            var externalPatrons = faker.Generate(200);

            // Add the generated ExternalPatrons objects to the context
            context.ExternalPatrons.AddRange(externalPatrons);

            // Save changes to the database
            context.SaveChanges();
        }
    }
}
