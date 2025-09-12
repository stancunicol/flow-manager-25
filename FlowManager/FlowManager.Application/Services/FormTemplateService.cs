using FlowManager.Application.Interfaces;
using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.FormTemplate;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using FlowManager.Application.Utils;
using FlowManager.Shared.DTOs.Responses.FormTemplateComponent;

namespace FlowManager.Infrastructure.Services
{
    public class FormTemplateService : IFormTemplateService
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IComponentRepository _componentRepository;
        private readonly IFlowRepository _flowRepository;

        public FormTemplateService(IFormTemplateRepository formTemplateRepository, IComponentRepository componentRepository, IFlowRepository flowRepository)
        {
            _formTemplateRepository = formTemplateRepository;
            _componentRepository = componentRepository;
            _flowRepository = flowRepository;
        }

        public async Task<FormTemplateResponseDto> DeleteFormTemplateAsync(Guid id)
        {
            FormTemplate? formTemplateToDelete = await _formTemplateRepository.GetFormTemplateByIdAsync(id);

            if (formTemplateToDelete == null)
            {
                throw new EntryNotFoundException($"FormTemplate with id {id} was not found.");
            }

            formTemplateToDelete.DeletedAt = DateTime.UtcNow;
            await _formTemplateRepository.SaveChangesAsync();

            return new FormTemplateResponseDto
            {
                Id = formTemplateToDelete.Id,
                Name = formTemplateToDelete.Name,
                Content = formTemplateToDelete.Content,
                FlowId = formTemplateToDelete.ActiveFlowId ?? Guid.Empty,
                CreatedAt = formTemplateToDelete.CreatedAt,
                UpdatedAt = formTemplateToDelete.UpdatedAt,
                DeletedAt = formTemplateToDelete.DeletedAt
            };
        }

