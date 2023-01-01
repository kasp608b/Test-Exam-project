using System;
using Core.Entities.Entities.BE;
using Core.Services.Validators.Implementations;
using Core.Services.Validators.Interfaces;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace Infrastructure.UnitTests.ValidatorTests
{
    public class DoctorValidatorTest
    {
        private DoctorValidator _doctorValidator;

        public DoctorValidatorTest()
        {
            _doctorValidator = new DoctorValidator();
        }

        [Fact]
        public void DoctorValidator_ShouldBeOfTypeIDoctorValidator()
        {
            _doctorValidator.Should().BeAssignableTo<IDoctorValidator>();
        }

        [Fact]
        public void PhoneValidation_WithValidPhonenumber_ShouldNotThrowError()
        {
            Action action = () => _doctorValidator.PhoneValidation(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().NotThrow<Exception>();

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithNullPhonenumber)]
        public void PhoneValidation_WithNullPhonenumber_ShouldThrowError(Doctor doctorWithNullPhonenumber)
        {
            Action action = () => _doctorValidator.PhoneValidation(doctorWithNullPhonenumber);
            action.Should().Throw<NullReferenceException>().WithMessage("a doctor needs a phone number");

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithInvalidPhonenumber)]
        public void PhoneValidation_WithInvalidPhonenumber_ShouldThrowError(Doctor doctorWithInvalidPhonenumber)
        {
            Action action = () => _doctorValidator.PhoneValidation(doctorWithInvalidPhonenumber);
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid phone number");

        }

        [Fact]
        public void EmailValidation_WithValidEmail_ShouldNotThrowError()
        {
            Action action = () => _doctorValidator.EmailValidation(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().NotThrow<Exception>();

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithNullEmail)]
        public void EmailValidation_WithNullEmail_ShouldThrowError(Doctor doctorWithNullEmail)
        {
            Action action = () => _doctorValidator.EmailValidation(doctorWithNullEmail);
            action.Should().Throw<NullReferenceException>().WithMessage("a doctor needs an email");

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithInvalidEmail)]
        public void EmailValidation_WithInvalidEmail_ShouldThrowError(Doctor doctorWithInvalidEmail)
        {
            Action action = () => _doctorValidator.EmailValidation(doctorWithInvalidEmail);
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid email address");

        }

        [Fact]
        public void LastNameValidation_WithValidLastName_ShouldNotThrowError()
        {
            Action action = () => _doctorValidator.LastNameValidation(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().NotThrow<Exception>();

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithInvalidLastname)]
        public void LastNameValidation_WithInvalidLastName_ShouldThrowError(Doctor doctorWithInvalidLastname)
        {
            Action action = () => _doctorValidator.LastNameValidation(doctorWithInvalidLastname);
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid last name");

        }

        [Fact]
        public void FirstNameValidation_WithValidLastName_ShouldNotThrowError()
        {
            Action action = () => _doctorValidator.FirstNameValidation(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().NotThrow<Exception>();

        }

        [Theory]
        [MemberData(nameof(GetData), parameters: TestData.DoctorWithInvalidFirstname)]
        public void FirstNameValidation_WithInvalidLastName_ShouldThrowError(Doctor doctorWithInvalidFirstname)
        {
            Action action = () => _doctorValidator.FirstNameValidation(doctorWithInvalidFirstname);
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid first name");

        }


        [Fact]
        public void DefaultValidator_WithDoctorThatsNull_ShouldThrowException()
        {
            Action action = () => _doctorValidator.DefaultValidator(null as Doctor);
            action.Should().Throw<NullReferenceException>().WithMessage("Doctor cannot be null");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("t")]
        [InlineData(null)]
        public void DefaultValidator_WithDoctorInvalidFirstName_ShouldThrowException(string firstName)
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = firstName,
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid first name");
        }

        [Theory]
        [InlineData("")]
        [InlineData("t")]
        [InlineData(null)]
        public void DefaultValidator_WithDoctorInvalidLastName_ShouldThrowException(string lastName)
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = lastName,
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid last name");
        }

        [Fact]
        public void DefaultValidator_WithDoctorHasNoEmail_ShouldThrowException()
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                PhoneNumber = "11554477",
            });
            action.Should().Throw<NullReferenceException>().WithMessage("a doctor needs an email");
        }

        [Theory]
        [InlineData("lumby98gmail.com")]
        [InlineData("lumby98@gmailcom")]
        [InlineData("lumby98@gmail.co.uk")]
        [InlineData("")]
        public void DefaultValidator_WithDoctorHasNoValidEmail_ShouldThrowException(string email)
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = email,
                PhoneNumber = "23115177",
            });
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid email address");
        }

        [Fact]
        public void DefaultValidator_WithDoctorHasValidEmail_ShouldNotThrowException()
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = "11554477",
            });
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public void DefaultValidator_WithDoctorHasNoPhoneNumber_ShouldThrowException()
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
            });
            action.Should().Throw<NullReferenceException>().WithMessage("a doctor needs a phone number");
        }

        [Theory]
        [InlineData("1")]
        [InlineData("")]
        [InlineData("235689562014")]
        [InlineData("23-11-51-77")]
        public void DefaultValidator_WithDoctorHasNoValidPhoneNumber_ShouldThrowException(string phoneNumber)
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = phoneNumber
            });
            action.Should().Throw<ArgumentException>().WithMessage("a doctor needs a valid phone number");
        }

        [Theory]
        [InlineData("23115177")]
        [InlineData("23 11 51 77")]
        public void DefaultValidator_WithDoctorHasValidPhoneNumber_ShouldNotThrowException(string phoneNumber)
        {
            Action action = () => _doctorValidator.DefaultValidator(new Doctor()
            {
                FirstName = "Mads",
                LastName = "Lumby",
                DoctorEmailAddress = "lumby98@gmail.com",
                PhoneNumber = phoneNumber
            });
            action.Should().NotThrow<Exception>();
        }

        [Theory]
        [InlineData("lumby98gmail.com")]
        [InlineData("lumby98@gmailcom")]
        [InlineData("lumby98@gmail.co.uk")]
        [InlineData("")]
        public void EmailValidation_WithNoValidId_ShouldThrowException(string email)
        {
            IDoctorValidator doctorValidator = new DoctorValidator();
            Action action = () => doctorValidator.ValidateEmail(email);
            action.Should().Throw<ArgumentException>().WithMessage("This is not a valid email address");
        }

        public static IEnumerable<object[]> GetData(TestData testData)
        {
            return testData switch
            {
                TestData.ValidDoctors => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false } },
                            },

                TestData.DoctorWithNullPhonenumber => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = null, IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = null, IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = null, IsAdmin = false } },
                            },

                TestData.DoctorWithInvalidPhonenumber => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "sefsefsefsef", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "14444444444444444444444444444444444444444444444444444444444444444444", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "11", IsAdmin = false } },
                            },

                TestData.DoctorWithNullEmail => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = null, PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = null, PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = null, PhoneNumber = "09876543", IsAdmin = false } },
                            },

                TestData.DoctorWithInvalidEmail => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = "Mason", DoctorEmailAddress = "doctorgmail.com", PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail", PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "Bullock", DoctorEmailAddress = "1231231245", PhoneNumber = "09876543", IsAdmin = false } },
                            },

                TestData.DoctorWithInvalidLastname => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = "Karl", LastName = null, DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "Peter", LastName = "b", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "Sandra", LastName = "3", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false } },
                            },

                TestData.DoctorWithInvalidFirstname => new List<object[]>
                            {
                                new object[] { new Doctor() { FirstName = null, LastName = "Mason", DoctorEmailAddress = "doctor@gmail.com", PhoneNumber = "23115177", IsAdmin = true } },
                                new object[] { new Doctor() { FirstName = "b", LastName = "Holt", DoctorEmailAddress = "Porter@hotmail.dk", PhoneNumber = "12345678", IsAdmin = false } },
                                new object[] { new Doctor() { FirstName = "3", LastName = "Bullock", DoctorEmailAddress = "SB@Yahoo.uk", PhoneNumber = "09876543", IsAdmin = false } },
                            },

                _ => null,
            };
        }

        public enum TestData
        {
            ValidDoctors,
            DoctorWithNullPhonenumber,
            DoctorWithInvalidPhonenumber,
            DoctorWithNullEmail,
            DoctorWithInvalidEmail,
            DoctorWithInvalidLastname,
            DoctorWithInvalidFirstname,
        }

    }
}