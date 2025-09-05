using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.ApiResponse
{
    public class ApiResponseDto<T>
    {
        public T Result { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
}
