using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>
/// Profile for mapping between WebApi requests and Application commands/results for GetSale.
/// </summary>
public class GetSaleProfile : Profile
{
    /// <summary>
    /// Initializes mappings for GetSale operation.
    /// </summary>
    public GetSaleProfile()
    {
        CreateMap<Guid, GetSaleCommand>()
            .ConstructUsing(id => new GetSaleCommand(id));

        CreateMap<GetSaleResult, GetSaleResponse>();
        CreateMap<GetSaleItemResult, GetSaleItemResponse>();
    }
}
