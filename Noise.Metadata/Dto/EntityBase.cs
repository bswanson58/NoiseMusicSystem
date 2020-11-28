using LiteDB;

namespace Noise.Metadata.Dto {
    class EntityBase {
        public  ObjectId    Id { get; }

        protected EntityBase() {
            Id = ObjectId.NewObjectId();
        }

        protected EntityBase( ObjectId id ) {
            Id = id;
        }
    }
}
