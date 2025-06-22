namespace EMSMvc.Core.Application.DTOs
{
    public class SimpleChart
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Values { get; set; } = new List<int>();
    }
}
