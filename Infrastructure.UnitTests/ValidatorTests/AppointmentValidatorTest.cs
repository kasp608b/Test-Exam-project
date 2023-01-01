using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities.Entities.BE;
using Core.Services.Validators.Implementations;
using Core.Services.Validators.Interfaces;
using Xunit;
using FluentAssertions;

namespace Infrastructure.UnitTests.ValidatorTests
{
    public class AppointmentValidatorTest
    {
        private AppointmentValidator _appointmentValidator;

        public AppointmentValidatorTest()
        {
            _appointmentValidator = new AppointmentValidator();
        }

        [Fact]
        public void AppointmentValidator_ShouldBeOfTypeIAppointmentValidator()
        {
            _appointmentValidator.Should().BeAssignableTo<IAppointmentValidator>();
        }

        [Fact]
        public void CreateValidation_WithAppointmentThatsNull_shouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(null as Appointment);
            action.Should().Throw<NullReferenceException>().WithMessage("Appointment cannot be null");
        }

        [Fact]
        public void CreateValidation_withValidAppointment_ShouldNotThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"

            });

            action.Should().NotThrow<Exception>();
        }


        [Fact]
        public void CreateValidation_withAppointmentHasAnId_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });

            action.Should().Throw<ArgumentException>().WithMessage("A new appointment should not have an id");
        }

        [Fact]
        public void CreateValidation_AppointmentWithNoDate_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a dateTime");
        }

        [Fact]
        public void CreateValidation_AppointmentWithNoDuration_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a duration");
        }

        [Fact]
        public void CreationValidation_AppointmentHasToLongDescription_shouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description =
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohelloh",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("description is too long");
        }

        [Fact]
        public void CreationValidation_AppointmentExpiredDate_shouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(-1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("The date is invalid, you cant set an appointment in the past");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreationValidation_AppointmentNegativeDuration_shouldThrowException(int duration)
        {
            IAppointmentValidator appointmentValidator = new AppointmentValidator();
            Action action = () => appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = duration,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a duration");
        }

        [Fact]
        public void CreationValidation_AppointmentTooLongDuration_shouldThrowException()
        {
            
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 1441,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("The duration cannot be longer than one day");
        }

        [Fact]
        public void CreationValidation_AppointmentNoDoctorEmailAddress_shouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
            });
            action.Should().Throw<ArgumentException>().WithMessage("Appointments needs a doctor");
        }


        [Fact]
        public void EditValidation_WithAppointmentThatsNull_shouldThrowException()
        {
          
            Action action = () => _appointmentValidator.EditValidation(null as Appointment);
            action.Should().Throw<NullReferenceException>().WithMessage("Appointment cannot be null");
        }

        [Fact]
        public void EditValidation_withInvalidAppointment_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });

            action.Should().Throw<ArgumentException>().WithMessage("When updating an appointment you need an id");
        }


        [Fact]
        public void EditValidation_withAppointmentHasAnId_ShouldNotThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });

            action.Should().NotThrow<Exception>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void EditValidation_withAppointmentWithNegativeId_ShouldThrowException(int id)
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = id,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });

            action.Should().Throw<ArgumentException>().WithMessage("When updating an appointment you need an id");
        }

        [Fact]
        public void EditValidation_AppointmentWithNoDate_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a dateTime");
        }

        [Fact]
        public void EditValidation_AppointmentWithNoDuration_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a duration");
        }

        [Fact]
        public void EditValidation_AppointmentHasToLongDescription_shouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description =
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                    "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohelloh",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("description is too long");
        }

        [Fact]
        public void EditValidation_AppointmentExpiredDate_shouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(-1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("The date is invalid, you cant set an appointment in the past");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void EditValidation_AppointmentNegativeDuration_shouldThrowException(int duration)
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = duration,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a duration");
        }

        [Fact]
        public void EditValidation_AppointmentTooLongDuration_shouldThrowException()
        {
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 1441,
                DoctorEmailAddress = "Mads@gmail.com"
            });
            action.Should().Throw<ArgumentException>().WithMessage("The duration cannot be longer than one day");
        }

        [Fact]
        public void EditValidation_AppointmentNoDoctorEmailAddress_shouldThrowException()
        {
            
            Action action = () => _appointmentValidator.EditValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
            });
            action.Should().Throw<ArgumentException>().WithMessage("Appointments needs a doctor");
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void IdValidation_withAppointmentWithNegativeId_ShouldThrowException(int id)
        {
            Action action = () => _appointmentValidator.IdValidation(id);

            action.Should().Throw<ArgumentException>().WithMessage("Id cannot be negative");
        }


        [Fact]
        public void CreateIdValidation_AppointmentWithoutId_ShouldNotThrowException()
        {
            Action action = () => _appointmentValidator.CreateIdValidation(new Appointment()
            {
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public void CreateIdValidation_AppointmentWithId_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.CreateIdValidation(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().Throw<ArgumentException>().WithMessage("A new appointment should not have an id");
        }

        [Fact]
        public void EditIdValidation_AppointmentWithoutId_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.EditIdValidation(new Appointment()
            {
                
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().Throw<ArgumentException>().WithMessage("When updating an appointment you need an id");
        }

        [Fact]
        public void DateValidation_AppointmentWithNoDate_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.DateValidation(new Appointment()
            {
                AppointmentId = 1,
                Description = "my knee hurt",
                DurationInMin = 15,
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a dateTime");
        }

        [Fact]
        public void DurationValidator_AppointmentWithNoDuration_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.DurationValidator(new Appointment()
            {
                AppointmentId = 1,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Description = "my knee hurtdddddddddddddddddddddddddddd",
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().Throw<ArgumentException>().WithMessage("an appointment needs a duration");
        }

        [Fact]
        public void DescriptionValidator_AppointmentWithToLongDesc_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.DescriptionValidator(new Appointment()
            {
                AppointmentId = 1,
                Description = "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello" +
                "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohelloh",
                DoctorEmailAddress = "Mads@gmail.com",
            });

            action.Should().Throw<ArgumentException>().WithMessage("description is too long");
        }

        [Fact]
        public void EmailValidator_AppointmentWithNoDoctor_ShouldThrowException()
        {
            Action action = () => _appointmentValidator.EmailValidator(new Appointment()
            {
                AppointmentId = 1,
                Description = "my knee hurtdddddddddddddddddddddddddddd",
            });

            action.Should().Throw<ArgumentException>().WithMessage("Appointments needs a doctor");
        }


    }
}
