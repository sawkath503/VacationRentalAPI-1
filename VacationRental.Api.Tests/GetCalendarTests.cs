using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models.ViewModels;
using VacationRental.Logic.Models.BindingModels;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class GetCalendarTests
    {
        private readonly HttpClient _client;

        public GetCalendarTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenGetCalendar_ThenAGetReturnsTheCalculatedCalendar()
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 2, PreparationTimeInDays = 1
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // booking 1 starts 3 days from today for 2 nights
            var postBooking1Request = new BookingBindingModel
            {
                 RentalId = postRentalResult.Id,
                 Nights = 2,
                 Start = DateTime.Today.AddDays(3) 
            };

            ResourceIdViewModel postBooking1Result;
            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
                postBooking1Result = await postBooking1Response.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // booking 1 starts 4 days from today for 2 nights
            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 2,
                Start = DateTime.Today.AddDays(4) 
            };

            ResourceIdViewModel postBooking2Result;
            using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            {
                Assert.True(postBooking2Response.IsSuccessStatusCode);
                postBooking2Result = await postBooking2Response.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // get calendar entries for 6 days starting 2 days from today 
            var url = $"/api/v1/calendar?rentalId={postRentalResult.Id}&start={DateTime.Today.AddDays(2).ToString("yyyy-MM-dd")}&nights=6";
            using (var getCalendarResponse = await _client.GetAsync(url))
            {
                Assert.True(getCalendarResponse.IsSuccessStatusCode);

                var getCalendarResult = await getCalendarResponse.Content.ReadAsAsync<CalendarViewModel>();
                
                // should have 6 days of entry
                Assert.Equal(postRentalResult.Id, getCalendarResult.RentalId);
                Assert.Equal(6, getCalendarResult.Dates.Count);

                // day1:  2 days from today (no booking)
                Assert.Equal(DateTime.Today.AddDays(2).Date, getCalendarResult.Dates[0].Date);
                Assert.Empty(getCalendarResult.Dates[0].Bookings);

                // day2: 3 days from today (only 1st booking)
                Assert.Equal(DateTime.Today.AddDays(3).Date, getCalendarResult.Dates[1].Date);                
                Assert.Single(getCalendarResult.Dates[1].Bookings);
                Assert.Contains(getCalendarResult.Dates[1].Bookings, x => x.Id == postBooking1Result.Id);

                // day3: 4 days from today (both bookings)
                Assert.Equal(DateTime.Today.AddDays(4).Date, getCalendarResult.Dates[2].Date);
                Assert.Equal(2, getCalendarResult.Dates[2].Bookings.Count);
                Assert.Contains(getCalendarResult.Dates[2].Bookings, x => x.Id == postBooking1Result.Id);
                Assert.Contains(getCalendarResult.Dates[2].Bookings, x => x.Id == postBooking2Result.Id);

                // day4: 5 days from today (both bookings). Preparation time for booking 1 reflected here
                Assert.Equal(DateTime.Today.AddDays(5).Date, getCalendarResult.Dates[3].Date);
                Assert.Equal(2, getCalendarResult.Dates[3].Bookings.Count);
                Assert.Contains(getCalendarResult.Dates[3].Bookings, x => x.Id == postBooking1Result.Id);
                Assert.Contains(getCalendarResult.Dates[3].Bookings, x => x.Id == postBooking2Result.Id);

                // day5: 6 days from today (2nd booking). Preparation time for booking 2 reflected here
                Assert.Equal(DateTime.Today.AddDays(6).Date, getCalendarResult.Dates[4].Date);
                Assert.Single(getCalendarResult.Dates[4].Bookings);
                Assert.Contains(getCalendarResult.Dates[4].Bookings, x => x.Id == postBooking2Result.Id);

                // day6: 7 days from today (no booking). 
                Assert.Equal(DateTime.Today.AddDays(7).Date, getCalendarResult.Dates[5].Date);
                Assert.Empty(getCalendarResult.Dates[5].Bookings);
            }
        }

        public static IEnumerable<object[]> CalendarData
        {
            get
            {
                yield return new object[] { DateTime.Today.AddDays(2), -2, true };  // nights negative
                yield return new object[] { DateTime.Today.AddDays(1), 2, false };  // invalid rental id
            }
        }

        [Theory, MemberData(nameof(CalendarData))]
        public async Task Post_Should_Return_Bad_Request_With_Invalid_Values(DateTime start, int nights, bool useValidRentalId)
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 2
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var url = $"/api/v1/calendar?rentalId={(useValidRentalId ? postRentalResult.Id : -1)}&start={start.ToString("yyyy-MM-dd")}&nights={nights}";
            using (var getCalendarResponse = await _client.GetAsync(url))
            {
                Assert.False(getCalendarResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, getCalendarResponse.StatusCode);
            }
        }
    }
}
