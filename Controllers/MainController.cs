using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Controllers
{
    public class MainController : Controller
    {
        public async Task<IActionResult> Index()
        {
            await Task.Delay(5000);
            return Redirect("http://localhost:8080/");
        }
        public IActionResult Error() => View();
    }
}
