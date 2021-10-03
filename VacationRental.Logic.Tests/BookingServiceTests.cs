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

namespace VacationRental.Logic.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private Fixture _fixture;
        private RentalService _fixtureRentalService;
        private BookingService _fixtureBookingService;

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
        }

        public static IEnumerable<TestCaseData> AddBooking
        {
            get { yield return new TestCaseData(DateTime.Today, 1); }
        }

        [Test]
        [TestCaseSource(nameof(AddBooking))]
        public async Task AddBooking_Should_Return_Valid_BookingId(DateTime start, int nights)
        {
            // arrange
            _fixture = new Fixture();
            var fixtureRentalBindingModel = _fixture.Build<RentalBindingModel>()
                .With(x => x.Units, 3)
                .With(x => x.PreparationTimeInDays, 1)
                .Create();

            var rental = await _fixtureRentalService.AddRental(fixtureRentalBindingModel).ConfigureAwait(false);

            var fixtureBookingBindingModel = _fixture.Build<BookingBindingModel>()
                .With(x => x.RentalId, rental.Id)
                .With(x => x.Start, start)
                .With(x => x.Nights, 1)
                .Create();
            
            var newBookingId = (await _fixtureBookingService.AddBooking(fixtureBookingBindingModel).ConfigureAwait(false)).Id;

            //act
            var booking = await _fixtureBookingService.GetById(newBookingId).ConfigureAwait(false);

            //assert
            Assert.IsFalse(booking.HasError);

            Assert.IsTrue(booking.Id > 0);
            Assert.AreEqual(start, booking.Start);
            Assert.AreEqual(nights, booking.Nights);
        }

        public static IEnumerable<TestCaseData> AddBooking2
        {
            get { yield return new TestCaseData(DateTime.Today, 1); }
        }

        [Test]
        [TestCaseSource(nameof(AddBooking2))]
        public async Task AddBooking_Should_Return_Error_With_Invalid_Rental_Id(DateTime start, int nights)
        {
            // arrange
            _fixture = new Fixture();
            var fixtureRentalBindingModel = _fixture.Build<RentalBindingModel>()
                .With(x => x.Units, 3)
                .With(x => x.PreparationTimeInDays, 1)
                .Create();

            var rental = await _fixtureRentalService.AddRental(fixtureRentalBindingModel);

            var fixtureBookingBindingModel = _fixture.Build<BookingBindingModel>()
                .With(x => x.RentalId, rental.Id + _fixture.Create<int>())
                .With(x => x.Start, start)
                .With(x => x.Nights, nights)
                .Create();

            // act
            var booking = await _fixtureBookingService.AddBooking(fixtureBookingBindingModel).ConfigureAwait(false);

            //assert
            Assert.IsTrue(booking.HasError);
            Assert.IsTrue(booking.ErrorMessage.Contains("Rental not found"));
            Assert.IsTrue(booking.Id == 0);
        }

        public static IEnumerable<TestCaseData> AddBookingSucceedsTestData
        {
            get
            {
                yield return new TestCaseData(new RentalBindingModel { Units = 4, PreparationTimeInDays = 1 },
                new BookingBindingModel[] { new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(1) },
                new BookingBindingModel{ Nights = 4, Start = DateTime.Today }, new BookingBindingModel{ Nights = 2, Start = DateTime.Today.AddDays(2) },
                new BookingBindingModel{ Nights = 5, Start = DateTime.Today }, new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(5) }});                
            }
        }

        [Test]
        [TestCaseSource(nameof(AddBookingSucceedsTestData))]
        public async Task AddBooking_Should_Add_Booking_When_No_Conflict(RentalBindingModel rentalBindingModel, BookingBindingModel[] bookingBindingModels)
        {
            // arrange
            var addedRentalId = (await _fixtureRentalService.AddRental(rentalBindingModel).ConfigureAwait(false)).Id;
            var newBookingIds = new List<int>();

            // act & assert
            foreach (var booking in bookingBindingModels)
            {
                booking.RentalId = addedRentalId;
                var addBooking = await _fixtureBookingService.AddBooking(booking).ConfigureAwait(false);
                if (addBooking.Id > 0) newBookingIds.Add(addBooking.Id);

                if (addBooking.HasError) Assert.Fail("booking service model has error ");
            }

            Assert.AreEqual(bookingBindingModels.Count(), newBookingIds.Count);
        }

        public static IEnumerable<TestCaseData> AddBookingFailsTestData
        {
            get
            {
                yield return new TestCaseData(new RentalBindingModel { Units = 4, PreparationTimeInDays = 1 },
                new BookingBindingModel[] { new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(1) },
                new BookingBindingModel{ Nights = 4, Start = DateTime.Today }, new BookingBindingModel{ Nights = 2, Start = DateTime.Today.AddDays(2) },
                new BookingBindingModel{ Nights = 5, Start = DateTime.Today }, new BookingBindingModel{ Nights = 3, Start = DateTime.Today.AddDays(3) }});
            }
        }

        [Test]
        [TestCaseSource(nameof(AddBookingFailsTestData))]
        public async Task AddBooking_Should_Not_Add_Rental_When_Conflict(RentalBindingModel rentalBindingModel, BookingBindingModel[] bookingBindingModels)
        {
            // arrange
            var addedRentalId = (await _fixtureRentalService.AddRental(rentalBindingModel).ConfigureAwait(false)).Id;
            var newBookingIds = new List<int>();

            // act & assert
            foreach (var booking in bookingBindingModels)
            {
                booking.RentalId = addedRentalId;
                var addedBooking = await _fixtureBookingService.AddBooking(booking).ConfigureAwait(false);
                if (addedBooking.Id > 0) newBookingIds.Add(addedBooking.Id);
            }

            // assert
            Assert.AreNotEqual(bookingBindingModels.Count(), newBookingIds.Count);
        }
    }
}
