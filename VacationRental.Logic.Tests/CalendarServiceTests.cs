using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoFixture;
using NUnit.Framework;
using VacationRental.Logic.Services;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Repos;
using VacationRental.Logic.Models.BindingModels;
using VacationRental.Data.Models;
using AutoFixture.Kernel;

namespace VacationRental.Logic.Tests
{
    [TestFixture]
    public class CalendarServiceTests
    {
        private Fixture _fixture;
        private RentalService _fixtureRentalService;
        private BookingService _fixtureBookingService;
        private CalendarService _fixtureCalendarService;

        [SetUp]
        public void Initialize()
        {
            var fixtureRentalRepo = new Fixture().Create<RentalRepo>();
            var fixtureBookingRepo = new Fixture().Create<BookingRepo>();

            _fixture = new Fixture();
            _fixture.Register<IRentalRepo>(() => fixtureRentalRepo);
            _fixture.Register<IBookingRepo>(() => fixtureBookingRepo);
            _fixtureRentalService = _fixture.Create<RentalService>();
            _fixtureBookingService = _fixture.Create<BookingService>();
            _fixtureCalendarService = _fixture.Create<CalendarService>();
        }

        public static IEnumerable<TestCaseData> CalendarGetTestData
        {
            get
            {
                yield return new TestCaseData(new RentalBindingModel { Units = 4, PreparationTimeInDays = 1 },
                new BookingBindingModel[] { new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(1) },
                new BookingBindingModel{ Nights = 4, Start = DateTime.Today }, new BookingBindingModel{ Nights = 2, Start = DateTime.Today.AddDays(2) },
                new BookingBindingModel{ Nights = 5, Start = DateTime.Today }, new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(5) }},
                DateTime.Today.AddDays(-1), 15);

                yield return new TestCaseData(new RentalBindingModel { Units = 3, PreparationTimeInDays = 1 },
                new BookingBindingModel[] { new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(1) },
                new BookingBindingModel{ Nights = 4, Start = DateTime.Today }, new BookingBindingModel{ Nights = 2, Start = DateTime.Today.AddDays(3) },
                new BookingBindingModel{ Nights = 1, Start = DateTime.Today }, new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(5) }},
                DateTime.Today.AddDays(-1), 15);
            }
        }

        [Test]
        [TestCaseSource(nameof(CalendarGetTestData))]
        public async Task Get_Should_Assign_Unit_To_Bookings_Within_NumberOfUnits_Range(RentalBindingModel rentalBindingModel, BookingBindingModel[] bookingBindingModels, DateTime start, int nights)
        {
            // arrange
            var rental = await _fixtureRentalService.AddRental(rentalBindingModel).ConfigureAwait(false);            
            var newBookingIds = new List<int>();
                        
            foreach (var booking in bookingBindingModels)
            {
                booking.RentalId = rental.Id;
                var addedBooking= await _fixtureBookingService.AddBooking(booking).ConfigureAwait(false);
                if (addedBooking.Id > 0) newBookingIds.Add(addedBooking.Id);

                if (addedBooking.HasError) Assert.Fail("booking service model has error ");
            }

            // act 
            var calendar = await _fixtureCalendarService.Get(rental.Id, start, nights).ConfigureAwait(false);
            
            var hasItemWithUnitIdLargerThanCount = false;
            foreach (var _ in from d in calendar.Dates
                              from b in d.Bookings
                              where b.Unit > rentalBindingModel.Units
                              select new { })
            {
                hasItemWithUnitIdLargerThanCount = true;
            }

            //assert            
            Assert.AreEqual(bookingBindingModels.Count(), newBookingIds.Count);
            Assert.NotNull(calendar);            
            Assert.False(hasItemWithUnitIdLargerThanCount);
        }
    
    }
}
