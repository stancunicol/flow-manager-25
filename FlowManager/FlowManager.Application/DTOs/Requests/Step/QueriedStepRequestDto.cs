using FlowManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.DTOs.Requests.Step
{
    public class QueriedStepRequestDto
    {
        public string? Name { get; set; }
        public QueryParamsDto? QueryParams { get; set; }    
    }
}