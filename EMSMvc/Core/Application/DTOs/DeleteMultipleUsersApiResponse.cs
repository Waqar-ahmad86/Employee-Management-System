namespace EMSMvc.Core.Application.DTOs
{
    public class DeleteMultipleUsersApiResponse : ApiResponseWithMessage
    {
        public int DeletedCount { get; set; }
        public List<string>? NotFoundIds { get; set; }
        public List<string>? Errors { get; set; }
    }
}
