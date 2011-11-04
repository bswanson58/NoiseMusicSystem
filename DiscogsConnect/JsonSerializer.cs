
namespace DiscogsConnect
{    
    public class JsonSerializer
    {
        private IJsonSerializer _current = new NewtonsoftJsonSerializer();
        private static readonly JsonSerializer _instance = new JsonSerializer();
        
        public static IJsonSerializer Current
        {
            get
            {                
                return _instance.InnerCurrent;
            }
        }
        
        public static void SetJsonSerializer(IJsonSerializer jsonSerializer)
        {
            _instance.InnerSetApplication(jsonSerializer);
        }
                        
        public IJsonSerializer InnerCurrent
        {
            get
            {                
                return _current;
            }
        }

        public void InnerSetApplication(IJsonSerializer jsonSerializer)
        {
            _current = jsonSerializer ?? new NewtonsoftJsonSerializer();
        }        
    }
}
