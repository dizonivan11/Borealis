namespace Borealis.Net
{
    public class Encodable
    {
        public string Decoded { get; set; }
        public string Encoded { get; set; }

        public Encodable(string decoded, string encoded) {
            Decoded = decoded;
            Encoded = encoded;
        }
    }
}
