using Imato.MongoDb.Repository;
using System;

namespace Imato.MongoDb.Repository.Test
{
    public class TestEntity : BaseEintity
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }

        public TestEntity Copy()
        {
            return (TestEntity)MemberwiseClone();
        }
    }
}