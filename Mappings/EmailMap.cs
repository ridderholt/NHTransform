using FluentNHibernate.Mapping;
using NHTransform.Entities;

namespace NHTransform.Mappings
{
    public class EmailMap : ClassMap<Email>
    {
        public EmailMap()
        {
            Table("Email");

            Id(x => x.Id).Column("Id").GeneratedBy.Native();

            Map(x => x.Address).Column("Address");

            References(x => x.Person).Column("PersonId");
        }
    }
}