namespace Imato.MongoDb.Repository
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WithIndexAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;
    }
}