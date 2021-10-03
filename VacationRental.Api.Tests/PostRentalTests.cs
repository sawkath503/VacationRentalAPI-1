using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models.ViewModels;
using VacationRental.Logic.Models.BindingModels;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class PostRentalTests
    {
        private readonly HttpClient _client;

        public PostRentalTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostRental_ThenAGetReturnsTheCreatedRental()
        {
            var request = new RentalBindingModel
            {
                Units = 25,
                PreparationTimeInDays = 1
            };

            ResourceIdViewModel postResult;
            using (var postResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", request))
            {
                Assert.True(postResponse.IsSuccessStatusCode);
                postResult = await postResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            using (var getResponse = await _client.GetAsync($"/api/v1/rentals/{postResult.Id}"))
            {
                Assert.True(getResponse.IsSuccessStatusCode);

                var getResult = await getResponse.Content.ReadAsAsync<RentalViewModel>();
                Assert.Equal(request.Units, getResult.Units);
            }
        }

        [Theory]
        [InlineData(1, -1)]
        [InlineData(-10, 2)]
        [InlineData(-5, -1)]
        public async Task Post_Should_Return_Bad_Request_With_Invalid_Values(int numberOfUnits, int PreparationDays)
        {
            var request = new RentalBindingModel
            {
                Units = numberOfUnits,
                PreparationTimeInDays = PreparationDays
            };

            ResourceIdViewModel postResult;
            using (var postResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", request))
            {                
                Assert.False(postResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
            }
        }
    }
}