        public async Task<PagedResponseDto<FormTemplateResponseDto>> GetAllFormTemplatesQueriedAsync(QueriedFormTemplateRequestDto payload)
        {
            QueryParams? parameters = payload.QueryParams?.ToQueryParams();
            (List<FormTemplate> data, int totalCount) = await _formTemplateRepository.GetAllFormTemplatesQueriedAsync(payload.Name, parameters);

            return new PagedResponseDto<FormTemplateResponseDto>
            {
                Data = data.Select(ft => new FormTemplateResponseDto
                {
                    Id = ft.Id,
                    Name = ft.Name,
                    Content = ft.Content,
                    FlowId = ft.ActiveFlowId,
                    Components = ft.Components.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
                    {
                        Id = ftc.ComponentId,
                    }).ToList(),
                    CreatedAt = ft.CreatedAt,
                    UpdatedAt = ft.UpdatedAt,
                    DeletedAt = ft.DeletedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount,
            };
        }

        public async Task<FormTemplateResponseDto> GetFormTemplateByIdAsync(Guid id)
        {
            FormTemplate? formTemplate = await _formTemplateRepository.GetFormTemplateByIdAsync(id);

            if (formTemplate == null)
            {
                throw new EntryNotFoundException($"FormTemplate with id {id} was not found.");
            }

            return new FormTemplateResponseDto
            {
                Id = formTemplate.Id,
                Name = formTemplate.Name,
                Content = formTemplate.Content,
                FlowId = formTemplate.ActiveFlowId ?? Guid.Empty,
                Components = formTemplate.Components.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.ComponentId,
                }).ToList(),
                CreatedAt = formTemplate.CreatedAt,
                UpdatedAt = formTemplate.UpdatedAt,
                DeletedAt = formTemplate.DeletedAt
            };
        }

        public async Task<bool> GetFormTemplateNameUnicityAsync(string formTemplateName)
        {
            return await _formTemplateRepository.GetFormTemplateNameUnicityAsync(formTemplateName);
        }

        public async Task<FormTemplateResponseDto> PatchFormTemplateAsync(Guid id, PatchFormTemplateRequestDto payload)
        {
            var formTemplateToPatch = await _formTemplateRepository.GetFormTemplateByIdAsync(id);

            if (formTemplateToPatch == null)
            {
                throw new EntryNotFoundException($"FormTemplate with id {id} was not found.");
            }

            if (payload.Name != null && !string.IsNullOrEmpty(payload.Name))
                formTemplateToPatch.Name = payload.Name;

            if (payload.Content != null && !string.IsNullOrEmpty(payload.Content))
                formTemplateToPatch.Content = payload.Content;

            if (payload.Components != null)
            {
                foreach (Guid componentId in payload.Components)
                {
                    if (await _componentRepository.GetComponentByIdAsync(componentId) == null)
                    {
                        throw new EntryNotFoundException($"Component with id {componentId} was not found.");
                    }
                }

                foreach (FormTemplateComponent component in formTemplateToPatch.Components)
                {
                    component.DeletedAt = DateTime.UtcNow;
                }

                foreach (Guid componentId in payload.Components)
                {
                    if (formTemplateToPatch.Components.Any(c => c.ComponentId == componentId))
                    {
                        FormTemplateComponent? existingComponent = await _formTemplateRepository.GetFormTemplateComponentByIdAsync(componentId, includeDeleted: true);
                        existingComponent.DeletedAt = null;
                    }
                    else
                    {
                        FormTemplateComponent newComponent = new FormTemplateComponent
                        {
                            ComponentId = componentId,
                            FormTemplateId = formTemplateToPatch.Id
                        };
                        formTemplateToPatch.Components.Add(newComponent);
                    }
                }
            }

            formTemplateToPatch.UpdatedAt = DateTime.UtcNow;

            await _formTemplateRepository.SaveChangesAsync();

            Guid? activeFlowId = null;
            if (id != null && id != Guid.Empty)
            {
                activeFlowId = id;
            }

            return new FormTemplateResponseDto
            {
                Id = formTemplateToPatch.Id,
                Name = formTemplateToPatch.Name,
                Content = formTemplateToPatch.Content,
                FlowId = formTemplateToPatch.ActiveFlowId ?? Guid.Empty,
                Components = formTemplateToPatch.Components.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
                {
                    Id = ftc.ComponentId,
                }).ToList(),
                CreatedAt = formTemplateToPatch.CreatedAt,
                UpdatedAt = formTemplateToPatch.UpdatedAt,
                DeletedAt = formTemplateToPatch.DeletedAt
            };
        }

        public async Task<FormTemplateResponseDto> PostFormTemplateAsync(PostFormTemplateRequestDto payload)
        {
            FormTemplate? existingFormTemplate = await _formTemplateRepository.GetFormTemplateByNameAsync(payload.Name);
            if(existingFormTemplate != null)
            {
                if(existingFormTemplate.DeletedAt == null)
                {
                    throw new UniqueConstraintViolationException($"FormTemplate with name {payload.Name} already exists. Please choose a different name.");
                }
                else
                {
                    throw new UniqueConstraintViolationException($"FormTemplate with name {payload.Name} was deleted. Please choose a different name.");
                }   
            }

            FormTemplate newFormTemplate = new FormTemplate
            {
                Name = payload.Name,
                Content = payload.Content,
                CreatedAt = DateTime.UtcNow,
            };

            if(payload.FlowId != null && payload.FlowId != Guid.Empty)
            {
                newFormTemplate.FormTemplateFlows.Add(new FormTemplateFlow
                {
                    FlowId = (Guid)payload.FlowId,
                    FormTemplateId = newFormTemplate.Id,
                });
            }

            newFormTemplate.Components = payload.Components.Select(c => new FormTemplateComponent
            {
                ComponentId = c,
                FormTemplateId = newFormTemplate.Id
            }).ToList();

            await _formTemplateRepository.AddAsync(newFormTemplate);

            Guid? activeFlowId = null;
            if (payload.FlowId != null && payload.FlowId != Guid.Empty)
            {
                activeFlowId = payload.FlowId;
            }

            return new FormTemplateResponseDto
            {
                Id = newFormTemplate.Id,
                Name = newFormTemplate.Name,
                Content = newFormTemplate.Content,
                FlowId = payload.FlowId ?? Guid.Empty,
                Components = newFormTemplate.Components.Where(ftc => ftc.DeletedAt == null).Select(ftc => new FormTemplateComponentResponseDto
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