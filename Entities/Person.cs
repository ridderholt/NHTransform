using System.Collections.Generic;

namespace NHTransform.Entities
{
    public class Person
    {
        public virtual int Id { get; set; }
        public virtual string Firstname { get; set; }
        public virtual string Lastname { get; set; }
        public virtual List<Email> Email { get; set; }
        public virtual Address Address { get; set; }
    }
}