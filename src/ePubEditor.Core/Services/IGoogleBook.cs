using ePubEditor.Core.Models.GoogleBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core.Services
{
    internal interface IGoogleBook
    {
        Task<Result> GetBookInfoAsync(string insb);
        Task<Result> GetBookInfoAsync(string title, string author);
    }
}
