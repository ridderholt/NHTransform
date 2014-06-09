using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NHTransform.Entities;
using NHTransform.Transformers;
using Xunit;
using Xunit.Extensions;

namespace NHTransform.Test
{
    public class CustomTransformerTests
    {
        [Theory, PropertyData("BasicProperties")]
        public void Should_map_basic_properties(object[] tuple, string[] aliases, Person expected)
        {
            var transfomer = new CustomTransformer<Person>();

            var result = transfomer.TransformTuple(tuple, aliases);
            var actual = result as Person;

            actual.Firstname.Should().Be(expected.Firstname);
            actual.Lastname.Should().Be(expected.Lastname);
            actual.Id.Should().Be(expected.Id);
        }

        [Theory, PropertyData("OneToOneProperties")]
        public void Should_map_one_to_one_relation(object[] tuple, string[] aliases, Person expected)
        {
            var transfomer = new CustomTransformer<Person>();

            var result = transfomer.TransformTuple(tuple, aliases);
            var actual = result as Person;

            actual.Firstname.Should().Be(expected.Firstname);
            actual.Lastname.Should().Be(expected.Lastname);
            actual.Id.Should().Be(expected.Id);

            actual.Address.Should().NotBeNull();

            actual.Address.City.Should().Be(expected.Address.City);
            actual.Address.Street.Should().Be(expected.Address.Street);
            actual.Address.Zipcode.Should().Be(expected.Address.Zipcode);
        }

        [Fact]
        public void Should_map_one_to_many_realtions()
        {
            var persons = new List<Person>
            {
                new Person{Firstname = "Robin", Lastname = "Ridderholt", Id = 1},
                new Person{Firstname = "Micke", Lastname = "Larsson", Id = 2, Email = new List<Email>{new Email{Address = "micke@miklar.se"}}},
                new Person{Firstname = "Micke", Lastname = "Larsson", Id = 2, Email = new List<Email>{new Email{Address = "mikael.larsson@laget.se"}}}
            };

            var transformer = new CustomTransformer<Person>(person => person.Id);

            transformer.TransformTuple(new object[] {"Micke", "Larsson", 2, "micke@miklar.se"}, new[] {"Firstname", "Lastname", "Id", "Email.Address"});

            var list = transformer.TransformList(persons) as List<Person>;

            list.Should().HaveCount(2);
            list.Single(x => x.Id == 2).Email.Should().HaveCount(2);
            list.Single(x => x.Id == 1).Email.Should().BeNull();
        }

        public static IEnumerable<object[]> BasicProperties
        {
            get
            {
                yield return new object[] { new object[]{"Test", "Testsson", 1} , new string[]{"Firstname", "Lastname", "Id"}, new Person{Firstname = "Test", Lastname = "Testsson", Id = 1}};
                yield return new object[] { new object[]{"Robin", "Ridderholt", 2} , new string[]{"Firstname", "Lastname", "Id"}, new Person{Firstname = "Robin", Lastname = "Ridderholt", Id = 2}};
                yield return new object[] { new object[]{"Mikael", "Larsson", 3} , new string[]{"Firstname", "Lastname", "Id"}, new Person{Firstname = "Mikael", Lastname = "Larsson", Id = 3}};
            }
        }
        public static IEnumerable<object[]> OneToOneProperties
        {
            get
            {
                yield return
                    new object[]
                    {
                        new object[] {"Test", "Testsson", 1, "Gata", "Stad", "12345"},
                        new string[]
                        {"Firstname", "Lastname", "Id", "Address.Street", "Address.City", "Address.Zipcode"},
                        new Person {Firstname = "Test", Lastname = "Testsson", Id = 1, Address = new Address
                        {
                            City = "Stad",
                            Street = "Gata",
                            Zipcode = "12345"
                        }}
                    };
            }
        } 
    }
}
