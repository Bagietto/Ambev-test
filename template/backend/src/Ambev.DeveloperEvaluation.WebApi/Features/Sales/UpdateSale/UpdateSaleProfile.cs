using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Profile for mapping between WebApi requests and Application commands/results for UpdateSale.
/// </summary>
public class UpdateSaleProfile : Profile
{
    /// <summary>
    /// Initializes mappings for UpdateSale operation.
    /// </summary>
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
        CreateMap<UpdateSaleItemRequest, UpdateSaleItemCommand>();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>();
        CreateMap<UpdateSaleItemResult, UpdateSaleItemResponse>();
    }
}
