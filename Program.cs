using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;
using NHTransform.Entities;
using NHTransform.Transformers;

namespace NHTransform
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = Fluently.Configure()
           .Database(MsSqlConfiguration.MsSql2008
               //.ConnectionString(@"Server=.\SQLEXPRESS;Database=dbTest;User Id=sa; Password=goofy;")
               .ConnectionString(@"Data Source=(localdb)\Projects;Initial Catalog=dbTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False")
               .AdoNetBatchSize(300)
               .ShowSql())
           .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Person>())
           //.ExposeConfiguration(x => new SchemaExport(x).Create(false, true))
           .BuildConfiguration();

            var factory = cfg.BuildSessionFactory();

            using (var session = factory.OpenSession())
            {
                var sql = @"SELECT t1.Id as [Id], t1.Firstname as [Firstname], t1.Surname as [Lastname], t2.Street as [Address.Street], t2.City as [Address.City], 
                            t2.Zipcode as [Address.Zipcode], t3.Address as [Email.Address]
                            FROM Person t1
                                LEFT OUTER JOIN Address t2 ON t1.AddressId = t2.PersonId
                                LEFT OUTER JOIN Email t3 ON t1.Id = t3.PersonId
                            WHERE t1.Id = 1 OR t1.Id = 2";
                var sqlQuery = session.CreateSQLQuery(sql);
                sqlQuery.SetResultTransformer(SQLTransformer.For<Person>(x => x.Id));
                //sqlQuery.SetResultTransformer(new CustomAliasToBeanResultTransformer(typeof(Person)));

                //var person = sqlQuery.UniqueResult<Person>();
                //Console.WriteLine(person.Firstname);
                var list = sqlQuery.List<Person>();

                //var groupBy = list.GroupBy(x => x.Id)
                //    .Select(x =>
                //    {
                //        var model = x.First();
                //        if (model.Email != null)
                //        {
                //            Func<Person, IEnumerable<Email>> collectionSelector = y => y.Email;
                //            model.Email = x.SelectMany(collectionSelector).ToList();
                //        }

                //        return model;
                //    }).ToList();


                foreach (var person in list)
                {
                    Console.WriteLine(person.Firstname);
                }
            }
        }
    }
}
