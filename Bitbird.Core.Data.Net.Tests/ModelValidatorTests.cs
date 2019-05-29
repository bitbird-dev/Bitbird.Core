using System;
using System.ComponentModel.DataAnnotations;
using Bitbird.Core.Data.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validator = Bitbird.Core.Data.Validation.Validator;

namespace Bitbird.Core.Data.Net.Tests
{
    public class Person
    {
        [ValidatorCheckNotNullOrEmpty]
        [ValidatorCheckTrimmed]
        [Required]
        [StringLength(7)]
        public string Name { get; set; }
    }

    public class CreatePersonModel
    {
        [CopyValidatorFrom(typeof(Person), nameof(Person.Name))]
        public string Name { get; set; }
    }

    public class OuterClass
    {
        [ValidatorCheckRecursive]
        public CreatePersonModel CreatePerson { get; set; }
    }

    [TestClass]
    public class ModelValidatorTestsModelValidatorTests
    {
        [TestMethod]
        public void ModelValidatorTestNotNull()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<Person>();

            modelValidator.Validate(
                new Person
                {
                    Name = null
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorTestNotEmpty()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<Person>();

            modelValidator.Validate(
                new Person
                {
                    Name = " \t\n\r"
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorTestTrimmed()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<Person>();

            modelValidator.Validate(
                new Person
                {
                    Name = " Test "
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorTestMaxLength()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<Person>();

            modelValidator.Validate(
                new Person
                {
                    Name = "algjasdogjlskdjglkjdsigejjgokdsplfjlhg"
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }




        [TestMethod]
        public void ModelValidatorCopyTestNotNull()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<CreatePersonModel>();

            modelValidator.Validate(
                new CreatePersonModel
                {
                    Name = null
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestNotEmpty()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<CreatePersonModel>();

            modelValidator.Validate(
                new CreatePersonModel
                {
                    Name = " \t\n\r"
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestTrimmed()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<CreatePersonModel>();

            modelValidator.Validate(
                new CreatePersonModel
                {
                    Name = " Test "
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestMaxLength()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<CreatePersonModel>();

            modelValidator.Validate(
                new CreatePersonModel
                {
                    Name = "algjasdogjlskdjglkjdsigejjgokdsplfjlhg"
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }




        [TestMethod]
        public void ModelValidatorCopyTestRecursiveNotNull()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePerson = 
                        new CreatePersonModel
                        {
                            Name = null
                        }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestRecursiveNotEmpty()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePerson =
                        new CreatePersonModel
                        {
                            Name = " \t\n\r"
                        }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestRecursiveTrimmed()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePerson =
                        new CreatePersonModel
                        {
                            Name = " Test "
                        }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestRecursiveMaxLength()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePerson =
                        new CreatePersonModel
                        {
                            Name = "algjasdogjlskdjglkjdsigejjgokdsplfjlhg"
                        }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
    }
}
