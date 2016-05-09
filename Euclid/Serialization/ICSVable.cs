namespace Euclid.Serialization
{
    public interface ICSVable
    {
        string ToCSV();
        void FromCSV(string text);
    }
}
