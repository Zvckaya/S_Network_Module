
namespace S_Network_Module
{
    public class CPacket
    {

        public byte[] buffer { get; private set; }
        public int position { get; private set; }

        internal void copy_to(CPacket clone)
        {
            throw new NotImplementedException();
        }

        internal void record_size()
        {
            throw new NotImplementedException();
        }
    }
}