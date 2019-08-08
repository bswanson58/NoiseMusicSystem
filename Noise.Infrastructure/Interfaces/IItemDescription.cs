namespace Noise.Infrastructure.Interfaces {
    public interface IItemDescription {
        string  ItemIdentity { get; }

        string  Name { get; }
        string  Description { get; }
    }
}
