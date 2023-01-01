using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Entities.Entities.BE;
using Core.Entities.Entities.Filter;
using Core.Services.ApplicationServices.Implementations;
using Core.Services.ApplicationServices.Interfaces;
using Core.Services.DomainServices;
using Core.Services.Validators.Implementations;
using Core.Services.Validators.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.ServiceTests
{
    public class DoctorServiceTest
    {
        private IService<Doctor, string> _doctorService;
        private readonly Mock<IRepository<Doctor, string>> _doctorRepoMock;
        private readonly Mock<IDoctorValidator> _doctorValidatorMock;

        private SortedDictionary<string, Doctor> _allDoctors;

        public DoctorServiceTest()
        {

            _allDoctors = new SortedDictionary<string, Doctor>();
            _doctorRepoMock = new Mock<IRepository<Doctor, string>>();
            _doctorValidatorMock = new Mock<IDoctorValidator>();
            
            _doctorRepoMock
                .Setup(repo => repo
                    .Add(It.IsAny<Doctor>()))
                .Callback<Doctor>(doctor => _allDoctors
                    .Add(doctor.DoctorEmailAddress, doctor))
                .Returns<Doctor>(doctor => _allDoctors[doctor.DoctorEmailAddress]);

            _doctorRepoMock
                .Setup(repo => repo
                    .Edit(It.IsAny<Doctor>()))
                .Callback<Doctor>(doctor => _allDoctors[doctor.DoctorEmailAddress] = doctor)
                .Returns<Doctor>(doctor => _allDoctors[doctor.DoctorEmailAddress]);

            _doctorRepoMock
                .Setup(repo => repo
                    .Remove(It
                        .IsAny<string>()))
                .Callback<string>(email => _allDoctors.Remove(email))
                .Returns<string>((email) => _allDoctors.ContainsKey(email) ? _allDoctors[email] : null);

            _doctorRepoMock
                .Setup(repo => repo
                    .GetAll(It.IsAny<Filter>()))
                .Returns<Filter>((filter) => new FilteredList<Doctor>() { List = _allDoctors.Values.ToList(), TotalCount = _allDoctors.Count, FilterUsed = filter });

            _doctorRepoMock
                .Setup(repo => repo
                    .GetById(It.IsAny<string>()))
                .Returns<string>((email) => _allDoctors
                    .ContainsKey(email) ? _allDoctors[email] : null);

            _doctorRepoMock
                .Setup(repo => repo
                    .Count())
                .Returns(() => _allDoctors.Count);

            // Get instances of the mocked repositories
            IRepository<Doctor, string> repo = _doctorRepoMock.Object;
            IDoctorValidator validator = _doctorValidatorMock.Object;

            // Create a doctorService 
            _doctorService = new DoctorService(repo, validator);
        }

        [Fact]
        public void DoctorService_ValidCompanyRepository_shouldNotBeNull()
        {
            // assert
            Assert.NotNull(_doctorService);
        }

        [Fact]
        public void DoctorService_NormalInitialization_IsOfTypeIService()
        {
            // assert
            _doctorService.Should()
                .BeAssignableTo<IService<Doctor, string>>();
        }

        #region GetAll

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetAllValidDoctorsEmptyFilter)]
        public void GetAll_EmptyFilter_ShouldNotThrowException(List<Doctor> doctors, Filter filter)
        {
            //arrange
            foreach (var doctor in doctors)
            {
                _allDoctors.Add(doctor.DoctorEmailAddress, doctor);
            }

            // the doctors in the repository
            var expected = new FilteredList<Doctor>()
                { List = _allDoctors.Values.ToList(), TotalCount = _allDoctors.Count, FilterUsed = filter };

            expected.TotalCount = _allDoctors.Count;

            // act
            var result = _doctorService.GetAll(filter);

            // assert
            Assert.Equal(expected.List, result.List);
            _doctorRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(dFilter => dFilter == filter)), Times.Once);

        }

        [Fact]
        public void GetAll_CurrentPageNegative_ShoudlThrowException()
        {
            //arrange
            Doctor d1 = new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" };
            Doctor d2 = new Doctor() { DoctorEmailAddress = "michael@hotmail.com" };
            var doctors = new List<Doctor>() { d1, d2 };
            Filter filter = new Filter() {CurrentPage = -1};

            _allDoctors.Add(d1.DoctorEmailAddress, d1);
            _allDoctors.Add(d2.DoctorEmailAddress, d2);


            // the doctors in the repository
            var expected = new FilteredList<Doctor>()
                { List = _allDoctors.Values.ToList(), TotalCount = _allDoctors.Count, FilterUsed = filter };

            expected.TotalCount = _allDoctors.Count;

            // act
            Action action = () => _doctorService.GetAll(filter);

            // assert
            action.Should().Throw<InvalidDataException>().WithMessage("current page and items pr page can't be negative");
            _doctorRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(dFilter => dFilter == filter)), Times.Never);

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetAllIndexOutOfBounds)]
        public void GetAll_IndexOutOfBounds_ShouldThrowException(List<Doctor> doctors, Filter filter)
        {
            //arrange
            foreach (var doctor in doctors)
            {
                _allDoctors.Add(doctor.DoctorEmailAddress, doctor);
            }

            // act
            Action action = () => _doctorService.GetAll(filter);

            // assert
            action.Should().Throw<ArgumentException>().WithMessage("no more doctors");
            _doctorRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(dFilter => dFilter == filter)), Times.Never);

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetAllNoDoctorsSatifyFilter)]
        public void GetAll_NoDoctorsSatisfyFilter_ShouldThrowException(List<Doctor> doctors, Filter filter)
        {
            //arrange
            _doctorRepoMock
                .Setup(repo => repo
                    .GetAll(It.IsAny<Filter>()))
                .Returns<Filter>((filter) => new FilteredList<Doctor>() { List = new List<Doctor> { }, TotalCount = _allDoctors.Count, FilterUsed = filter });

            foreach (var doctor in doctors)
            {
                _allDoctors.Add(doctor.DoctorEmailAddress, doctor);
            }

            // act
            Action action = () => _doctorService.GetAll(filter);

            // assert
            action.Should().Throw<KeyNotFoundException>().WithMessage("Could not find doctors that satisfy the filter");
            _doctorRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(dFilter => dFilter == filter)), Times.Once);

        }



        #endregion

        #region get by id

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetByIdWithValidId)]
        public void GetById_WithValidId_ShouldNotThrowException(List<Doctor> doctors, Doctor doctorToGet )
        {
            // arrange
            _allDoctors.Add(doctorToGet.DoctorEmailAddress, doctorToGet);

            foreach (var doctor in doctors)
            {
                _allDoctors.Add(doctor.DoctorEmailAddress, doctor);
            }

            // act
            var result = _doctorService.GetById(doctorToGet.DoctorEmailAddress);

            Assert.Equal(doctorToGet, result);

            _doctorRepoMock.Verify(repo => repo
                .GetById(It.Is<string>(id => id == doctorToGet.DoctorEmailAddress)), Times.Once);

            _doctorValidatorMock.Verify(validator => validator
                .ValidateEmail(It.Is<string>(id => id == doctorToGet.DoctorEmailAddress)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetByIdWithValidId)]
        public void GetById_DoctorDoesNotExist_shouldThrowException(List<Doctor> doctors, Doctor doctorToGet)
        {
            // arrange
            foreach (var doctor in doctors)
            {
                _allDoctors.Add(doctor.DoctorEmailAddress, doctor);
            }

            // act
            Action action = () => _doctorService.GetById(doctorToGet.DoctorEmailAddress);

            // assert
            action.Should().Throw<KeyNotFoundException>().WithMessage("Doctor does not exist");
            
            _doctorRepoMock.Verify(repo => repo
                .GetById(It.Is<string>(id => id == doctorToGet.DoctorEmailAddress)), Times.Once);

            _doctorValidatorMock.Verify(validator => validator
                .ValidateEmail(It.Is<string>(id => id == doctorToGet.DoctorEmailAddress)), Times.Once);
        }


        #endregion

        #region Add

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.ValidDoctors)]
        public void Add_WithValidDoctor_shouldNotThrowException(Doctor doctorToAdd)
        {
            // act
            Action action = () => _doctorService.Add(doctorToAdd);
            // assert
            action.Should().NotThrow<Exception>();
            Assert.Contains(doctorToAdd, _allDoctors.Values);

            _doctorRepoMock.Verify(repo => repo
                .Add(It.Is<Doctor>(doctor => doctor == doctorToAdd)), Times.Once);

            _doctorValidatorMock.Verify(validator => validator
                .DefaultValidator(It.Is<Doctor>(doctor => doctor == doctorToAdd)), Times.Once);
        }

        #endregion

        #region Edit

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.EditedAndUneditedDoctors)]
        public void Edit_WithValidDoctor_shouldNotThrowException(Doctor uneditedDoctor, Doctor editedDoctor)
        {
            _allDoctors.Add(uneditedDoctor.DoctorEmailAddress, uneditedDoctor);

            // act
            Action action = () => _doctorService.Edit(editedDoctor);
            // assert
            action.Should().NotThrow<Exception>();
            Assert.Equal(_doctorRepoMock.Object.GetById(editedDoctor.DoctorEmailAddress), editedDoctor);

            _doctorRepoMock.Verify(repo => repo
                .Edit(It.Is<Doctor>(doctor => doctor == editedDoctor)), Times.Once);

            _doctorValidatorMock.Verify(validator => validator
                .DefaultValidator(It.Is<Doctor>(doctor => doctor == editedDoctor)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.EditedAndUneditedDoctorsInvalid)]
        public void Edit_WithInValidDoctor_shouldThrowException(Doctor uneditedDoctor, Doctor editedDoctor)
        {
            // arrange
            _allDoctors.Add(uneditedDoctor.DoctorEmailAddress, uneditedDoctor);

            // act
            Action action = () => _doctorService.Edit(editedDoctor);
            // assert
            action.Should().Throw<ArgumentException>().WithMessage("A doctor with this email does not exist");

            _doctorRepoMock.Verify(repo => repo
                .Edit(It.Is<Doctor>(doctor => doctor == editedDoctor)), Times.Never);

            _doctorValidatorMock.Verify(validator => validator
                .DefaultValidator(It.Is<Doctor>(doctor => doctor == editedDoctor)), Times.Once);
        }


        #endregion

        #region Remove

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.ValidDoctors)]
        public void Remove_WithValidDoctor_shouldNotThrowException(Doctor doctorToAdd)
        {
            // arrange

            _allDoctors.Add(doctorToAdd.DoctorEmailAddress, doctorToAdd);

            // act
            Action action = () => _doctorService.Remove(doctorToAdd.DoctorEmailAddress);
            // assert
            action.Should().NotThrow<Exception>();
            Assert.Null(_doctorRepoMock.Object.GetById(doctorToAdd.DoctorEmailAddress));

            _doctorRepoMock.Verify(repo => repo
                .Remove(It.Is<string>(id => id == doctorToAdd.DoctorEmailAddress)), Times.Once);

            _doctorValidatorMock.Verify(validator => validator
                .ValidateEmail(It.Is<string>(id => id == doctorToAdd.DoctorEmailAddress)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.ValidDoctors)]
        public void Remove_WithNonExistingDoctor_shouldThrowException(Doctor doctorToAdd)
        {
            // act
            Action action = () => _doctorService.Remove(doctorToAdd.DoctorEmailAddress);
            // assert
            action.Should().Throw<KeyNotFoundException>().WithMessage("This doctor does not exist");

            _doctorRepoMock.Verify(repo => repo
                .Remove(It.Is<string>(id => id == doctorToAdd.DoctorEmailAddress)), Times.Never);

            _doctorValidatorMock.Verify(validator => validator
                .ValidateEmail(It.Is<string>(email => email == doctorToAdd.DoctorEmailAddress)), Times.Once);
        }


        #endregion

        public static IEnumerable<object[]> GetData(TestData testData)
        {
            return testData switch
            {
                TestData.GetAllValidDoctorsEmptyFilter => new List<object[]>
                            {  
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" } }, new Filter() },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" } }, new Filter() },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } }, new Filter() },
                            },

                TestData.GetAllIndexOutOfBounds => new List<object[]>
                            {  
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } }, new Filter() { CurrentPage = 2, ItemsPrPage = 3 } },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" }, new Doctor() { DoctorEmailAddress = "leFang@hotmail.fr" }, new Doctor() { DoctorEmailAddress = "hannahBarbera@yahoo.us" }, new Doctor() { DoctorEmailAddress = "nickGak@gmail.dk" } }, new Filter() { CurrentPage = 2, ItemsPrPage = 6 } },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" }, new Doctor() { DoctorEmailAddress = "leFang@hotmail.fr" }, new Doctor() { DoctorEmailAddress = "hannahBarbera@yahoo.us" }, new Doctor() { DoctorEmailAddress = "nickGak@gmail.dk" } }, new Filter() { CurrentPage = 3, ItemsPrPage = 3 } },
                            },

                TestData.GetAllNoDoctorsSatifyFilter => new List<object[]>
                            {
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } }, new Filter() { SearchField = "FirstName", SearchText = "Franklin" } },
                            },

                TestData.GetByIdWithValidId => new List<object[]>
                            {
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "michael@hotmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } }, new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" } },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } }, new Doctor() { DoctorEmailAddress = "michael@hotmail.com" } },
                                new object[] { new List<Doctor>{ new Doctor() { DoctorEmailAddress = "lumby98@gmail.com" }, new Doctor(){ DoctorEmailAddress = "michael@hotmail.com" } }, new Doctor() { DoctorEmailAddress = "flameDog@gmail.uk" } },
                            },

                TestData.ValidDoctors => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false } },
                            },

                TestData.EditedAndUneditedDoctors => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true }, new Doctor() { FirstName = "Charl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "2342533", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false }, new Doctor() { FirstName = "Pyotr", LastName = "Holtz", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "2344223", IsAdmin = false }, },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false }, new Doctor() { FirstName = "Sabrina", LastName = "Balrog", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "34225255", IsAdmin = true } },
                            },

                TestData.EditedAndUneditedDoctorsInvalid => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true }, new Doctor() { FirstName = "Charl", LastName = "Mason", DoctorEmailAddress = "Smek@gmail.com", PhoneNumber = "2342533", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false }, new Doctor() { FirstName = "Pyotr", LastName = "Holtz", DoctorEmailAddress = "Poimd@hotmail.dk", PhoneNumber = "2344223", IsAdmin = false }, },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false }, new Doctor() { FirstName = "Sabrina", LastName = "Balrog", DoctorEmailAddress = "SBFF@Yahoo.uk", PhoneNumber = "34225255", IsAdmin = true } },
                            },

                _ => null,
            };
        }

        public enum TestData
        {
            GetAllValidDoctorsEmptyFilter,
            GetAllIndexOutOfBounds,
            GetAllNoDoctorsSatifyFilter,
            GetByIdWithValidId,
            ValidDoctors,
            EditedAndUneditedDoctors,
            EditedAndUneditedDoctorsInvalid
        }
    }
}