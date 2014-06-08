using FluentNHibernate.Mapping;
using NHTransform.Entities;

namespace NHTransform.Mappings
{
    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Table("Person");

            Id(x => x.Id).Column("Id").GeneratedBy.Native();

            Map(x => x.Firstname).Column("Firstname");
            Map(x => x.Lastname).Column("Surname");

            References(x => x.Address).Column("AddressId");

            HasMany(x => x.Email).KeyColumn("PersonId");
        }
    }
}