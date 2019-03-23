using System;
using Xunit;
using MSTeams.Karma;
using MSTeams.Karma.Models;
using FluentAssertions;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace MSTeams_Karma.Tests
{
    public class DocumentDBRepositoryTests
    {
        private static readonly string AuthKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
        private static readonly string Endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];

        [Fact(Skip = "This is an integration test. Run locally to test.")]
        public void TestGetDocumentClient()
        {
            DocumentClient client = DocumentDBRepository<KarmaModel>.GetDocumentClient(Endpoint, AuthKey);
            client.ServiceEndpoint.Should().NotBeNull().And.Should().NotBe(string.Empty);
        }

        [Fact(Skip = "This is an integration test. Run locally to test.")]
        public async Task TestDocumentClientWorks()
        {
            DocumentClient client = DocumentDBRepository<KarmaModel>.GetDocumentClient(Endpoint, AuthKey);

            // If it doesn't work, this should throw an exception, which fails the test.
            try
            {
                await client.OpenAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
