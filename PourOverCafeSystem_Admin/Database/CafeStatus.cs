using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PourOverCafeSystem_Admin.Database
{
    public class CafeStatus
    {
        public int Id { get; set; }
        public bool IsClosed { get; set; }
    }
}
