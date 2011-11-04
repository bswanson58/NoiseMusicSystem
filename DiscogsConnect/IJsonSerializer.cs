
namespace DiscogsConnect
{
    public interface IJsonSerializer
    {        
        T DeserializeObject<T>(string json);     
    }
}
