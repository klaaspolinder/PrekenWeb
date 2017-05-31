using AutoMapper;
using Business.Models;
using Business.Services.Interfaces;
using Data.Database.Dapper.Interfaces.Gateways;
using Data.Database.Dapper.Models;

namespace Business.Services
{
    internal class BooksService : Service<Book, BookFilter, BookData, BookDataFilter>, IBooksService
    {
        public BooksService(IMapper mapper, IBooksGateway gateway)
            : base(mapper, gateway)
        {
        }
    }
}