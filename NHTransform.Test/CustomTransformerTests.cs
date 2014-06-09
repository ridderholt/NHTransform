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
