using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bitbird.Core.Data.Validation;
using JetBrains.Annotations;
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
        [UsedImplicitly]
        public string Name { get; set; }
    }

    public class CreatePersonModel : IIsDeletedFlagEntity, IIsActiveFlagEntity, IIdSetter<long>
    {
        public long Id { get; set; }

        [CopyValidatorFrom(typeof(Person), nameof(Person.Name))]
        [ValidatorCheckUniqueness(true, true)]
        [UsedImplicitly]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class OuterClass
    {
        [ValidatorCheckRecursive]
        [UsedImplicitly]
        public CreatePersonModel CreatePerson { get; set; }

        [ValidatorCheckRecursive]
        [ValidatorCheckDistinct(typeof(CreatePersonModelEqualityProvider))]
        [UsedImplicitly]
        public CreatePersonModel[] CreatePersons { get; set; }
    }

    public class CreatePersonModelEqualityProvider : IDistinctSelectEqualityMemberProvider<CreatePersonModel, string>
    {
        public string GetEqualityMember(CreatePersonModel item)
        {
            return item.Name;
        }
    }

    public class CreatePeopleRepository : IGetQueryByEntity
    {
        [NotNull, ItemNotNull] private readonly CreatePersonModel[] people;

        public CreatePeopleRepository([NotNull, ItemNotNull] CreatePersonModel[] people)
        {
            this.people = people ?? throw new ArgumentNullException(nameof(people));
        }

        public IQueryable<TEntity> GetNonTrackingQuery<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(CreatePersonModel))
                return people.OfType<TEntity>().AsQueryable();

            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetTrackingQuery<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(CreatePersonModel))
                return people.OfType<TEntity>().AsQueryable();

            throw new NotImplementedException();
        }
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
                    Name = new string('x', 100)
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public async Task ModelValidatorTestUnique()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<Person>();

            var people = new []
            {
                new CreatePersonModel { Id = 0, Name = "A", IsDeleted = true },
                new CreatePersonModel { Id = 0, Name = "A", IsActive = false },
                new CreatePersonModel { Id = 0, Name = "A", IsDeleted = true, IsActive = false },
                new CreatePersonModel { Id = 0, Name = "A" },
                new CreatePersonModel { Id = 1, Name = "B" },
                new CreatePersonModel { Id = 2, Name = "C" },
                new CreatePersonModel { Id = 3, Name = "D" },
                new CreatePersonModel { Id = 4, Name = "E" },
                new CreatePersonModel { Id = 5, Name = "F" }
            };
            validator.RegisterQueryByEntity(new CreatePeopleRepository(people));

            var person = people.OfActiveState(ActiveState.Active).OfDeletedState(DeletedState.NotDeleted)
                .Single(x => x.Id == 5);

            await validator.CheckUniqueAsync<CreatePersonModel, CreatePersonModel, string>(
                person.Name, 
                x => x.IsDeleted == false && x.IsActive == true && x.Id != person.Id, 
                x => x.Name,
                x => x.Name);

            validator.ThrowIfHasErrors();

            person = new CreatePersonModel {Id = 6, Name = "F"};

            await validator.CheckUniqueAsync<CreatePersonModel, CreatePersonModel, string>(
                person.Name,
                x => x.IsDeleted == false && x.IsActive == true && x.Id != person.Id,
                x => x.Name,
                x => x.Name);

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
                    Name = new string('x', 100)
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
                            Name = new string('x', 100)
                        }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyTestRecursiveArrayMaxLength()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePersons = new []
                    {
                        new CreatePersonModel
                        {
                            Name = "meh"
                        },
                        new CreatePersonModel
                        {
                            Name = new string('x', 100)
                        }
                    }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
        [TestMethod]
        public void ModelValidatorCopyDistinctTest()
        {
            var validator = new Validator();
            var modelValidator = ModelValidators.GetValidator<OuterClass>();

            modelValidator.Validate(
                new OuterClass
                {
                    CreatePersons = new[]
                    {
                        new CreatePersonModel
                        {
                            Name = "meh"
                        },
                        new CreatePersonModel
                        {
                            Name = "meh"
                        }
                    }
                },
                validator);

            Console.WriteLine(Assert.ThrowsException<ApiErrorException>(() => validator.ThrowIfHasErrors()).ToString());
        }
    }
}
