using FluentNHibernate.Mapping;
using NHTransform.Entities;

namespace NHTransform.Mappings
{
    public class AddressMap : ClassMap<Address>
    {
        public AddressMap()
        {
            Table("Address");

            Id(x => x.Id).Column("Id").GeneratedBy.Native();

            Map(x => x.Street).Column("Street");
            Map(x => x.City).Column("City");
            Map(x => x.Zipcode).Column("Zipcode");

            References(x => x.Person).Column("PersonId");
        }
    }
}