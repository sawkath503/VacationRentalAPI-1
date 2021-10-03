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
    public class RentalServiceTests
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

        [Test]
        [TestCase(3, 1)]
        public async Task AddRental_Should_Add_Rental(int numberOfUnits, int preparationDays)
        {
            // arrange
            _fixture = new Fixture();
            var fixtureRentalBindingModel = _fixture.Build<RentalBindingModel>()
                .With(x => x.Units, numberOfUnits)
                .With(x => x.PreparationTimeInDays, preparationDays)
                .Create();

            // act
            var retValue = await _fixtureRentalService.AddRental(fixtureRentalBindingModel).ConfigureAwait(false);
            var addedRental = await _fixtureRentalService.GetById(retValue.Id).ConfigureAwait(false);

            //assert
            Assert.IsFalse(retValue.HasError);
            Assert.IsTrue(retValue.Id > 0);
            Assert.AreEqual(numberOfUnits, addedRental.Units);
            Assert.AreEqual(preparationDays, addedRental.PreparationTimeInDays);            
        }

        private static IEnumerable<TestCaseData> UpdateRentalSuccessCases
        {
            get
            {
                // adding number of units should work
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2) },
                    new RentalBindingModel { Units = 3, PreparationTimeInDays = 1 });

                // reducing number of units should work (if all are not occupied)
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2) },
                    new RentalBindingModel { Units = 1, PreparationTimeInDays = 2 });

                // reducing number of units should work (if all are not occupied)
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today.AddDays(4), 2) },
                    new RentalBindingModel { Units = 1, PreparationTimeInDays = 2 });

                // reducing number of preparation days should work
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 2 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2) },
                    new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 });

                // 2 units, 3 booking, 3rd booking start 4 days (2 nights booking + 2 preparation days) after the start 
                // of other 2 bookings. Reducing number of preparation days should work 
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 2 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today.AddDays(4), 2) },
                    new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 });

                // 2 units, 3 booking, 3rd booking start 4 days (2 nights booking + 2 preparation days) after the start 
                // of other 2 bookings. Increasing both number of units and number of preparation days should work 
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 2 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today.AddDays(4), 2) },
                    new RentalBindingModel { Units = 3, PreparationTimeInDays = 3 });
            }
        }

        [Test]
        [TestCaseSource(nameof(UpdateRentalSuccessCases))]
        public async Task UpdateRental_Should_Successfully_Update_Existing_Rental(RentalBindingModel rental, Tuple<DateTime, int>[] bookingsData, RentalBindingModel updateModel)
        {
            // arrange
            var retValue = await _fixtureRentalService.AddRental(rental).ConfigureAwait(false);
            var addedRentalId = (await _fixtureRentalService.GetById(retValue.Id).ConfigureAwait(false)).Id;

            var newBookingIds = new List<int>();
            _fixture = new Fixture();

            foreach (var booking in bookingsData)
            {
                var fixtureBookingBindingModel = _fixture.Build<BookingBindingModel>()
                    .With(x => x.RentalId, addedRentalId)
                    .With(x => x.Start, booking.Item1)
                    .With(x => x.Nights, booking.Item2)
                    .Create();
                
                newBookingIds.Add((await _fixtureBookingService.AddBooking(fixtureBookingBindingModel).ConfigureAwait(false)).Id);
            }

            // act
            var updatedRental = await _fixtureRentalService.UpdateRental(addedRentalId, updateModel).ConfigureAwait(false);

            //assert
            Assert.IsFalse(newBookingIds.Where(i => i <= 0).Any()); // all booking ids should be positive int
            Assert.AreEqual(bookingsData.Count(), newBookingIds.Count()); // make sure all bookings are entered from testdata
            Assert.IsFalse(updatedRental.HasError);
            Assert.AreEqual(addedRentalId, updatedRental.Id);  //rental added should be the same one updated during test
            Assert.AreEqual(updateModel.Units, updatedRental.Units);
            Assert.AreEqual(updateModel.PreparationTimeInDays, updatedRental.PreparationTimeInDays);
                                    
            // values from before and after the update operation should not be equal
            Assert.IsFalse((rental.PreparationTimeInDays == updateModel.PreparationTimeInDays) && (rental.Units == updateModel.Units));
        }

        private static IEnumerable<TestCaseData> UpdateRentalFailureCases
        {
            get
            {
                // reducing number of units should fail (when all units are occup
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2) },
                    new RentalBindingModel { Units = 1, PreparationTimeInDays = 1 });

                // reducing number of units should fail (not enough time between booking for one unit)
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 1 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today.AddDays(3), 2) },
                    new RentalBindingModel { Units = 1, PreparationTimeInDays = 2 });

                // 2 units, 3 booking, 3rd booking start 4 days (2 nights booking + 2 preparation days) after the start date
                // of other 2 bookings.  Increasing number of preparation days should fail
                yield return new TestCaseData(new RentalBindingModel { Units = 2, PreparationTimeInDays = 2 },
                    new Tuple<DateTime, int>[] { Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today, 2), Tuple.Create(DateTime.Today.AddDays(4), 2) },
                    new RentalBindingModel { Units = 2, PreparationTimeInDays = 3 });
            }
        }

        [Test]
        [TestCaseSource(nameof(UpdateRentalFailureCases))]
        public async Task UpdateRental_Should_Fail_To_Update_Existing_Rental(RentalBindingModel rental, Tuple<DateTime, int>[] bookingsData, RentalBindingModel updateModel)
        {
            // arrange
            var retValue = await _fixtureRentalService.AddRental(rental).ConfigureAwait(false);
            var addedRentalId = (await _fixtureRentalService.GetById(retValue.Id).ConfigureAwait(false)).Id;

            var newBookingIds = new List<int>();

            foreach (var booking in bookingsData)
            {
                var fixtureBookingBindingModel = _fixture.Build<BookingBindingModel>()
                    .With(x => x.RentalId, addedRentalId)
                    .With(x => x.Start, booking.Item1)
                    .With(x => x.Nights, booking.Item2)
                    .Create();

                newBookingIds.Add((await _fixtureBookingService.AddBooking(fixtureBookingBindingModel).ConfigureAwait(false)).Id);
            }

            // act
            var updatedRental = await _fixtureRentalService.UpdateRental(addedRentalId, updateModel).ConfigureAwait(false);

            //assert
            Assert.IsFalse(newBookingIds.Where(i => i <= 0).Any()); // all booking ids should be positive int
            Assert.AreEqual(bookingsData.Count(), newBookingIds.Count()); // make sure all bookings are entered from testdata

            Assert.IsTrue(updatedRental.HasError);

            // when update fails all these values should set to 0
            Assert.AreEqual(0, updatedRental.Id);
            Assert.AreEqual(0, updatedRental.Units);
            Assert.AreEqual(0, updatedRental.PreparationTimeInDays);

            // when update fails, properties from the return value after update operation should not match proposed update model
            Assert.AreNotEqual(updatedRental.PreparationTimeInDays + updatedRental.Units
                , updateModel.PreparationTimeInDays + updateModel.Units);
        }
    }
}
