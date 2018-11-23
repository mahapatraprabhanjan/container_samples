using Identity.Api.Extensions;
using Identity.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Identity.Api.Data
{
    public class ApplicationDbContextSeed
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public async Task SeedAsync(ApplicationDbContext context, IHostingEnvironment env, ILogger<ApplicationDbContextSeed> logger,
            IOptions<AppSettings> settings, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            try
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;

                if (!context.Users.Any())
                {
                    context.Users.AddRange(useCustomizationData
                        ? GetUsersFromFile(contentRootPath, logger)
                        : GetDefaultUser());
                }

                if(useCustomizationData)
                {
                    //Image implementation
                }
            }
            catch (Exception ex)
            {
                if(retryForAvailability < 10)
                {
                    retryForAvailability++;

                    logger.LogError(ex.Message, $"There is an error migrating data for ApplicationDbContext.");

                    await SeedAsync(context, env, logger, settings, retryForAvailability);
                }
            }
        }

        private IEnumerable<ApplicationUser> GetUsersFromFile(string contentRootPath, ILogger logger)
        {
            var csvFileUsers = Path.Combine(contentRootPath, "Setup", "Users.csv");

            if (!File.Exists(csvFileUsers))
            {
                return GetDefaultUser();
            }

            string[] csvHeaders;

            try
            {
                string[] requiredHeaders = {
                    "firstName",
                    "lastName",
                    "address1",
                    "address2",
                    "city",
                    "state",
                    "zipCode"
                };

                csvHeaders = GetHeaders(requiredHeaders, csvFileUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                return GetDefaultUser();
            }

            List<ApplicationUser> users = File.ReadAllLines(csvFileUsers)
                .Skip(1)
                .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .SelectTry(column => CreateApplicationUser(column, csvHeaders))
                .OnCaughtException(ex => { logger.LogError(ex.Message); return null; })
                .Where(x => x != null)
                .ToList();

            return users;
        }

        private IEnumerable<ApplicationUser> GetDefaultUser()
        {
            var user = new ApplicationUser
            {
                FirstName = "Demo",
                LastName= "Demo",
                Address1 = "Address line 1",
                Address2 = "Address line 2",
                City = "Bangalore",
                State ="Karnataka",
                ZipCode = "560087"
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word");

            return new List<ApplicationUser>
            {
                user
            };
        }

        private ApplicationUser CreateApplicationUser(string[] column, string[] csvHeaders)
        {
            if(column.Count() != csvHeaders.Count())
            {
                throw new Exception($"Column count {column.Count()} not the same as headers count {csvHeaders.Count()}");
            }

            var user = new ApplicationUser
            {
                FirstName = column[Array.IndexOf(csvHeaders, "firstName")].Trim('"').Trim(),
                LastName = column[Array.IndexOf(csvHeaders, "lastName")].Trim('"').Trim(),
                Address1 = column[Array.IndexOf(csvHeaders, "address1")].Trim('"').Trim(),
                Address2 = column[Array.IndexOf(csvHeaders, "address2")].Trim('"').Trim(),
                City = column[Array.IndexOf(csvHeaders, "city")].Trim('"').Trim(),
                State = column[Array.IndexOf(csvHeaders,"state")].Trim('"').Trim(),
                ZipCode = column[Array.IndexOf(csvHeaders, "zipCode")].Trim('"').Trim(),
                PasswordHash = column[Array.IndexOf(csvHeaders, "password")].Trim('"').Trim()
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);

            return user;
        }

        private string[] GetHeaders(string[] requiredHeaders, string csvFile)
        {
            string[] csvHeaders = File.ReadLines(csvFile).First().ToLowerInvariant().Split(',');

            if(csvHeaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"Required header count {requiredHeaders.Count()} is different then read header {csvHeaders.Count()}");
            }

            foreach (var requiredHeader in requiredHeaders)
            {
                if(!csvHeaders.Contains(requiredHeader))
                {
                    throw new Exception($"does not contain required header '{requiredHeader}'");
                }
            }

            return csvHeaders;
        }
    }
}
