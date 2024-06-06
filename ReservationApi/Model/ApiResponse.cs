namespace ReservationApi.Model
{
    public class ApiResponse
    {      
       
            public dynamic Response { get; set; }
            public bool IsSuccessful { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public dynamic Data { get; set; }
        
    }
}
