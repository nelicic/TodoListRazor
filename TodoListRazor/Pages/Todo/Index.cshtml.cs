using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TodoListRazor.Authorization;
using TodoListRazor.Data;
using TodoListRazor.Models;

namespace TodoListRazor.Pages.Todo
{
    public class IndexModel : DI_BasePageModel
    {
        public IList<TodoTask> TodoTask { get; set; } = default!;

        public IndexModel(ApplicationDbContext context, 
            IAuthorizationService authorizationService, 
            UserManager<IdentityUser> userManager) 
            : base(context, authorizationService, userManager)
        {
        }

        public async Task OnGetAsync()
        {
            var currentUserId = UserManager.GetUserId(User);

            TodoTask = await Context.TodoTask
                .Where(x => x.CreatorId == currentUserId && !x.IsCompleted).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var task = await Context.TodoTask.SingleOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return NotFound();

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, task, TodoTaskOperations.Update);

            if (!isAuthorized.Succeeded)
                return Forbid();

            task.IsCompleted = true;
            await Context.SaveChangesAsync();

            return RedirectToPage("/Todo/Index");
        }
    }
}
