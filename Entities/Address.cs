namespace NHTransform.Entities
{
    public class Address
    {
        public virtual int Id { get; set; }
        public virtual string Street { get; set; }
        public virtual string Zipcode { get; set; }
        public virtual string City { get; set; }
        public virtual Person Person { get; set; }
    }
}