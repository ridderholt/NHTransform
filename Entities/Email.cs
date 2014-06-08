namespace NHTransform.Entities
{
    public class Email
    {
        public virtual int Id { get; set; }
        public virtual string Address { get; set; }
        public virtual Person Person { get; set; }
    }
}