using DapperFilterExtensions.Data;
using DapperFilterExtensions.Filtering;
using Data.Database.Dapper.Filters;
using Data.Database.Dapper.Interfaces.Gateways;
using Data.Models;

namespace Data.Database.Dapper.Gateways
{
    internal class ImagesGateway : Gateway<ImageData, ImageDataFilter>, IImagesGateway
    {
        public ImagesGateway(IDbConnectionFactory connectionFactory, IPredicateFactory predicateFactory)
            : base(connectionFactory, predicateFactory)
        {
        }
    }
}