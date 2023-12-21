using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;
        public PageManager pm { get; private set; }
        public IndexModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
            pm = new() { NumPerPage = 20 };
        }
        /// <summary>
        /// 分页管理
        /// </summary>
        public class PageManager
        {
            public int NumPerPage { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public int ResNum { get; set; }
            public int CurrentPage { get; set; }
            public int PreviousPage { get; set; }
            public int NextPage { get; set; }
            [Range(0, int.MaxValue)]
            public int JumpPage { get; set; }
            public void Set(int pageNum, int resNum)
            {
                StartIndex = pageNum * NumPerPage;
                NextPage = pageNum + 1;
                ResNum = resNum;
                //越界
                if (ResNum <= StartIndex)
                {
                    pageNum = ResNum / NumPerPage;
                    StartIndex = pageNum * NumPerPage;
                    NextPage = pageNum;
                }
                CurrentPage = pageNum;
                //JumpPage = pageNum;
                PreviousPage = ((pageNum - 1) < 0) ? pageNum : pageNum - 1;
                EndIndex = StartIndex + NumPerPage - 1;
            }
        }

        public IList<Book> Book { get;set; } = default!;

        public async Task OnGetAsync(int pageNum = 0)
        {
            if (_context.Book == null) return;
            pm.Set(pageNum, await _context.Book.CountAsync());
            Book = await _context.Book.Skip(pm.StartIndex).Take(pm.NumPerPage).ToListAsync();
        }
    }
}
