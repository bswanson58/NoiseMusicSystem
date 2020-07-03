namespace Album4Matter.Models {
    public interface IKeyMaker {
        int     MakeKey();
    }

    public static class KeyMaker {
        private static IKeyMaker    mKeyMaker;

        public  static int          RootKey = 0;

        public static IKeyMaker Master {
            get => mKeyMaker ?? ( mKeyMaker = new KeyMaster());
            set => mKeyMaker = value;
        }
    }

    class KeyMaster : IKeyMaker {
        private static int  mNextKey = KeyMaker.RootKey;

        public int MakeKey() {
            mNextKey++;

            return mNextKey;
        }
    }
}
