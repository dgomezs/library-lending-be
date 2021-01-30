using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;

namespace BusinessLayer.Services.Catalog
{
    public interface ICatalogService
    {
        Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookId);
    }
}