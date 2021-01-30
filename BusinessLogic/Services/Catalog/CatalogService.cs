using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Entities;
using BusinessLayer.Repositories;

namespace BusinessLayer.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IBookCopyRepository bookCopyRepository;

        public CatalogService(IBookCopyRepository bookCopyRepository)
        {
            this.bookCopyRepository = bookCopyRepository;
        }


        public Task<List<BookCopy>> GetAvailableCopiesByBookId(Guid bookId)
        {
            return bookCopyRepository.GetAvailableCopiesByBookId(bookId);
        }
    }
}