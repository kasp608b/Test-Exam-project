using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Entities.Entities.BE;
using Core.Services.Validators.Implementations;
using Core.Services.Validators.Interfaces;
using Xunit;
using FluentAssertions;

namespace Infrastructure.UnitTests.ValidatorTests
{
   public class PatientValidatorTest
    {
        private PatientValidator _patientValidator;

        public PatientValidatorTest()
        {
            _patientValidator = new PatientValidator();
        }
        
        [Fact]
        public void PatientValidator_IsOfTypeIPatientValidator()
        {
            _patientValidator.Should().BeAssignableTo<IPatientValidator>();

        }

        [Fact]
        public void  DefaultValidation_WithNullPatient_ShouldThrowException()
        {
           
           Action action = () => _patientValidator.DefaultValidator(null as Patient);

           action.Should().Throw<NullReferenceException>().WithMessage("Patient cannot be null!");

           

        }

       
        [Fact]
        public void  DefaultValidation_WithNullLastName_ShouldThrowException()
        {
            
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = null} as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient Lastname cannot be null or empty!");

           

        }


        [Fact]
        public void  DefaultValidation_WithNullPhone_ShouldThrowException()
        {
            

            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = null} as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient phone number cannot be null or empty!");

           

        }

        [Fact]
        public void  DefaultValidation_WithValidPhone_ShouldNotThrowException()
        {
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "23115177" , PatientEmail = "hans@hotmail.com" , PatientCPR = "150429-0677" } as Patient);

            action.Should().NotThrow<Exception>();

        
        }

        [Theory]
        [InlineData("4020405")]
        [InlineData("40204055555555555")]
        [InlineData("0000000000")]
        [InlineData("9999999p")]
        public void  DefaultValidation_WithInvalidPhone_ShouldThrowException(string phoneNumber)
        {
            
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = phoneNumber} as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient Phone number has to be a valid Phone number");



        }

        [Fact]
        public void  DefaultValidation_WithNullEmail_ShouldThrowException()
        {
            
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050"} as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient e-mail cannot be null or empty!");

           

        }

        [Fact]
        public void  DefaultValidation_WithValidEmail_ShouldNotThrowException()
        {
         
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" , PatientEmail = "hans@hotmail.com"} as Patient);

            action.Should().NotThrow<InvalidDataException>();

           

        }

        [Theory]
        [InlineData("hanshotmail.com")]
        [InlineData("hans@@hotmail.com")]
        [InlineData("hanshotmai@.com")]
        [InlineData("hans@hot")]
        public void  DefaultValidation_WithInvalidEmail_ShouldThrowException(string email)
        {
            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" ,PatientEmail = email} as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient Email has to be a valid Email");

           

        }


        [Fact]
        public void  DefaultValidation_WithNullCPR_ShouldThrowException()
        {

            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" , PatientEmail = "hans@hotmail.com"} as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient CPR cannot be null or empty!");

           

        }


        [Fact]
        public void  DefaultValidation_WithNormalCPRSouldNotThrowExeption()
        {

            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" , PatientEmail = "hans@hotmail.com" , PatientCPR = "150429-0677"} as Patient);

            action.Should().NotThrow<InvalidDataException>();

           

        }

        [Fact]
        public void  DefaultValidation_WithInvalidCPR_ShouldThrowException()
        {

            Action action = () => _patientValidator.DefaultValidator(new Patient(){PatientFirstName = "name" , PatientLastName = "lastname", PatientPhone = "40204050" , PatientEmail = "hans@hotmail.com" ,PatientCPR = "400429-0677"} as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient CPR has to be a valid CPR number");

           

        }

        [Fact]
        public void DefaultValidation_WithNullFirstName_ShouldThrowException()
        {

            Action action = () => _patientValidator.DefaultValidator(new Patient() { PatientFirstName = null } as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient Firstname cannot be null or empty!");



        }

        [Fact]
        public void ValidateFirstName_WithNullFirstName_ShouldThrowException()
        {

            Action action = () => _patientValidator.ValidateFirstName(new Patient() { PatientFirstName = null } as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient Firstname cannot be null or empty!");



        }

        [Fact]
        public void ValidateLastName_WithNullLastName_ShouldThrowException()
        {

            Action action = () => _patientValidator.ValidateLastName(new Patient() { PatientLastName = null } as Patient);

            action.Should().Throw<NullReferenceException>().WithMessage("Patient Lastname cannot be null or empty!");



        }

        [Theory]
        [InlineData("4020405")]
        [InlineData("40204055555555555")]
        [InlineData("0000000000")]
        [InlineData("9999999p")]
        public void ValidatePhone_WithInvalidPhone_ShouldThrowException(string phoneNumber)
        {

            Action action = () => _patientValidator.ValidatePhone(new Patient() { PatientFirstName = "name", PatientLastName = "lastname", PatientPhone = phoneNumber } as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient Phone number has to be a valid Phone number");



        }

        [Theory]
        [InlineData("hanshotmail.com")]
        [InlineData("hans@@hotmail.com")]
        [InlineData("hanshotmai@.com")]
        [InlineData("hans@hot")]
        public void ValidateEmail_WithInvalidEmail_ShouldThrowException(string email)
        {
            Action action = () => _patientValidator.ValidateEmail(new Patient() { PatientFirstName = "name", PatientLastName = "lastname", PatientPhone = "40204050", PatientEmail = email } as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient Email has to be a valid Email");



        }

        [Fact]
        public void ValidateCPR_WithInvalidCPR_ShouldThrowException()
        {

            Action action = () => _patientValidator.ValidateCPR(new Patient() { PatientFirstName = "name", PatientLastName = "lastname", PatientPhone = "40204050", PatientEmail = "hans@hotmail.com", PatientCPR = "400429-0677" } as Patient);

            action.Should().Throw<InvalidDataException>().WithMessage("Patient CPR has to be a valid CPR number");



        }



    }
}
