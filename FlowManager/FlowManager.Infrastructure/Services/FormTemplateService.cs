using FlowManager.Application.DTOs.Requests.FormTemplate;
using FlowManager.Application.DTOs.Responses;
using FlowManager.Application.DTOs.Responses.FormTemplate;
using FlowManager.Application.DTOs.Responses.FormTemplateComponent;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Services
{
    public class FormTemplateService : IFormTemplateService
    {
        private readonly AppDbContext _dbContext;
        
        public FormTemplateService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FormTemplateResponseDto?> DeleteFormTemplateAsync(Guid id)
        {
            FormTemplate? formTemplateToDelete = await _dbContext.FormTemplates.FirstOrDefaultAsync(ft => ft.Id == id);

            if(formTemplateToDelete == null)
            {
                return null; // middleware exception later
            }
            
            formTemplateToDelete.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return new FormTemplateResponseDto
            {
                Id = formTemplateToDelete.Id,
                Name = formTemplateToDelete.Name,
                Content = formTemplateToDelete.Content,
                CreatedAt = formTemplateToDelete.CreatedAt,
                UpdatedAt = formTemplateToDelete.UpdatedAt,
                DeletedAt = formTemplateToDelete.DeletedAt
            };
        }

        public async Task<PagedResponseDto<FormTemplateResponseDto>> GetAllFormTemplatesQueriedAsync(QueriedFormTemplateRequestDto payload)
        {
            IQueryable<FormTemplate> query = _dbContext.FormTemplates
                                                       .Where(ft => ft.DeletedAt == null)
                                                       .Include(ft => ft.Components);

            if(payload.Name != null)
                query = query.Where(ft => ft.Name.Contains(payload.Name));

            int totalCount = await query.CountAsync();

            if (payload.QueryParams == null)
            {
                var data = await query.Select(ft => new FormTemplateResponseDto
                {
                    Id = ft.Id,
                    Name = ft.Name,
                    Content = ft.Content,
                    Components = ft.Components.Select(ftc => new FormTemplateComponentResponseDto
                    {
                        Id = ftc.Id,
                        Label = ftc.Label,
                        Type = ftc.Type,
                        Required = ftc.Required,
                        Properties = ftc.Properties ?? new Dictionary<string, object>()
                    }).ToList(),
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt,
                    DeletedAt = ft.DeletedAt
                }).ToListAsync();

                var pagedResponse = new PagedResponseDto<FormTemplateResponseDto>
                {
                    Data = data,
                    TotalCount = totalCount,
                    Page = 1,
                    PageSize = totalCount
                };

                return pagedResponse;
            }

            if (payload.QueryParams.SortBy != null)
            { 
                if(payload.QueryParams.SortDescending is bool SortDesc)
                    query = query.ApplySorting<FormTemplate>(payload.QueryParams.SortBy, SortDesc);
                else
                    query = query.ApplySorting<FormTemplate>(payload.QueryParams.SortBy, false);
            }

            if(payload.QueryParams.Page == null ||  payload.QueryParams.Page < 0 || 
                payload.QueryParams.PageSize == null || payload.QueryParams.PageSize < 0)
            {
                List<FormTemplate> data = await query.ToListAsync();
                return new PagedResponseDto<FormTemplateResponseDto>
                {
                    Data = data.Select(ft => new FormTemplateResponseDto
                    {
                        Id = ft.Id,
                        Name = ft.Name,
                        Content = ft.Content,
                        Components = ft.Components.Select(ftc => new FormTemplateComponentResponseDto
                        {
                            Id = ftc.Id,
                            Label = ftc.Label,
                            Type = ftc.Type,
                            Required = ftc.Required,
                            Properties = ftc.Properties ?? new Dictionary<string, object>()
                        }).ToList(),
                        CreatedAt = ft.CreatedAt,
                        UpdatedAt = ft.UpdatedAt,
                        DeletedAt = ft.DeletedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = 1,
                    PageSize = totalCount,
                };
            }
            else
            {
                List<FormTemplate> data = await query.Skip((int)payload.QueryParams.PageSize * ((int)payload.QueryParams.Page - 1))
                    .Take((int)payload.QueryParams.PageSize).ToListAsync();
                return new PagedResponseDto<FormTemplateResponseDto>
                {
                    Data = data.Select(ft => new FormTemplateResponseDto
                    {
                        Id = ft.Id,
                        Name = ft.Name,
                        Content = ft.Content,
                        Components = ft.Components.Select(ftc => new FormTemplateComponentResponseDto
                        {
                            Id = ftc.Id,
                            Label = ftc.Label,
                            Type = ftc.Type,
                            Required = ftc.Required,
                            Properties = ftc.Properties ?? new Dictionary<string, object>()
                        }).ToList(),
                        CreatedAt = ft.CreatedAt,
                        UpdatedAt = ft.UpdatedAt,
                        DeletedAt = ft.DeletedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = payload.QueryParams.Page ?? 1,
                    PageSize = payload.QueryParams.PageSize ?? totalCount,
                };
            }
        }

        public async Task<FormTemplateResponseDto?> GetFormTemplateByIdAsync(Guid id)
        {
            FormTemplate? formTemplate = await _dbContext.FormTemplates
                .Include(ft => ft.Components)
                .FirstOrDefaultAsync(ft => ft.Id == id && ft.DeletedAt == null);

            if(formTemplate == null)
            {
                return null; // middleware exception later
            }

            return new FormTemplateResponseDto
            {
                Id = formTemplate.Id,
                Name = formTemplate.Name,
                Content = formTemplate.Content,
                Components = formTemplate.Components.Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.Id,
                    Label = ftc.Label,
                    Type = ftc.Type,
                    Required = ftc.Required,
                    Properties = ftc.Properties ?? new Dictionary<string, object>()
                }).ToList(),
                CreatedAt = formTemplate.CreatedAt,
                UpdatedAt = formTemplate.UpdatedAt,
                DeletedAt = formTemplate.DeletedAt
            };
        }

        public async Task<FormTemplateResponseDto?> PatchFormTemplateAsync(PatchFormTemplateRequestDto payload)
        {
            var formTemplateToPatch = await _dbContext.FormTemplates.FirstOrDefaultAsync(ft => ft.Id == payload.Id && ft.DeletedAt == null);
            if(formTemplateToPatch == null)
            {
                return null; // middleware exception later
            }

            PatchHelper.PatchFrom<PatchFormTemplateRequestDto, FormTemplate>(formTemplateToPatch, payload);
            formTemplateToPatch.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new FormTemplateResponseDto
            {
                Id = formTemplateToPatch.Id,
                Name = formTemplateToPatch.Name,
                Content = formTemplateToPatch.Content,
                Components = formTemplateToPatch.Components.Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.Id,
                }).ToList(),
                CreatedAt = formTemplateToPatch.CreatedAt,
                UpdatedAt = formTemplateToPatch.UpdatedAt,
                DeletedAt = formTemplateToPatch.DeletedAt
            };
        }

        public async Task<FormTemplateResponseDto> PostFormTemplateAsync(PostFormTemplateRequestDto payload)
        {
            if(await _dbContext.FormTemplates.FirstOrDefaultAsync(ft => ft.Name == payload.Name) != null)
            {
                return null; // middleware exception later
            }

            FormTemplate newFormTemplate = new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = payload.Name,
                Content = payload.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Components = payload.Components.Select(c => new FormTemplateComponent
                {
                    Id = Guid.NewGuid()
                }).ToList()
            };
            _dbContext.FormTemplates.Add(newFormTemplate);
            await _dbContext.SaveChangesAsync();
            return new FormTemplateResponseDto
            {
                Id = newFormTemplate.Id,
                Name = newFormTemplate.Name,
                Content = newFormTemplate.Content,
                Components = newFormTemplate.Components.Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.Id
                }).ToList(),
                CreatedAt = newFormTemplate.CreatedAt,
                UpdatedAt = newFormTemplate.UpdatedAt,
                DeletedAt = newFormTemplate.DeletedAt
            };
        }
    }
}
