using LiteDB;

namespace MilkBottle.Entities {
    class EntityBase {
        public  ObjectId    Id { get; }

        protected EntityBase( ObjectId id ) {
            Id = id;
        }
    }
}
