using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;

namespace BusinessLayer.Services
{
    public interface IBookCatalogService
    {
        Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookId);
    }
}