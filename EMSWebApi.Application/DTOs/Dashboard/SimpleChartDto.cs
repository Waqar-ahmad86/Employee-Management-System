namespace EMSWebApi.Application.DTOs.Dashboard
{
    public class SimpleChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Values { get; set; } = new List<int>();
    }
}
