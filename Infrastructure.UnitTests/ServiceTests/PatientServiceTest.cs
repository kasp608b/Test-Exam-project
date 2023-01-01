using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Entities.Entities.BE;
using Core.Entities.Entities.Filter;
using Core.Services.ApplicationServices.Implementations;
using Core.Services.ApplicationServices.Interfaces;
using Core.Services.DomainServices;
using Core.Services.Validators.Interfaces;
using Xunit;
using FluentAssertions;
using Moq;
using Xunit.Sdk;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.UnitTests.ServiceTests
{

    public class PatientServiceTest
    {
        private IService<Patient, string> _patientService;
        private SortedDictionary<string, Patient> _allPatients;
        private Mock<IRepository<Patient, string>> _patientRepoMock;
        private Mock<IPatientValidator> _validatorMock;

        public PatientServiceTest()
        {
            _allPatients = new SortedDictionary<string, Patient>();

            _patientRepoMock = new Mock<IRepository<Patient, string>>();

            _validatorMock = new Mock<IPatientValidator>();


            _patientRepoMock
                .Setup(repo => repo
                    .Add(It.IsAny<Patient>()))
                .Callback<Patient>(patient => _allPatients
                    .Add(patient.PatientCPR, patient))
                .Returns<Patient>(patient => _allPatients[patient.PatientCPR]);

            _patientRepoMock
                .Setup(repo => repo
                    .Edit(It.IsAny<Patient>()))
                .Callback<Patient>(patient => _allPatients[patient.PatientCPR] = patient)
                .Returns<Patient>(patient => _allPatients[patient.PatientCPR]);

            _patientRepoMock
                .Setup(repo => repo
                    .Remove(It
                        .IsAny<string>()))
                .Callback<string>(id => _allPatients.Remove(id))
                .Returns<string>((id) => _allPatients
                    .ContainsKey(id) ? _allPatients[id] : null);

            _patientRepoMock
                .Setup(repo => repo
                    .GetAll(It.IsAny<Filter>()))
                .Returns<Filter>((filter) => new FilteredList<Patient>() { List = _allPatients.Values.ToList(), TotalCount = _allPatients.Count, FilterUsed = filter });


            _patientRepoMock
                .Setup(repo => repo
                    .GetById(It.IsAny<string>()))
                .Returns<string>((CPR) => _allPatients
                    .ContainsKey(CPR)
                    ? _allPatients[CPR]
                    : null);

            _patientRepoMock
                .Setup(repo => repo
                    .Count())
                .Returns(() => _allPatients.Count);

            // Get instances of the mocked repositories
            IRepository<Patient, string> patientRepo = _patientRepoMock.Object;
            IPatientValidator validator = _validatorMock.Object;

            // Create a PatientService 
            _patientService = new PatientService(_patientRepoMock.Object, _validatorMock.Object);
        }



        [Fact]
        public void PatientService_ShouldBeOfTypeIService()
        {
            _patientService.Should()
                .BeAssignableTo<IService<Patient, string>>();



        }

        #region Getall




        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetAllValidPatientsEmptyFilter)]
        public void Getall_EmptyFilterValidPatients_ShouldGetAllPatients(List<Patient> patients, Filter filter)
        {
            //arrange

            foreach (var patient in patients)
            {
                _patientService.Add(patient);
            }
            
            // the doctors in the repository
            var expected = new FilteredList<Patient>()
                { List = _allPatients.Values.ToList(), TotalCount = _allPatients.Count, FilterUsed = filter };

            expected.TotalCount = _allPatients.Count;
           

            var result = _patientService.GetAll(filter);

            // assert
            Assert.Equal(expected.List, result.List);
            _patientRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(pFilter => pFilter == filter)), Times.Once);
        }

        [Fact]
        public void GetAll_NegativPagging_ShouldThrowException()
        {
            //arrange
            Patient p1 = new Patient() { PatientCPR = "011200-4106" };
            Patient p2 = new Patient() { PatientCPR = "011200-4107" };

            var patients = new List<Patient>() { p1, p2 };
            Filter filter = new Filter() { CurrentPage = -1};

            _allPatients.Add(p1.PatientCPR, p1);
            _allPatients.Add(p2.PatientCPR, p2);
            // the doctors in the repository
            var expected = new FilteredList<Patient>()
                { List = _allPatients.Values.ToList(), TotalCount = _allPatients.Count, FilterUsed = filter };

            expected.TotalCount = _allPatients.Count;
           
            Action action = () => _patientService.GetAll(filter);

            // assert
            action.Should().Throw<InvalidDataException>()
                .WithMessage("current page and items pr page can't be negative");
            _patientRepoMock.Verify(repo => repo.GetAll(It.Is<Filter>(pFilter => pFilter == filter)), Times.Never);
        }

        #endregion

        #region GetPatientById

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.GetByIdValidIds)]
        public void GetById_WithValidId_ShouldNotThrowException(Patient c1)
        {
            // arrange
            
            // company c1 exists in the Company repository
            _allPatients.Add(c1.PatientCPR, c1);


            // act
            var result = _patientService.GetById(c1.PatientCPR);

            // assert
            Assert.Equal(c1, result);
            _patientRepoMock.Verify(repo => repo.GetById(It.Is<string>(id => id == c1.PatientCPR)), Times.Once);
            _validatorMock.Verify(validator => validator.ValidateCPR(c1.PatientCPR), Times.Once);
        }

        #endregion


        #region Add
    
        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.AddWithValidPatients)]
        public void Add_WithValidPatient_ShouldNotThrowException(Patient patient)
       {
           // arrange
        

            // act

            _patientService.Add(patient);

           // assert
           Assert.Contains(patient, _allPatients.Values);
           _patientRepoMock.Verify(repo => repo.Add(It.Is<Patient>(c => c == patient)), Times.Once);
           _validatorMock.Verify(validator => validator.DefaultValidator(patient), Times.Once);
        }

       [Fact]
       public void Add_PatientAlreadyInTheDatabase_ShouldThrowException()
       {
           //arrange
           var patient = new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" , PatientEmail = "hans@hotmail.com" , PatientCPR = "150429-0677"};
           
            _allPatients.Add(patient.PatientCPR,patient);

            //act + assert
            Action action = () => _patientService.Add(patient);
            action.Should().Throw<InvalidDataException>().WithMessage("Patient is already in the database");



       }


        #endregion


        #region Edit

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.EditWithValidPatients)]
       public void Edit_ValidPatientWithPatientInDatabse_ShouldNotThrowException(Patient patientnew, Patient PatientOld)
       {
           // arrange
            _allPatients.Add(PatientOld.PatientCPR,PatientOld);

           // act
            _patientService.Edit(patientnew);
            
           // assert
           var updatedPatient = _allPatients[patientnew.PatientCPR];

           updatedPatient.Should().Be(patientnew);

            _validatorMock.Verify(validator => validator.DefaultValidator(patientnew), Times.Once);

        }


       [Fact]
       public void EditPatient_WithPatientNotInTheDatabase_ShouldThrowException()
       {
           // arrange
           var Patient = new Patient()
           {
               PatientCPR = "011200-4106", 
               PatientEmail = "jake@hotmail.com", 
               PatientLastName = "jakeowsky", 
               PatientPhone = "20201090", 
               PatientFirstName = "jake"



           };

           // act
           Action action = () => _patientService.Edit(Patient);

           //assert
           action.Should().Throw<ArgumentException>().WithMessage("Patient is not in the database");

       }



        #endregion

        #region RemovePatientById
        [Fact]
        public void Remove_ValidPatient_ShouldNotThrowException()
        {
            // arrange

            // company c1 exists in the Company repository
            var c1 = new Patient() { PatientCPR = "011200-4106" };
            _allPatients.Add(c1.PatientCPR, c1);

            // act
            var result = _patientService.Remove(c1.PatientCPR);

            //assert
            _validatorMock.Verify(validator => validator.ValidateCPR(c1.PatientCPR), Times.Once);
        }

        [Fact]
        public void RemovePatientById_VerifyRemoved()
        {
            // arrange
            

            // company c1 exists in the Company repository
            var c1 = new Patient() { PatientCPR = "011200-4106" };
            _allPatients.Add(c1.PatientCPR, c1);

            // act
            var result = _patientService.Remove(c1.PatientCPR);

            Assert.Null(_patientRepoMock.Object.GetById(c1.PatientCPR));
            _patientRepoMock.Verify(repo => repo.Remove(It.Is<string>(c => c == c1.PatientCPR )),Times.Once());

        }

        [Fact]
        public void RemoveNonexistantPatient_ShouldThrowException()
        {
            // arrange

            // company c1 exists in the Company repository
            var c1 = new Patient() { PatientCPR = "011200-4106" };

            // act
            Action action = () => _patientService.Remove(c1.PatientCPR);

            action.Should().Throw<ArgumentException>().WithMessage("Nonexistant patient cannot be removed!");
        }

        #endregion

        public static IEnumerable<object[]> GetData(TestData testData)
        {

            return testData switch
            {
                TestData.GetAllValidPatientsEmptyFilter => new List<object[]>
                            {
                                new object[] { new List<Patient> { new Patient() { PatientCPR = "011200-4106" } }, new Filter() {} },
                                new object[] { new List<Patient> { new Patient() { PatientCPR = "011200-4106" }, new Patient() { PatientCPR = "011200-4107" } }, new Filter() {} },
                                new object[] { new List<Patient> { new Patient() { PatientCPR = "011200-4106" }, new Patient() { PatientCPR = "011200-4107" }, new Patient() { PatientCPR = "011200-4108" } }, new Filter() {} }
                            },
                TestData.GetByIdValidIds => new List<object[]>
                            {
                                new object[] {  new Patient() { PatientCPR = "011200-4106" }, },
                                new object[] {  new Patient() { PatientCPR = "011200-4107" }  },
                                new object[] {  new Patient() { PatientCPR = "011200-4108" }  }
                            },

                TestData.AddWithValidPatients => new List<object[]>
                            {
                                new object[] {  new Patient() { PatientCPR = "011200-4106" , PatientEmail = "mike@hotmail.com", PatientFirstName = "mike" , PatientLastName = "mikeowsky" , PatientPhone = "40506090" } },
                                new object[] {  new Patient() { PatientCPR = "011200-4106" ,  }  },
                                new object[] {  new Patient() { PatientCPR = "011200-4106" , PatientFirstName = "mike" , PatientLastName = "mikeowsky" , PatientPhone = "40506090" }  }
                            },
                TestData.EditWithValidPatients => new List<object[]>
                            {
                                new object[] {
                                    new Patient() {
                                    PatientCPR = "011200-4106" ,
                                    PatientEmail = "mike@hotmail.com",
                                    PatientFirstName = "mike" ,
                                    PatientLastName = "mikeowsky" ,
                                    PatientPhone = "40506090" },

                                    new Patient() {
                                    PatientCPR = "011200-4106",
                                    PatientEmail = "jake@hotmail.com",
                                    PatientLastName = "jakeowsky",
                                    PatientPhone = "20201090",
                                    PatientFirstName = "jake" }, },
                                new object[] {
                                    new Patient() {
                                        PatientCPR = "011200-4106" ,  } ,

                                    new Patient() {
                                    PatientCPR = "011200-4106",
                                    PatientEmail = "jake@hotmail.com",
                                    PatientLastName = "jakeowsky",
                                    PatientPhone = "20201090",
                                    PatientFirstName = "jake" }, },
                                new object[] {
                                    new Patient() {
                                        PatientCPR = "011200-4106" ,
                                        PatientFirstName = "mike" ,
                                        PatientLastName = "mikeowsky" ,
                                        PatientPhone = "40506090" } ,

                                    new Patient() {
                                    PatientCPR = "011200-4106",
                                    PatientEmail = "jake@hotmail.com",
                                    PatientLastName = "jakeowsky",
                                    PatientPhone = "20201090",
                                    PatientFirstName = "jake" }, }
                            },
               

                _ => null,
            };
        }

        public enum TestData
        {
            GetAllValidPatientsEmptyFilter,
            GetByIdValidIds,
            AddWithValidPatients,
            EditWithValidPatients

        }
        /* PatientCPR = PatientCPR, 
               PatientEmail = Email, 
               PatientLastName = Lastname, 
               PatientPhone = phone, 
               PatientFirstName = FirstName
         * [InlineData("011200-4106" ,"mike" , "mikeowsky", "mike@hotmail.com" , "40506090" )]
         PatientCPR = PatientCPR, 
               PatientEmail = "jake@hotmail.com", 
               PatientLastName = "jakeowsky", 
               PatientPhone = "20201090", 
               PatientFirstName = "jake"

         */

    }
}
