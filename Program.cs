using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
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
               .ConnectionString(@"Server=.\SQLEXPRESS;Database=dbTest;User Id=sa; Password=goofy;")
               .AdoNetBatchSize(300)
               .ShowSql())
           .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Person>())
           .BuildConfiguration();

            var factory = cfg.BuildSessionFactory();

            using (var session = factory.OpenSession())
            {
                var sql = @"SELECT t1.Id as [Id], t1.Firstname as [Firstname], t1.Surname as [Lastname], t2.Street as [Address.Street], t2.City as [Address.City]
                            FROM Person t1
                                LEFT OUTER JOIN Address t2 ON t1.AddressId = t2.PersonId
                            WHERE t1.Id = 1 OR t1.Id = 2";
                var sqlQuery = session.CreateSQLQuery(sql);
                //sqlQuery.SetResultTransformer(SQLTransformer.For<Person>());
                //sqlQuery.SetResultTransformer(new CustomAliasToBeanResultTransformer(typeof(Person)));

                //var person = sqlQuery.UniqueResult<Person>();
                //Console.WriteLine(person.Firstname);
                var list = sqlQuery.List<Person>();

                //var groupBy = list.GroupBy(x => x.Id)
                //    .Select(x =>
                //    {
                //        var model = x.First();
                //        if (model.Email != null)
                //            model.Email = x.SelectMany(y => y.Email).ToList();

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
